using NetConsoleApp.Models;

namespace NetConsoleApp.ORM;

public interface IOrmAccountDao
{
    public IEnumerable<User> GetAll();
    public User? GetById(int id);
    public void Insert(string login, string password, string nickname);
    public void Remove(int? id);
    public void Update(string field, string value, int? id);
}