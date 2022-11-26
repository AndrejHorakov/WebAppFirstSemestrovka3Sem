using System.Data.SqlClient;
using System.Reflection;

namespace NetConsoleApp.DataBase;

internal class DataBaseForInstances
{
    private readonly string _connectionString;
    private const string DbName = "WebAppBookin";
    private readonly string _tableName;

    public DataBaseForInstances(string tableName)
    {
        _connectionString = @$"Data Source=LAPTOP-FGUJ2MHE;Initial Catalog={DbName};Integrated Security=True";
        _tableName = tableName;
    }
  

    public IEnumerable<T> Select<T>() where T : class
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        var query = $"select * from {_tableName}";
        var cmd = new SqlCommand(query, connection);
        using var reader = cmd.ExecuteReader();
        
        if (!reader.HasRows || !reader.Read()) yield break;
        
        var ctor = typeof(T).GetConstructor(
            Enumerable.Range(0, reader.FieldCount)
                .Select(reader.GetFieldType)
                .ToArray());

        if (ctor is null) yield break;

        var parameters = Enumerable.Range(0, reader.FieldCount)
            .Select(reader.GetValue)
            .ToArray();

        yield return (T)ctor.Invoke(parameters);

        while (reader.Read())
        {
            parameters = Enumerable.Range(0, reader.FieldCount)
                .Select(reader.GetValue)
                .ToArray();

            yield return (T)ctor.Invoke(parameters);
        }
    }

    public void Insert<T>(T instance)
    {
        var properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(p => typeof(T).GetProperty(p.Name)?
                .GetValue(instance) ?? string.Empty);
        var query = $"insert into {_tableName} values ({string.Join(", ", properties)})";
        ExecuteNonQuery(query);
    }
    
    public void Delete(int? id = null)
    {
        var query = $"delete from {_tableName}";
        query += id is not null ? $"where Id={id}" : "";
        
        ExecuteNonQuery(query);
    }

    public void Update(string field, string value, int? id = null)
    {
        if (id is not null)
            ExecuteNonQuery($"update {_tableName} set {field}={value} where Id={id}");
    }


    private void ExecuteNonQuery(string query)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        var cmd = new SqlCommand(query, connection);
        cmd.ExecuteNonQuery();
    }
}