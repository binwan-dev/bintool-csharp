using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using IPAddress = System.Net.IPAddress;

namespace BinTool.Socketing;

public class ServerSocket
{
    private Socket _socket;
    private readonly int _port;
    private readonly SocketAsyncEventArgs _acceptArgs;
    private readonly IList<ITcpConnectionEventListener> _connectionEventListener;
    private readonly SocketSetting _setting;
    private readonly Action<byte[], TcpConnection> _onDataReceived;
    private TcpConnection? _tcpConnection;
    private readonly ILogger<TcpConnection> _tcpConnectionLog;
    private int _accepting = 0;

    public ServerSocket(int port, Action<byte[], TcpConnection> onDataReceived, ILogger<TcpConnection> tcpConnectionLog,
        SocketSetting? setting = null)
    {
        _port = port.NotNull("Port 端口不能为空").Port("Port 端口必须是 0 - 65535 区间的整数");
        _onDataReceived = onDataReceived.NotNull("数据处理函数不能为空！");
        _setting = setting ?? new SocketSetting();
        _tcpConnectionLog = tcpConnectionLog;

        _connectionEventListener = new List<ITcpConnectionEventListener>();
        _acceptArgs = new SocketAsyncEventArgs();
        _acceptArgs.Completed += HandleAccpetCompleted;
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public string ID { get; } = Guid.NewGuid().ToString();

    public void Start(int backlog = 5000)
    {
        _socket.Listen(backlog);
        _socket.Bind(localEP: new IPEndPoint(IPAddress.Any, _port));
        Task.Run(TryAccept);
    }

    private void TryAccept()
    {
        if (Interlocked.CompareExchange(ref _accepting, 1, 0) != 0) return;

        try
        {
            _tcpConnectionLog.LogInformation($"socket listen for port: {_port}");

            while (true)
            {
                if (!_socket.AcceptAsync(_acceptArgs))
                {
                    Task.Run(() => HandleAccpetCompleted(_socket, _acceptArgs));
                }
            }
        }
        catch (Exception ex)
        {
            _tcpConnectionLog.LogError(ex, $"Socket accepting has an error! Port: {_port}");
        }
        finally
        {
            Interlocked.Exchange(ref _accepting, 0);
        }
    }

    public void RegisterConnectionEventListener(ITcpConnectionEventListener eventListener)
    {
        if (_connectionEventListener.Contains(eventListener))
            return;

        _connectionEventListener.Add(eventListener);
    }

    public void QueueMessage(byte[] data)
    {
        _tcpConnection?.QueueMessage(data);
    }

    protected void HandleAccpetCompleted(object? sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError != SocketError.Success || !_socket.Connected)
        {
            _tcpConnectionLog.LogError(
                $"Socket accpet has an SocketError! SocketError: {e.SocketError} SocketRemoteEndPoint: {e.RemoteEndPoint}");
            return;
        }

        _tcpConnectionLog.LogInformation($"Socket endpoint accept success! SocketRemoteEndPoint: {e.RemoteEndPoint}");
        _tcpConnection = new TcpConnection(_socket, _setting, _tcpConnectionLog, _onDataReceived, OnConnectionClosed);
        foreach (var connectionEvent in _connectionEventListener)
        {
            connectionEvent.ConnectEstablished(_tcpConnection);
        }
    }

    private void OnConnectionClosed(TcpConnection connection, SocketError socketError)
    {
        foreach (var connectionEvent in _connectionEventListener)
        {
            connectionEvent.ConnectClosed(connection, socketError);
        }
    }
}
