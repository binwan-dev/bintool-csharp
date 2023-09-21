using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BinTool.Socketing
{
    public interface ITcpConnectionEventListener
    {
        void ConnectEstablished(TcpConnection connection);

        void ConnectClosed(TcpConnection connection, SocketError error);

        void ConnectFailed(EndPoint endPoint, SocketError error);
    }

}
