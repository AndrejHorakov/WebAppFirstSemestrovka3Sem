namespace NetConsoleApp.Models;

public class Post
{
    public int Id { get; }
    public string Text { get; }
    public DateTime Date { get; }
    public string Author { get; }

    public Post(int id, string text, DateTime date, string author)
    {
        Id = id;
        Text = text;
        Date = date;
        Author = author;
    }
}