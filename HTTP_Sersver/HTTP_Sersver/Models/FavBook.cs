namespace NetConsoleApp.Models;

public class FavBook
{
    public int IdUser { get; }
    public string BookName { get; set; }
    public string Genre { get; set; }

    public FavBook(int idUser, string bookName, string genre)
    {
        IdUser = idUser;
        BookName = bookName;
        Genre = genre;
    }
}