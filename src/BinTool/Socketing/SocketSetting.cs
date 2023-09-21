using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinTool.Socketing
{
    public class SocketSetting
    {
        public bool EnableReConnect { get; set; } = true;

        public int ReConnectMaxTimes { get; set; } = 3;

        public int SendBufferSize { get; set; } = 1024 * 1024;

        public int ReceiveBufferSize { get; set; } = 1024 * 1024;
    }

}
