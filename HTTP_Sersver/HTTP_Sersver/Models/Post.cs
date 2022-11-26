namespace NetConsoleApp.Models;

public class Post
{
    public int Id { get; }
    public string Text { get; }
    public DateTime PublicationDate { get; }
    public string Author { get; }

    public Post(int id, string text, DateTime publicationDate, string author)
    {
        Id = id;
        Text = text;
        PublicationDate = publicationDate;
        Author = author;
    }
}