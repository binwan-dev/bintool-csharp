namespace BinTool.Dapper;

public class DbContextOptionsBuilder
{
    private static IDictionary<string, DapperOptions> _opotions;
    
    public DbContextOptionsBuilder()
    {
        _opotions = new Dictionary<string, DapperOptions>();
    }

    public DbContextOptionsBuilder UseSqlServer<TContext>(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        var options = new DapperOptions() {ConnectString = connectionString, ContextType = typeof(TContext)};
        _opotions.Add(options.ContextType.FullName, options);
        return this;
    }

    public DbContextOptionsBuilder UseSqlite<TContext>(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        var options = new DapperOptions() {ConnectString = connectionString, ContextType = typeof(TContext)};
        _opotions.Add(options.ContextType.FullName, options);
        return this;
    }

    internal static DapperOptions? GetOptions(Type type)
    {
        _opotions.TryGetValue(type.FullName, out var options);
        return options;
    }
}
