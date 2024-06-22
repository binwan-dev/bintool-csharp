using System;
using System.Collections.Generic;
using System.Text;

namespace BinTool.Socketing
{
    public interface IDataReceiveHandler
    {
        void HandleData(byte[] buffer, TcpConnection connection);
    }
}
