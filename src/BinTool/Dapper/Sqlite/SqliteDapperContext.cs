using System.Data.Common;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;

namespace BinTool.Dapper.Sqlite;

public class SqliteDapperContext:DapperContext
{
    protected override async Task<PagedData<T>> DoQueryPagedDataAsync<T>(string sql, object? param = null, string orderByField = "", int pageSize = 20,
        int pageNumber = 1)
    {
        var pagedData = new PagedData<T>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRow = await QuerySingleAsync<int>($"select count(*) from ({sql}) as t", param)
        };
        pagedData.TotalPage = pagedData.TotalRow % pagedData.PageSize == 0
            ? pagedData.TotalRow / pagedData.PageSize
            : pagedData.TotalRow / pagedData.PageSize + 1;
        
        pagedData.Data = (await QueryAsync<T>($"{sql} limit {pageSize} offset {(pageNumber-1)*pageSize};", param)).ToList();
        return pagedData;
    }

    protected override DbConnection CreateConnection() => new SqliteConnection();
}