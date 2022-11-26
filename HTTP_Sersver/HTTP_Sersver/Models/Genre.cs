namespace NetConsoleApp.Models;

public class Genre
{
    public int Id { get; }
    public string Title { get; }
    public int Rating { get; }
    public string TimeBorn { get; }

    public Genre(string title, int rating, string timeBorn)
    {
        Title = title;
        TimeBorn = timeBorn;
        Rating = rating;
    }
}