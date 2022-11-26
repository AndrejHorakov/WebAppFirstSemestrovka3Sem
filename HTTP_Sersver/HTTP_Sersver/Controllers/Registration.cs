using NetConsoleApp.Attributes;
using NetConsoleApp.Hashing;
using NetConsoleApp.Models;
using NetConsoleApp.ORM;

namespace NetConsoleApp.Controllers;

[HttpController("registration")]
public class Registration
{
    [HttpGET("")]
    public static byte[] RegistrationPage()
    {
        return File.ReadAllBytes(Path.GetFullPath("sites/registration/index.sbnhtml"));
    }
    
    [HttpPOST("")]
    public static (bool, User?) SaveUser(string login, string password, string againPassword, string nickname)
    {
        var db = new OrmAccountDao();
        
        if (db.GetAll().Any(u => u.Login == login) || password != againPassword)
        {
            return (false, null);
        }
        
        db.Insert(login, Hasher.Hash(password), nickname);
        var user = db.GetAll().FirstOrDefault(u => u.Login == login);

        return (true, user);
    }
}