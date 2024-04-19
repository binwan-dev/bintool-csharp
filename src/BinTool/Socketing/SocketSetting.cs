namespace BinTool.Socketing
{
    public class SocketSetting
    {
        public bool EnableReConnect { get; set; } = false;

        public int ReconnectBaseIntervalMillsecond { get; set; } = 3000;
        
        public int ReconnectMaxIntervalMillsecond { get; set; } = 60000;

        public int SendBufferSize { get; set; } = 1024 * 1024;

        public int ReceiveBufferSize { get; set; } = 1024 * 1024;

        public int SendTimeoutSeconds { get; set; } = 2;

        public int ReceiveTimeoutSeconds { get; set; } = 2;
    }

}
