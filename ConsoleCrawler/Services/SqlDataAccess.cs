using Dapper;
using Entities.Models;
using System.Data;
using System.Data.SqlClient;

namespace DataAccessLibrary;

/// <summary>
/// Write your Querys
/// </summary>
public class ResultData
{
    private readonly SqlDataAccess _db;
    public ResultData()
    {
        _db = new SqlDataAccess();
    }
    public Task<List<VisitedUrl>> GetVisitedUrl()
    {
        string sql = "select * from dbo.VisitedUrls";
        return _db.LoadData<VisitedUrl, dynamic>(sql, new { });
    }
    public Task InsertVisitedUrl(VisitedUrl url)
    {
        string sql = "INSERT INTO dbo.VisitedUrls (Id, Url) VALUES (@Id, @Url)";
        return _db.SaveData(sql, url);
    }
    public Task DeleteVisitedUrl(VisitedUrl url)
    {
        string sql = $"DELETE FROM dbo.VisitedUrls WHERE Id = @Id";
        return _db.SaveData(sql, url);
    }
}

/// <summary>
/// Makes the connection to the DB.
/// </summary>
public class SqlDataAccess
{
    public async Task<List<T>> LoadData<T, U>(string sql, U parameters) // Makes the Loading Query towards SQL.
    {
        using (IDbConnection connection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Crawler;Integrated Security=True;Connect Timeout=30;Encrypt=False;"))
        {
            var data = await connection.QueryAsync<T>(sql, parameters);

            return data.ToList();
        }
    }
    public async Task SaveData<T>(string sql, T parameters) // Makes the Execute towards SQL.
    {
        using (IDbConnection connection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Crawler;Integrated Security=True;Connect Timeout=30;Encrypt=False;"))
        {
            var data = await connection.ExecuteAsync(sql, parameters);
        }
    }
}
