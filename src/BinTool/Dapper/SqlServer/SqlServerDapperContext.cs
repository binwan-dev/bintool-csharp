using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Logging;
using ZstdSharp.Unsafe;

namespace BinTool.Dapper.SqlServer;

public abstract class SqlServerDapperContext:DapperContext
{
    protected override async Task<PagedData<T>> DoQueryPagedDataAsync<T>(string sql, object? param = null, string orderByField="", int pageSize = 20, int pageNumber = 1)
    {
        if (string.IsNullOrWhiteSpace(orderByField))
        {
            throw new InvalidOperationException("SqlServer page must be set OrderByField");
        }
        
        var totalRow = await DbConnection.QueryFirstAsync<int>($"select count(0) from ({sql}) as t",param);

        var start = (pageNumber - 1) * pageSize + 1;
        var end = start + pageSize - 1;
        sql =
            $"select * from (select *, row_number() over (order by {orderByField}) as RowIndex from ({sql}) as t) as t where t.RowIndex between {start} and {end}";
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug(sql);
        }
        var data = await DbConnection.QueryAsync<T>(sql, param);

        return new PagedData<T>()
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPage = totalRow % pageSize == 0 ? totalRow / pageSize : totalRow / pageSize + 1,
            TotalRow = totalRow,
            Data = data.ToList()
        };
    }

    protected override DbConnection CreateConnection() =>
        new System.Data.SqlClient.SqlConnection(Options.ConnectString);
}