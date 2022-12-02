using System.Text;
using NetConsoleApp.Attributes;
using NetConsoleApp.DataBase;
using NetConsoleApp.Models;
using NetConsoleApp.ORM;
using NetConsoleApp.SessionLogic;
using Scriban;
using Cookie = NetConsoleApp.Models.Cookie;

namespace NetConsoleApp.Controllers;

[HttpController("")]
public class Application
{
    private const string CookiesTableName = "[dbo].[Cookies]";

    [HttpGET("")]
    public static void BeginPage()
    {
        
    }

    [HttpPOST("")]
    public static void ExitProfile(string id)
    {
        
    }

    public static byte[] GetChangedPage<T>(string id, T model, string page)
    {
        var user = GetById(id);
        var flag = user is not null;
        var tpl = Template.Parse(page);
        var res = tpl!.Render(new {model = model, flag = flag});
        return Encoding.UTF8.GetBytes(res);
    }
    
    public static User? GetById(string id)
    {
        if (id is "" or null) return null;
        var guid = new Guid(id);
        var session = SessionProvider.GetSessionInfo(guid);
        int usId;
        if (session is not null)
        {
            usId = session.UserId;
        }
        else
        {
            var cookie = new DataBaseForInstances(CookiesTableName).Select<Cookie>().FirstOrDefault(cookie => cookie.Guid == id);
            if (cookie == null)
                return null;
            usId = cookie.IdUs;
        }
        return new OrmAccountRepository().GetById(usId);
    }
}