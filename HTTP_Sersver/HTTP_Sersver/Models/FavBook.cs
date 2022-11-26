namespace NetConsoleApp.Models;

public class FavBook
{
    public int Id { get; }
    public string BookName { get; set; }
    public string Genre { get; set; }

    public FavBook(int id, string bookName, string genre)
    {
        Id = id;
        BookName = bookName;
        Genre = genre;
    }
}