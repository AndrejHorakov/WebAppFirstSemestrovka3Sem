using System.Text;
using NetConsoleApp.Attributes;
using NetConsoleApp.DataBase;
using NetConsoleApp.Models;
using NetConsoleApp.ORM;
using Scriban;

namespace NetConsoleApp.Controllers;

[HttpController("profile")]
public class Profile
{
    private const string ProfilePage = "sites/profile/page.sbnhtml";
    private const string FavBooksTableName = "[dbo].[FavBooks]";
    private const string BooksTableName = "[dbo].[Books]";

    [HttpGET("")]
    public static byte[] GetProfilePage(string id)
    {
        var user = Application.GetById(id);
        var favBooks = new DataBaseForInstances(FavBooksTableName)
            .Select<FavBook>()
            .Where(fb => fb.IdUser == user!.Id);
        var books = new DataBaseForInstances(BooksTableName)
            .Select<Book>()
            .Where(book => favBooks.Any(fb => fb.BookName == book.Name && fb.Genre == book.Genre));
        return GetChangedPage(id, books,
            File.ReadAllText(Path.GetFullPath(ProfilePage)));
    }

    [HttpPOST("edit")]
    public static byte[] UpdateProfilePage(string nickname, string id)
    {
        var db = new OrmAccountDao();
        var user = Application.GetById(id);
        db.Update("Nickname", nickname, user!.Id);
        return GetProfilePage(id);
    }
    
    [HttpPOST("deleteFav")]
    public static void DeleteFavBook(int idBook, string id)
    {
        var db = new DataBaseForInstances(BooksTableName);
        var book = db.Select<Book>().FirstOrDefault(b => b.Id == idBook);
        var user = Application.GetById(id);
        
        if (user is null || book is null)
            return;
        var dbFavBooks = new DataBaseForInstances(FavBooksTableName);
        dbFavBooks.Delete("BookName", book.Name, user.Id);
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