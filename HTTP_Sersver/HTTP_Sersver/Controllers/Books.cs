
using NetConsoleApp.Attributes;
using NetConsoleApp.DataBase;
using NetConsoleApp.Models;

namespace NetConsoleApp.Controllers;

[HttpController("books")]
public class Books
{
    private const string BooksTableName = "[dbo].[Books]";
    private const string FavBooksTableName = "[dbo].[FavBooks]";
    private const string BooksPage = "sites/books/page.sbnhtml";
    [HttpGET("")]
    public static byte[] GetBooksPage(string id)
    {
        var books = new DataBaseForInstances(BooksTableName).Select<Book>().ToArray();
        var favBooks = new DataBaseForInstances(FavBooksTableName).Select<FavBook>();
       
        var user = Application.GetById(id);
        if (user is not null)
        {
            foreach (var book in books)
            {
                if (!favBooks.Any(fb => fb.IdUser == user.Id && fb.BookName == book.Name && fb.Genre == book.Genre))
                    book.Disrespectful = true;
            }
        }

        return Application.GetChangedPage(id, books.ToList(),
            File.ReadAllText(Path.GetFullPath(BooksPage)));
    }
        
    [HttpPOST("postToFav")]
    public static void PostFavBook(int idBook, string id)
    {
        var db = new DataBaseForInstances(BooksTableName);
        var book = db.Select<Book>().FirstOrDefault(b => b.Id == idBook);
        var user = Application.GetById(id);
        
        if (user is null || book is null)
            return;
        var dbFavBooks = new DataBaseForInstances(FavBooksTableName);
        var fb = new FavBook(user.Id, book.Name, book.Genre);
        dbFavBooks.Insert(fb);
    }

}