using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace BinTool.Socketing
{
    public class ClientSocket
    {
        private Socket? _socket;
        private readonly string _address;
        private readonly int _port;
        private readonly SocketAsyncEventArgs _connectArgs;
        private readonly IList<ITcpConnectionEventListener> _connectionEventListener;
        private readonly SocketSetting _setting;
        private readonly Action<byte[], TcpConnection> _onMessageReceived;
        private readonly AutoResetEvent _connectWaitEvent;
        private TcpConnection? _tcpConnection;
        private readonly ILogger<TcpConnection> _tcpConnectionLog;
        private int _reconnecting;
        private int _reconnectInterval;

        public ClientSocket(string ip, int port, Action<byte[], TcpConnection> onMessageReceived,
            ILogger<TcpConnection> tcpConnectionLog, SocketSetting? setting = null)
        {
            _address = ip.NotNull("IP 地址不能为空").IPv4("IP 必须是 Ipv4 协议地址");
            _port = port.NotNull("Port 端口不能为空").Port("Port 端口必须是 0 - 65535 区间的整数");
            _onMessageReceived = onMessageReceived.NotNull("数据处理函数不能为空！");
            _setting = setting ?? new SocketSetting();
            _tcpConnectionLog = tcpConnectionLog;

            _connectionEventListener = new List<ITcpConnectionEventListener>();
            _connectArgs = new SocketAsyncEventArgs();
            _connectArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(_address), _port);
            _connectArgs.Completed += HandleConnectCompleted;
            _connectWaitEvent = new AutoResetEvent(false);
            _reconnectInterval = _setting.ReconnectBaseIntervalMillsecond;
        }

        public EndPoint RemoteEndPoint => _connectArgs.RemoteEndPoint!;

        public string Id { get; } = Guid.NewGuid().ToString();

        internal Socket Socket => _socket!;

        public void Connect(int timeoutMillSeconds = 5000)
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var result = _socket.BeginConnect(_connectArgs.RemoteEndPoint, null, null);
                result.AsyncWaitHandle.WaitOne(timeoutMillSeconds);
                if (!_socket.Connected)
                {
                    _socket.Close();
                    _socket.Dispose();
                    _connectArgs.SocketError = SocketError.TimedOut;
                }
                else
                {
                    _connectArgs.SocketError = SocketError.Success;
                }
                Task.Run(() => HandleConnectCompleted(_socket, _connectArgs));
            }
            catch(Exception ex) 
            {
                _tcpConnectionLog.LogError(ex, $"Connect has an error! RemoteEndPoint: {_connectArgs.RemoteEndPoint}");
                Task.Run(() => HandleConnectCompleted(_socket, _connectArgs));
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

        protected void HandleConnectCompleted(object? sender, SocketAsyncEventArgs e)
        {
            _connectWaitEvent.Set();
            if (e.SocketError != SocketError.Success || !_socket!.Connected)
            {
                OnConnectionFailed(e.SocketError);
                return;
            }

            _reconnectInterval = _setting.ReconnectBaseIntervalMillsecond;
            
            try
            {
                _tcpConnection = new TcpConnection(_socket, _setting, _tcpConnectionLog, _onMessageReceived, OnConnectionClosed);
                foreach (var connectionEvent in _connectionEventListener)
                {
                    try
                    {
                        connectionEvent.ConnectEstablished(_tcpConnection);
                    }
                    catch (Exception ex)
                    {
                        _tcpConnectionLog.LogError(ex,
                            $"Invoke ConnectEstablished Event failed! Target[{connectionEvent.GetType().FullName}]");
                    }
                }
            }
            catch (Exception exception)
            {
                _tcpConnectionLog.LogError(exception, $"Create TcpConnection failed!");
            }
        }

        private void OnConnectionFailed(SocketError socketError)
        {
            foreach (var connectionEvent in _connectionEventListener)
            {
                try
                {
                    connectionEvent.ConnectFailed(RemoteEndPoint, socketError);
                }
                catch (Exception e)
                {
                    _tcpConnectionLog.LogError(e,$"Invoke ConnectFailed Event failed! Target[{connectionEvent.GetType().FullName}] SocketError[{socketError}]");
                }
            }

            TryReConnect();
        }

        private void OnConnectionClosed(TcpConnection connection, SocketError socketError)
        {
            foreach (var connectionEvent in _connectionEventListener)
            {
                try
                {
                    connectionEvent.ConnectClosed(connection, socketError);
                }
                catch (Exception e)
                {
                    _tcpConnectionLog.LogError(e, $"Invoke ConnectionClosed Event failed! Target[{connectionEvent.GetType().FullName}] SocketError[{socketError}]");
                }
            }

            if(socketError != SocketError.Success) TryReConnect();
            _tcpConnectionLog.LogWarning($"Socket has closed! RemoteIP[{_connectArgs.RemoteEndPoint}] SocketError[{socketError}]");
        }

        private void TryReConnect()
        {
            if (!_setting.EnableReConnect || Interlocked.CompareExchange(ref _reconnecting, 1, 0) != 0)
            {
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    _tcpConnectionLog.LogDebug($"Reconnect will be {_reconnectInterval} second!");
                    await Task.Delay(_reconnectInterval);
                    _reconnectInterval += _reconnectInterval;
                    _reconnectInterval = _reconnectInterval > _setting.ReconnectMaxIntervalMillsecond
                        ? _setting.ReconnectMaxIntervalMillsecond
                        : _reconnectInterval;

                    _tcpConnectionLog.LogDebug($"Reconnecting... RemoteEndPoint[{_address}:{_port}]");
                    Connect();
                }
                catch (Exception ex)
                {
                    _tcpConnectionLog.LogDebug(ex, $"Reconnect failed! RemoteEndPoint[{_address}:{_port}]");
                }
                finally
                {
                    Interlocked.Exchange(ref _reconnecting, 0);
                }
            });
        }
    }
}
