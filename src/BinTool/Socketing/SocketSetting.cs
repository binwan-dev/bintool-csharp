using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinTool.Socketing
{
    public class SocketSetting
    {
        public bool EnableReConnect { get; set; } = false;

        public int ReconnectBaseIntervalSecond { get; set; } = 3;
        
        public int ReconnectMaxIntervalSecond { get; set; } = 60;

        public int SendBufferSize { get; set; } = 1024 * 1024;

        public int ReceiveBufferSize { get; set; } = 1024 * 1024;

        public int SendTimeoutSeconds { get; set; } = 2;

        public int ReceiveTimeoutSeconds { get; set; } = 2;
    }

}
