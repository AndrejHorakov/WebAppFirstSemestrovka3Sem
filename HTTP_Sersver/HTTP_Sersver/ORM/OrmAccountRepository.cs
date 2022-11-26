using System.Data.SqlClient;
using NetConsoleApp.Models;

namespace NetConsoleApp.ORM;

public class OrmAccountRepository : IOrmAccountRepository
{
    private const string ConnectionString =
        @$"Data Source=LAPTOP-FGUJ2MHE;Initial Catalog={DbName};Integrated Security=True";

    private const string TableName = "[dbo].[Users]";
    private const string DbName = "WebAppBookin";
    private readonly Dictionary<int, User> _accounts;
    
    public OrmAccountRepository()
    {
        _accounts = new Dictionary<int, User>();
        foreach (var account in GetAll())
            _accounts.Add(account.Id, account);
    }
    
    public IEnumerable<User> GetAll()
    {
        if (_accounts.Count == 0)
        {
            var query = $"select * from {TableName}";
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();

            var cmd = new SqlCommand(query, connection);
            using var reader = cmd.ExecuteReader();
            
            if (!reader.HasRows) yield break;
            while (reader.Read())
                yield return new User(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                                reader.GetString(3));
        }
        else
            foreach (var account in _accounts.Values)
                yield return account;
    }

    public User? GetById(int id) => 
        _accounts.ContainsKey(id) ? _accounts[id] : null;

    public void Insert(User user)
    {
        var query = $"insert into {TableName} values ('{user.Nickname}', '{user.Password}')";
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();

        var cmd = new SqlCommand(query, connection);
        cmd.ExecuteNonQuery();
        _accounts.Add(user.Id, user);
    }

    public void Remove(User user)
    {
        var query = $"delete from {TableName} where Id={user.Id}";
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();

        var cmd = new SqlCommand(query);
        cmd.ExecuteNonQuery();
        _accounts.Remove(user.Id);
    }

    public void Update(User now, User will)
    {
        var query = $"update {TableName} set Login={will.Nickname}, Password={will.Password} where Id={now.Id}";

        using var connection = new SqlConnection(ConnectionString);
        connection.Open();

        var cmd = new SqlCommand(query);
        cmd.ExecuteNonQuery();
        _accounts[now.Id] = will;
    }
}