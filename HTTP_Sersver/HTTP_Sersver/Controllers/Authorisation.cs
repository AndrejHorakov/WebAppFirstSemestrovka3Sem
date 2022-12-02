using System.Security.Cryptography;
using System.Text;
using NetConsoleApp.Attributes;
using NetConsoleApp.DataBase;
using NetConsoleApp.Hashing;
using NetConsoleApp.Models;
using NetConsoleApp.ORM;

namespace NetConsoleApp.Controllers;

[HttpController("authorisation")]
public class Authorisation
{
    private const string CookiesTableName = "[dbo].[Cookies]";
    
    [HttpGET("")]
    public static byte[] AuthorisationPage()
    {
        return File.ReadAllBytes(Path.GetFullPath("sites/authorisation/index.sbnhtml"));
    }
    
    [HttpPOST("")]
    public static (bool, User?, bool) CheckAuthorisation(string login, string password, string remeberMe = "")
    {
        var db = new OrmAccountDao();
        var user = db.GetAll().FirstOrDefault(u => u.Login == login);
        if (user is null || Hasher.Hash(password) != user.Password)
            return (false, null, false);
        Guid result;
        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(user.Nickname + user.Password));
            result = new Guid(hash);
        }

        var dbCookie = new DataBaseForInstances(CookiesTableName);
        if (!dbCookie.Select<Cookie>().Any(c => c.Guid == result.ToString()))
            dbCookie.Insert(new Cookie(result.ToString(), user.Id));
        return (true, user, String.Equals(remeberMe, "on"));
    }
}