using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

namespace BinTool.Socketing
{
    public class TcpConnection
    {
        private readonly SocketSetting _setting;
        private readonly ILogger<TcpConnection> _log;
        private readonly ConcurrentQueue<ArraySegment<byte>> _sendQueue;
        private readonly SocketAsyncEventArgs _sendSocketArgs;
        private readonly SocketAsyncEventArgs _receiveSocketArgs;
        private readonly Action<byte[],TcpConnection> _onMessageReceived;
        private readonly Action<TcpConnection, SocketError> _onConnectionClosed;
        private readonly EndPoint _remoteEndPoint;
        private readonly ConcurrentQueue<byte[]> _receiveQueue = new();

        private Socket? _socket;
        private int _sending;
        private int _receiving;
        private bool _disposed;
        private int _handingReceiveData;

        public TcpConnection(Socket socket, SocketSetting? setting, ILogger<TcpConnection> log, Action<byte[],TcpConnection> onMessageReceived, Action<TcpConnection, SocketError> onConnectionClosed)
        {
            _socket = socket.NotNull("The socket cannot be null!");
            _setting = setting ?? new SocketSetting();
            _log = log;
            _onMessageReceived = onMessageReceived.NotNull("The DataReceived action cannot be null!");
            _onConnectionClosed = onConnectionClosed.NotNull("The ConnectionClosed action cannot be null!");
            _socket.SendTimeout = setting!.SendTimeoutSeconds;
            _socket.ReceiveTimeout = setting.ReceiveTimeoutSeconds;

            _sendQueue = new ConcurrentQueue<ArraySegment<byte>>();
            _sendSocketArgs = new SocketAsyncEventArgs();
            _sendSocketArgs.Completed += SendCompleted;
            _receiveSocketArgs = new SocketAsyncEventArgs();
            _receiveSocketArgs.SetBuffer(new byte[_setting.ReceiveBufferSize], 0, _setting.ReceiveBufferSize);
            _receiveSocketArgs.Completed += ReceiveCompleted;

            if (!TryCloneRemoteEndPoint(_socket.RemoteEndPoint, out _remoteEndPoint!))
                _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            Task.Run(TryReceive);
        }

        internal Socket? Socket => _socket;

        public bool Connected => _socket?.Connected ?? false;

        public EndPoint RemoteEndPoint => _remoteEndPoint;

        public void QueueMessage(byte[] data)
        {
            data.NotNull("The data cannot be null!")
            .CheckLength(1, _setting.SendBufferSize, $"The messsage length out of limit! MessageCount: {data.Length}, AllowLength: 1-{_setting.SendBufferSize}");

            _sendQueue.Enqueue(new ArraySegment<byte>(data));

            TrySend();
        }

        #region Send

        private void TrySend()
        {
            if (Interlocked.CompareExchange(ref _sending, 1, 0) != 0)
            {
                return;
            }

            Task.Run(Send);
        }

        private void Send()
        {
            try
            {
                while (true)
                {
                    if (_socket == null || !_socket.Connected)
                    {
                        _log.LogWarning("Send process stop! socket connection was disconnected!");
                        return;
                    }

                    if (!_sendQueue.TryDequeue(out ArraySegment<byte> data))
                    {
                        break;
                    }
                    
                    _sendSocketArgs.SetBuffer(data.ToArray(), 0, data.Count);
                    if (!_socket.SendAsync(_sendSocketArgs))
                    {
                        SendCompleted(_socket, _sendSocketArgs);
                    }
                }
            }
            catch (Exception ex)
            {
                CloseSocket($"The socket send data has an Error! Remain UnSend count: {_sendQueue.Count}, RemoteEndPoint: {RemoteEndPoint}", SocketError.ConnectionReset, ex: ex);
            }
            finally
            {
                Interlocked.Exchange(ref _sending, 0);
            }
        }

        private void SendCompleted(object? sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                CloseSocket("Send message failed!", e.SocketError);
                return;
            }

            _log.LogDebug($"Send message successes! RemoteEndPoint: {RemoteEndPoint}, Data: {e.Buffer?.ToStr()}");
        }

        #endregion

        #region Receive

        private void TryReceive()
        {
            if (Interlocked.CompareExchange(ref _receiving, 1, 0) != 0)
            {
                return;
            }

            Task.Run(Receive);
        }

        private void Receive()
        {
            try
            {
                if (_socket == null || !_socket.Connected)
                {
                    _log.LogWarning($"The socket was disconnected! stop receive data! RemoteEndPoint: {RemoteEndPoint}");
                    return;
                }

                if (!_socket.ReceiveAsync(_receiveSocketArgs))
                {
                    Task.Run(() => ReceiveCompleted(_socket, _receiveSocketArgs));
                }
            }
            catch (Exception ex)
            {
                CloseSocket($"The socket receive data has an Error!", SocketError.ConnectionReset, ex: ex);
            }
            finally
            {
                Interlocked.Exchange(ref _receiving, 0);
            }
        }

        private async void ReceiveCompleted(object? sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                CloseSocket("The socket receive has an error!", e.SocketError);
                return;
            }
            
            try
            {
                var buffer = e.Buffer.AsSpan(0, e.BytesTransferred).ToArray();
                _receiveQueue.Enqueue(buffer);
                TryHandleReceiveData();
            }
            catch (Exception ex)
            {
                _log.LogError($"The socket OnDataReceived has an error! RemoteEndPoint: {RemoteEndPoint}", ex);
                await Task.Delay(1000);
            }
            finally
            {
                Receive();
            }
        }

        #endregion
        
        #region HandleReceiveData

        private void TryHandleReceiveData()
        {
            if(Interlocked.CompareExchange(ref _handingReceiveData,1,0)!=0)return;

            Task.Run(HandleReceiveData);
        }

        private void HandleReceiveData()
        {
            try
            {
                while (_receiveQueue.TryDequeue(out var buffer))
                {
                    _onMessageReceived.Invoke(buffer, this);
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, $"Handle receive data has an exception! Will be 1 seconds retrying! RemoteEndPoint[{RemoteEndPoint}]");
                Thread.Sleep(1000);
                TryHandleReceiveData();
            }
            finally
            {
                Interlocked.Exchange(ref _handingReceiveData, 0);
            }
        }
        #endregion

        public void CloseSocket(string message, SocketError error = SocketError.Success, Exception? ex = null)
        {
            if (error != SocketError.Success)
            {
                message += $" SocketError: {error.ToString()}";
            }
            if (ex != null)
            {
                _log.LogError(message, ex);
            }

            if (_socket == null)
            {
                return;
            }

            try
            {
                Dispose();
            }
            catch (Exception shutdownEx)
            {
                _log.LogError($"The socket close has an error! RemoteEndPoint: {RemoteEndPoint}", shutdownEx);
            }
            finally
            {
                _onConnectionClosed(this, error);
            }
        }

        public void Dispose()
        {
             if (_disposed)
            {
                return;
            }

            _disposed = true;

            if(_socket==null)return;
            _socket.Close();
            _socket.Dispose();
        }

        private bool TryCloneRemoteEndPoint(EndPoint endPoint, out EndPoint? remoteEndPoint)
        {
            remoteEndPoint = null;
            try
            {
                if (endPoint is IPEndPoint ipEndPoint)
                {
                    var point = ipEndPoint.ToString().Split(':');
                    remoteEndPoint = new IPEndPoint(IPAddress.Parse(point[0]), point[1].ToInteger());
                    return true;
                }

                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                _log.LogError(e, $"CloneRemoteEndPoint has failed!");
                return false;
            }
        }

    }
}
