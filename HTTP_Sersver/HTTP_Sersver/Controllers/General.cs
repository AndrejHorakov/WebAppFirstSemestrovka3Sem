using System.Text;
using NetConsoleApp.Attributes;
using NetConsoleApp.DataBase;
using NetConsoleApp.Models;
using Scriban;

namespace NetConsoleApp.Controllers;

[HttpController("general")]
public class General
{
    private const string PostsTableName = "[dbo].[Posts]";
    private const string GeneralPage = "sites/general/page.sbnhtml";
    
    [HttpGET("")]
    public static byte[] GetGeneralPage(string id) =>
        GetChangedPage(id, new DataBaseForInstances(PostsTableName).Select<Post>().ToList(),
            File.ReadAllText(Path.GetFullPath(GeneralPage)));
    
    [HttpPOST("edit")]
    public static void UpdatePostById(int idPost, string text, string id)
    {
        var db = new DataBaseForInstances(PostsTableName);
        if (text is not (not null or "")) return;
        db.Update("Text", text.Replace("+", " "), idPost);
        db.Update("PublicationDate", DateTime.Now.ToString(), idPost);
    }

    [HttpPOST("add")]
    public static void AddPost(string text, string? id)
    {
        var db = new DataBaseForInstances(PostsTableName);
        var account = Application.GetById(id);
        db.Insert(text.Replace("+", " "), DateTime.Today.ToString(), account.Nickname);
    }

    private static byte[] GetChangedPage<T>(string? id, T model, string page)
    {
        var user = Application.GetById(id);
        var flag = user is not null;
        var tpl = Template.Parse(page);
        var res = tpl!.Render(new {model = model, flag = flag, user = user});
        return Encoding.UTF8.GetBytes(res);
    }
}