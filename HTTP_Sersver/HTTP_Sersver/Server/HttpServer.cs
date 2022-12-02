using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NetConsoleApp.Models;
using static System.GC;
using NetConsoleApp.ResponseLogic;
using NetConsoleApp.SessionLogic;
using Cookie = System.Net.Cookie;

namespace NetConsoleApp;

internal class HttpServer : IDisposable
{

    private SettingParameters _settings;
    private readonly HttpListener _listener;
    private readonly IHttpResponseProvider _provider;
    private const string SettingPath = "./Settings/settings.json"; 
    
    public HttpServer(IHttpResponseProvider provider)
    {
        _provider = provider;
        _listener = new HttpListener();
        _settings = new SettingParameters();
        Start();
    }

    public void Start()
    {
        if (_listener.IsListening)
        {
            Console.WriteLine("Server is already running");
            return;
        }

        if (File.Exists(Path.GetFullPath(SettingPath)))
            _settings = JsonSerializer.Deserialize<SettingParameters>(File.ReadAllText(SettingPath))
                       ?? new SettingParameters();
        _listener.Prefixes.Clear();
        _listener.Prefixes.Add($"http://localhost:{_settings.Port}/" );
        _listener.Start();
        Listen();
    }

    public void Stop()
    {
        if (!_listener.IsListening)
        {
            Console.WriteLine("Server is already stopped");
            return;
        }

        try
        {
            _listener.Stop();
        }
        catch(Exception e)
        {
            Console.WriteLine($"Stopping end exception {e.Message}");
        }
    }

    private void Listen() => _listener.BeginGetContext(ServerResponse, _listener);
    
    private void ServerResponse(IAsyncResult request)
    {
        try
        {
            if (_listener is null) return;
        
            var httpContext = _listener?.EndGetContext(request);
            var response = httpContext.Response;

            var serverResponse = _provider.GetServerResponse(_listener, _settings, httpContext);
            response.Headers.Set("Content-Type", serverResponse.ContentType);
            response.StatusCode = (int)serverResponse.ResponseCode;

            if (serverResponse.ResponseCode is HttpStatusCode.Redirect)
                response.Redirect(serverResponse.RedirectLink);

            if (serverResponse.AddCookies)
                AddCookieAndSession(serverResponse.Instance!, response);
            
            if (serverResponse.ClearCookies)
                ClearCookie(response);
            
            var output = response.OutputStream;
            output.WriteAsync(serverResponse.Buffer, 0, serverResponse.Buffer.Length);
            Task.WaitAll();

            output.Close();
            response.Close();

            Listen();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static void AddCookieAndSession((bool, User, bool) acc,
        HttpListenerResponse response)
    {
        response.Cookies.Add(new Cookie("SessionId",
            SessionProvider.CreateSession(acc.Item2.Id, acc.Item2.Login, DateTime.Now).ToString()));

        if (!acc.Item3) return;
        Guid result;
        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(acc.Item2.Nickname + acc.Item2.Password));
            result = new Guid(hash);
        }

        response.Cookies.Add(new Cookie("Cookie", result.ToString()));
    }

    private static void ClearCookie(HttpListenerResponse response)
    {
        response.Cookies.Add(new Cookie("SessionId", ""));
        response.Cookies.Add(new Cookie("Cookie", ""));
    }
    
    public void Dispose()
    {
        Stop();
        SuppressFinalize(this);
    }
}