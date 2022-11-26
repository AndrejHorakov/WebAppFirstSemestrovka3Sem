using NetConsoleApp.Attributes;
using NetConsoleApp.ORM;

namespace NetConsoleApp.Controllers;

[HttpController("profile")]
public class Profile
{
    private const string ProfilePage = "sites/profile/page.sbnhtml";
    
    [HttpGET("/profile")]
    public static byte[] GetProfilePage(string id) =>
        Application.GetChangedPage(id, new OrmAccountDao().GetById(Application.GetById(id)!.Id),
            File.ReadAllText(Path.GetFullPath(ProfilePage)));
    
    [HttpPOST("/profile")]
    public static byte[] UpdateProfilePage(string id, string nickname)
    {
        var db = new OrmAccountDao();
        var user = Application.GetById(id);
        db.Update("Nickname", nickname, user!.Id);
        return GetProfilePage(id);
    }
}