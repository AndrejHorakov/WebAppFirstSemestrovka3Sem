using NetConsoleApp.Models;
namespace NetConsoleApp.ORM;

public interface IOrmAccountRepository
{
    public IEnumerable<User> GetAll();
    public User? GetById(int id);
    public void Insert(User user);
    public void Remove(User user);
    public void Update(User old, User @new);
}