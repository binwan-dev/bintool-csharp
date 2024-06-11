using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Dapper;
using DapperExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace BinTool.Dapper
{
    public abstract class DapperContext : IDisposable
    {
        protected DapperOptions Options;
        private ILogger<DapperContext>? _log;

        public DapperContext()
        {
            Init(DbContextOptionsBuilder.GetOptions(this.GetType()) ?? new DapperOptions());
        }

        public DapperContext(DapperOptions? options)
        {
            Init(options);
        }

        private void Init(DapperOptions? options)
        {
          options ??= new DapperOptions();
          SetOptions(options);
          if (options == null || string.IsNullOrWhiteSpace(options.ConnectString))
          {
            throw new Exception("数据库连接字符串不能为空");
          }

          Options = options;
          DbConnection = CreateConnection();
          _log = BinToolDapperExtension.Provider?.GetService<ILogger<DapperContext>>();
          _log?.LogDebug($"dapper 数据库连接开启，HashCode:{DbConnection.GetHashCode()}");
        }

        protected ILogger<DapperContext> Log => _log;

        public DbConnection DbConnection { get; private set; }

        protected virtual void SetOptions(DapperOptions options)
        {
        }

        public DbTransaction BeginTransaction()
        {
          return this.DbConnection.BeginTransaction();
        }

        public DbTransaction BeginTransaction(IsolationLevel il)
        {
          return this.DbConnection.BeginTransaction(il);
        }

        public void ChangeDatabase(string databaseName)
        {
          this.DbConnection.ChangeDatabase(databaseName);
        }

        public void Close()
        {
          this.DbConnection.Close();
        }

        public IDbCommand CreateCommand()
        {
          return (IDbCommand) this.DbConnection.CreateCommand();
        }

        public object Insert<T>(T entity, DbTransaction? transaction = null, int? commandTimeout = null) where T : class
        {
          return DbConnection.Insert(entity, transaction, commandTimeout);
        }

        public void Insert<T>(IEnumerable<T> entities, DbTransaction? transaction = null, int? commandTimeout = null)
          where T : class
        {
          this.DbConnection.Insert(entities, transaction, commandTimeout);
        }

        public bool Update<T>(T entity, DbTransaction? transaction = null, int? commandTimeout = null) where T : class
        {
          return this.DbConnection.Update<T>(entity, transaction, commandTimeout);
        }

        public bool Delete<T>(T entity, DbTransaction? transaction = null, int? commandTimeout = null) where T : class
        {
          return this.DbConnection.Delete<T>(entity, transaction, commandTimeout);
        }

        public bool Delete<T>(object predicate, DbTransaction? transaction = null, int? commandTimeout = null)
          where T : class
        {
          return this.DbConnection.Delete<object>(predicate, transaction, commandTimeout);
        }

        public int Execute(string sql, object? param = null)
        {
          return this.DbConnection.Execute(sql, param, null, new int?(), new CommandType?());
        }

        public IDataReader ExecuteReader(string sql, object? param = null)
        {
          return this.DbConnection.ExecuteReader(sql, param, null, new int?(), new CommandType?());
        }

        public object? ExecuteScalar(string sql, object? param = null)
        {
          return this.DbConnection.ExecuteScalar(sql, param, null, new int?(), new CommandType?());
        }

        public SqlMapper.GridReader QueryMultiple(string sql, object? param = null)
        {
          return this.DbConnection.QueryMultiple(sql, param, null, new int?(), new CommandType?());
        }

        public IEnumerable<object> Query(string sql, object? param = null)
        {
          return this.DbConnection.Query(sql, param, null, true, new int?(), new CommandType?());
        }

        public object QueryFirst(string sql, object? param = null)
        {
          return this.DbConnection.QueryFirst(sql, param, null, new int?(), new CommandType?());
        }

        public object? QueryFirstOrDefault(string sql, object? param = null)
        {
          return this.DbConnection.QueryFirstOrDefault(sql, param, null, new int?(), new CommandType?());
        }

        public object QuerySingle(string sql, object? param = null)
        {
          return this.DbConnection.QuerySingle(sql, param, null, new int?(), new CommandType?());
        }

        public object? QuerySingleOrDefault(string sql, object? param = null)
        {
          return this.DbConnection.QuerySingleOrDefault(sql, param, null, new int?(), new CommandType?());
        }

        public IEnumerable<T>? Query<T>(string sql, object? param = null)
        {
          return this.DbConnection.Query<T>(sql, param, null, true, new int?(), new CommandType?());
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(
          string sql,
          Func<TFirst, TSecond, TReturn> map,
          object? param = null,
          IDbTransaction? transaction = null,
          bool buffered = true,
          string splitOn = "Id",
          int? commandTimeout = null,
          CommandType? commandType = null)
        {
          return this.DbConnection.Query<TFirst, TSecond, TReturn>(sql, map, param, transaction, buffered, splitOn,
            commandTimeout, commandType);
        }

        public T QueryFirst<T>(string sql, object? param = null)
        {
          return this.DbConnection.QueryFirst<T>(sql, param, null, new int?(), new CommandType?());
        }

        public T? QueryFirstOrDefault<T>(string sql, object? param = null)
        {
          return this.DbConnection.QueryFirstOrDefault<T>(sql, param, null, new int?(),
            new CommandType?());
        }

        public T QuerySingle<T>(string sql, object? param = null)
        {
          return this.DbConnection.QuerySingle<T>(sql, param, null, new int?(), new CommandType?());
        }

        public T? QuerySingleOrDefault<T>(string sql, object? param = null)
        {
          return this.DbConnection.QuerySingleOrDefault<T>(sql, param, null, new int?(),
            new CommandType?());
        }

        public Task<int> ExecuteAsync(string sql, object? param = null)
        {
            return this.DbConnection.ExecuteAsync(sql, param, null, new int?(), new CommandType?());
        }

        public async Task<IDataReader?> ExecuteReaderAsync(string sql, object? param = null)
        {
            var reader = await this.DbConnection.ExecuteReaderAsync(sql, param, null, new int?(), new CommandType?());
            return reader;
        }

        public Task<object?> ExecuteScalarAsync(string sql, object? param = null)
        {
            return this.DbConnection.ExecuteScalarAsync(sql, param);
        }

        public Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object? param = null)
        {
            return this.DbConnection.QueryMultipleAsync(sql, param);
        }

        public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
        {
            return this.DbConnection.QueryAsync<T>(sql, param);
        }

        public Task<T> QueryFirstAsync<T>(string sql, object? param = null)
        {
            return this.DbConnection.QueryFirstAsync<T>(sql, param);
        }

        public Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null)
        {
            return DbConnection.QueryFirstOrDefaultAsync<T>(sql, param);
        }

        public Task<T> QuerySingleAsync<T>(string sql, object? param = null)
        {
            return DbConnection.QuerySingleAsync<T>(sql, param);
        }

        public Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null)
        {
            return DbConnection.QuerySingleOrDefaultAsync<T>(sql, param);
        }

        /// <summary>
        /// Get page data
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="orderByField">When use sqlserver, muse be set it.</param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public virtual Task<PagedData<T>> QueryPagedDataAsync<T>(string sql, object? param = null, string orderByField = "",
          int pageSize = 20, int pageNumber = 1)
        {
          if (pageNumber <= 0)
          {
            throw new ArgumentException($"无效的PageNumber 参数值：{pageNumber}");
          }

          if (pageSize <= 0)
          {
            throw new ArgumentException($"无效的PageSize 参数值：{pageSize}");
          }

          if (string.IsNullOrWhiteSpace(sql))
          {
            throw new ArgumentException("sql参数值不能为空");
          }

          return DoQueryPagedDataAsync<T>(sql, param, orderByField, pageSize, pageNumber);
        }

        public void Dispose()
        {
            DbConnection.Close();
            DbConnection.Dispose();

            _log?.LogDebug($"dapper 数据库连接关闭，HashCode:{this.DbConnection.GetHashCode()}");
        }

        protected abstract Task<PagedData<T>> DoQueryPagedDataAsync<T>(string sql, object? param = null,
          string orderByField = "", int pageSize = 20, int pageNumber = 1);

        protected abstract DbConnection CreateConnection();
    }
}
