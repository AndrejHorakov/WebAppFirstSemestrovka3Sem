using NetConsoleApp.Attributes;
using NetConsoleApp.DataBase;
using NetConsoleApp.Models;

namespace NetConsoleApp.Controllers;

[HttpController("genres")]
public class Genres
{
    private const string GenresPage = "sites/genres/page.sbnhtml";
    private const string GenresTableName = "[dbo].[Genres]";
    
    [HttpGET("")]
    public static byte[] GetGenresPage(string? id) =>
        Application.GetChangedPage(id,new DataBaseForInstances(GenresTableName).Select<Genre>(),
            File.ReadAllText(Path.GetFullPath(GenresPage)));
}

