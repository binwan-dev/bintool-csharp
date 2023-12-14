using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace BinTool.Socketing
{
    public class ClientSocket
    {
        private Socket _socket;
        private readonly string _address;
        private readonly int _port;
        private readonly SocketAsyncEventArgs _connectArgs;
        private readonly IList<ITcpConnectionEventListener> _connectionEventListener;
        private readonly SocketSetting _setting;
        private readonly Action<byte[]> _onDataReceived;
        private readonly AutoResetEvent _connectWaitEvent;
        private TcpConnection? _tcpConnection;
        private readonly ILogger<TcpConnection> _tcpConnectionLog;
        private int _reconnecting = 0;

        public ClientSocket(string ip, int port, Action<byte[]> onDataReceived, ILogger<TcpConnection> tcpConnectionLog, SocketSetting? setting = null)
        {
            _address = ip.NotNull("IP 地址不能为空").IPv4("IP 必须是 IPv4 协议地址");
            _port = port.NotNull("Port 端口不能为空").Port("Port 端口必须是 0 - 65535 区间的整数");
            _onDataReceived = onDataReceived.NotNull("数据处理函数不能为空！");
            _setting = setting ?? new SocketSetting();
            _tcpConnectionLog = tcpConnectionLog;

            _connectionEventListener = new List<ITcpConnectionEventListener>();
            _connectArgs = new SocketAsyncEventArgs();
            _connectArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(_address), _port);
            _connectArgs.Completed += HandleConnectCompleted;
            _connectWaitEvent = new AutoResetEvent(false);
        }

        public EndPoint RemoteEndPoint => _connectArgs.RemoteEndPoint!;

        public string ID { get; } = Guid.NewGuid().ToString();

        internal Socket Socket => _socket;

        public void Connect(int timeoutMillseconds = 5000)
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var result = _socket.BeginConnect(_connectArgs.RemoteEndPoint, null, null);
                result.AsyncWaitHandle.WaitOne(timeoutMillseconds);
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
            if (e.SocketError != SocketError.Success || !_socket.Connected)
            {
                OnConnectionFailed(e.SocketError);
                return;
            }

            _tcpConnection = new TcpConnection(_socket, _setting, _tcpConnectionLog, _onDataReceived, OnConnectionClosed);
            foreach (var connectionEvent in _connectionEventListener)
            {
                connectionEvent.ConnectEstablished(_tcpConnection);
            }
        }

        private void OnConnectionFailed(SocketError socketError)
        {
            foreach (var connectionEvent in _connectionEventListener)
            {
                connectionEvent.ConnectFailed(RemoteEndPoint, socketError);
            }

            TryReConnect();
        }

        private void OnConnectionClosed(TcpConnection connection, SocketError socketError)
        {
            foreach (var connectionEvent in _connectionEventListener)
            {
                connectionEvent.ConnectClosed(connection, socketError);
            }

            if (socketError != SocketError.Shutdown && socketError != SocketError.Success)
            {
                TryReConnect();
            }
        }

        private void TryReConnect()
        {
            if (!_setting.EnableReConnect || Interlocked.CompareExchange(ref _reconnecting, 1, 0) != 0)
            {
                return;
            }

            try
            {
                _tcpConnectionLog.LogInformation($"Reconnecting... RemoteEndPoint[{_address}:{_port}]");
                Task.Run(() => { Connect(5000); });
            }
            catch (Exception ex)
            {
                _tcpConnectionLog.LogError(ex, $"Reconnect failed! RemoteEndPoint[{_address}:{_port}]");
            }
            finally
            {
                Interlocked.Exchange(ref _reconnecting, 0);
            }
        }
    }
}