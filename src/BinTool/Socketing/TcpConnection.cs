using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
        private readonly Action<byte[]> _onDataReceived;
        private readonly Action<TcpConnection, SocketError> _onConnectionClosed;

        private Socket _socket;
        private int _sending = 0;
        private int _receiving = 0;
        private bool _disposed = false;

        public TcpConnection(Socket socket, SocketSetting? setting, ILogger<TcpConnection> log, Action<byte[]> onDataReceived, Action<TcpConnection, SocketError> onConnectionClosed)
        {
            _socket = socket.NotNull("The socket cannot be null!");
            _setting = setting ?? new SocketSetting();
            _log = log;
            _onDataReceived = onDataReceived.NotNull("The DataReceived action cannot be null!");
            _onConnectionClosed = onConnectionClosed.NotNull("The ConnectionClosed action cannot be null!");

            _sendQueue = new ConcurrentQueue<ArraySegment<byte>>();
            _sendSocketArgs = new SocketAsyncEventArgs();
            _sendSocketArgs.Completed += SendCompleted;
            _receiveSocketArgs = new SocketAsyncEventArgs();
            _receiveSocketArgs.SetBuffer(new byte[_setting.ReceiveBufferSize], 0, _setting.ReceiveBufferSize);
            _receiveSocketArgs.Completed += ReceiveCompleted;

            Task.Run(TryReceive);
        }

        internal Socket Socket => _socket;

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

            try
            {
                Send();
            }
            catch (Exception ex)
            {
                _log.LogError($"The socket send data has an Error! Remain unsend count: {_sendQueue.Count}, RemoteEndPoint: {_socket.RemoteEndPoint?.ToString()}", ex);
            }
            finally
            {
                Interlocked.Exchange(ref _sending, 0);
            }
        }

        private void Send()
        {
            while (true)
            {
                if (!_socket.Connected)
                {
                    _log.LogWarning("Send process stop! socket connection was disconnected!");
                }

                if (!_sendQueue.TryDequeue(out ArraySegment<byte> data))
                {
                    break;
                }

                _sendSocketArgs.SetBuffer(data.ToArray(), 0, data.Count);
                if (!_socket.SendAsync(_sendSocketArgs))
                {
                    Task.Run(() => SendCompleted(_socket, _sendSocketArgs));
                }
            }
        }

        private void SendCompleted(object? sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                CloseSocket("Send message failed!", e.SocketError);
                return;
            }

            _log.LogDebug($"Send message successed! RemoteEndPoint: {_socket.RemoteEndPoint?.ToString()}, Data: {e.Buffer?.ToStr()}");
        }

        #endregion

        #region Receive

        private void TryReceive()
        {
            if (Interlocked.CompareExchange(ref _receiving, 1, 0) != 0)
            {
                return;
            }

            try
            {
                Receive();
            }
            catch (Exception ex)
            {
                CloseSocket($"The socket receive data has an Error!", ex: ex);
            }
            finally
            {
                Interlocked.Exchange(ref _receiving, 0);
            }
        }

        private void Receive()
        {
            if (!_socket.Connected)
            {
                _log.LogWarning($"The socket was disconnected! stop receive data! RemoteEndPoint: {_socket.RemoteEndPoint?.ToString()}");
                return;
            }

            if (!_socket.ReceiveAsync(_receiveSocketArgs))
            {
                Task.Run(() => ReceiveCompleted(_socket, _receiveSocketArgs));
            }
        }

        private void ReceiveCompleted(object? sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                CloseSocket("The socket receive has an error!", e.SocketError);
                return;
            }

            var buffer = new ArraySegment<byte>(e.Buffer ?? new byte[0], 0, e.BytesTransferred);
            try
            {
                Task.Run(() => { _onDataReceived(buffer.ToArray()); });
            }
            catch (Exception ex)
            {
                _log.LogError($"The socket OnDataReceived has an error! RemoteEndPoint: {_socket.RemoteEndPoint?.ToString()}, Data: {buffer.ToArray().ToStr()}", ex);
            }
            finally
            {
                Receive();
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

            var remoteEndpoint = _socket.RemoteEndPoint;
            try
            {
                Dispose();
            }
            catch (Exception shutdownEx)
            {
                _log.LogError($"The socket close has an error! RemoteEndPoint: {remoteEndpoint?.ToString()}", shutdownEx);
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

            _socket.Close();
            _socket.Dispose();
        }

    }
}
