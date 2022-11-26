using NetConsoleApp.Attributes;
using NetConsoleApp.DataBase;
using NetConsoleApp.Models;

namespace NetConsoleApp.Controllers;

[HttpController("favBooks")]
public class FavBooks
{
    private const string FavBooksTableName = "[dbo].[FavBooks]";
    private const string BooksPage = "sites/books/page.sbnhtml";
    private const string BooksTableName = "[dbo].[Books]";
    
    [HttpGET("/favBooks")]
    public static byte[] GetFavBooks(string id)
    {
        var user = Application.GetById(id);
        IEnumerable<Book> books = new List<Book>();
        if (user is not null)
        {
            
            var favBooks = new DataBaseForInstances(FavBooksTableName)
                .Select<FavBook>()
                .Where(fb => fb.Id == user.Id)
                .Select(fb => fb.BookName);
            books = new DataBaseForInstances(BooksTableName)
                .Select<Book>()
                .Where(b => favBooks.Contains(b.Name));
        }

        return Application.GetChangedPage(id, books.ToList(),
            File.ReadAllText(Path.GetFullPath(BooksPage)));
    }
    
    [HttpPOST("/favBooks")]
    public static byte[] DeleteFavBook(string bookName, string id)
    {
        var user = Application.GetById(id);
        var query = $"delete from {FavBooksTableName} " +
                    $"where Id = {user!.Id} and BookName = {bookName}";
        return GetFavBooks(id);
    }
}