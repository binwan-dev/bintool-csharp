namespace BinTool.Dapper
{
    public class DapperOptions
    {
        public string ConnectString { get; set; } = string.Empty;

        internal Type ContextType { get; set; } = null!;
    }
}
