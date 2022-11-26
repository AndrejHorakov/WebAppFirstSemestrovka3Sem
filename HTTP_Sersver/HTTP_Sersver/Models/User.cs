namespace NetConsoleApp.Models;

public class User
{
    public int Id { get; }
    public string Nickname { get; }
    public string Password { get; }
    public string Login { get; }
    public int Age { get; }

    public User(int id, string nickname, string password, string login)
    {
        Id = id;
        Nickname = nickname;
        Password = password;
        Login = login;
    }
}