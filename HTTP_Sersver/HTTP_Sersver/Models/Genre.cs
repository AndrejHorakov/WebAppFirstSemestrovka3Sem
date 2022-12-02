namespace NetConsoleApp.Models;

public class Genre
{
    public int Id { get; }
    public string Title { get; }
    public int Rating { get; }
    public string Description { get; }

    public Genre(int id, string title, string description, int rating)
    {
        Id = id;
        Title = title;
        Description = description;
        Rating = rating;
    }
}