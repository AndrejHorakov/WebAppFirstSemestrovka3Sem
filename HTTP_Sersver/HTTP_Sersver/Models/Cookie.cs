namespace NetConsoleApp.Models;

public class Cookie
{
    public string Guid { get; }
    public int IdUs { get;}

    public Cookie(string guid, int id)
    {
        Guid = guid;
        IdUs = id;
    }
}