using NetConsoleApp.Attributes;
using NetConsoleApp.DataBase;
using NetConsoleApp.Models;

namespace NetConsoleApp.Controllers;

[HttpController("general")]
public class General
{
    private const string PostsTableName = "[dbo].[Posts]";
    private const string GeneralPage = "sites/general/page.sbnhtml";
    
    [HttpGET("")]
    public static byte[] GetGeneralPage(string id) =>
        Application.GetChangedPage(id, new DataBaseForInstances(PostsTableName).Select<Post>().ToList(),
            File.ReadAllText(Path.GetFullPath(GeneralPage)));
    
    [HttpPOST("")]
    public static byte[] UpdatePostById(string? id, string text, int idPost)
    {
        var db = new DataBaseForInstances(PostsTableName);
        db.Update("Text", text, idPost);
        return GetGeneralPage(id);
    }
}