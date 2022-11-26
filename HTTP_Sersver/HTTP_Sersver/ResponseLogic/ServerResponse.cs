using System.Net;
using NetConsoleApp.Models;

namespace NetConsoleApp.ResponseLogic;

public class ServerResponse
{
    public readonly byte[] Buffer;
    public readonly string ContentType;
    public readonly HttpStatusCode ResponseCode;
    public string RedirectLink { get; set; }
    public bool AddCookies { get; set; }
    public bool ClearCookies { get; set; }
    public (bool, User?, bool) Instance { get; set; }

    public ServerResponse(byte[] buffer, string contentType, HttpStatusCode responseCode)
    {
        Buffer = buffer;
        ContentType = contentType;
        ResponseCode = responseCode;
    }
}