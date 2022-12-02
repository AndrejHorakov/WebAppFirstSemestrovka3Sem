namespace NetConsoleApp.Models;

public class Book
{
    public int Id { get; }
    public string Name { get; }
    public string Genre { get; }
    public string Description { get; }
    public string Path { get; }
    public bool Disrespectful { get; set; }

    public Book(int id, string name, string genre, string description, string path)
    {
        Id = id;
        Name = name;
        Genre = genre;
        Description = description;
        Path = path;
    }
}