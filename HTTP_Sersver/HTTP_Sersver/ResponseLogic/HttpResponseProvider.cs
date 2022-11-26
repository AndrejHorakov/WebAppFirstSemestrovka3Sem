using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using NetConsoleApp.Attributes;
using NetConsoleApp.Models;
using NetConsoleApp.ORM;
using NetConsoleApp.SessionLogic;
using NetConsoleApp.Controllers;

namespace NetConsoleApp.ResponseLogic;

internal partial class HttpResponseProvider : IHttpResponseProvider
{
    
    public ServerResponse GetServerResponse(HttpListener listener, SettingParameters settings, HttpListenerContext httpContext)
    {
        var fullPath = Path.GetFullPath(settings.Directory);
        var rawUrl = httpContext.Request.RawUrl?.Replace("%20", " ");
        var urlRefer = httpContext.Request.UrlReferrer?.AbsolutePath;
        if (Directory.Exists(fullPath))
        {
            var output = GetStaticFile(rawUrl, fullPath, urlRefer!); 
            if (output.ResponseCode != HttpStatusCode.NotFound)
                return output;
        }

        return GetDateFromController(httpContext.Request, rawUrl!);
    }


    private ServerResponse GetDateFromController(HttpListenerRequest request, string rawUrl)
    {
        if (request.Url!.Segments.Length < 2) return GetResponseNotFound(rawUrl);
        var isAuthorized = IsAuthorised(request);
        
        using var sr = new StreamReader(request.InputStream, request.ContentEncoding);
        var bodyParam = sr.ReadToEnd();
        
        var controllerName = request.Url.Segments[1].Replace("/", "");
        var strParams = request.Url.Segments
            .Skip(3)
            .Select(s => s.Replace("/", ""))
            .Concat(bodyParam.Split('&').Select(p => p.Split('=').LastOrDefault()))
            .ToArray();

        var assembly = Assembly.GetExecutingAssembly();
        var controller = assembly.GetTypes()
            .Where(t => Attribute.IsDefined(t, typeof(HttpController)))
            .FirstOrDefault(t => string.Equals(
                (t.GetCustomAttribute(typeof(HttpController)) as HttpController)?.ControllerName,
                controllerName,
                StringComparison.CurrentCultureIgnoreCase));

        var method = controller?.GetMethods()
            .FirstOrDefault(t => t.GetCustomAttributes(true)
                .Any(attr => attr.GetType().Name == $"Http{request.HttpMethod}"
                             && Regex.IsMatch(request.RawUrl ?? "",
                                 attr.GetType()
                                     .GetField("MethodUri")?
                                     .GetValue(attr)?.ToString() ?? "")));
        
        if (method is null) return GetResponseNotFound(rawUrl);

        if (IsRequiredAuthentication(method.Name) && !isAuthorized)
        {
            var result = new ServerResponse(Array.Empty<byte>(), "text/html", HttpStatusCode.Redirect)
             {
                 RedirectLink = "http://localhost:8080/Application/"
             };
            return result;
        }

        if (IsRequiredRedirect(method.Name) && isAuthorized)
        {
            var result = new ServerResponse(Array.Empty<byte>(), "text/html", HttpStatusCode.Redirect)
             {
                 RedirectLink = "http://localhost:8080/profile"
             };
            return result;
        }

        strParams = AddAuthorisation(strParams, request);
        var queryParams = method.GetParameters()
            .Select((p, i) => Convert.ChangeType(strParams[i], p.ParameterType))
            .ToArray();
        
        var ret = method.Invoke(Activator.CreateInstance(controller!), queryParams);

        if (method.Name == "ExiteProfile")
        {
            var result = new ServerResponse(Array.Empty<byte>(), "text/html", HttpStatusCode.Redirect);
            result.RedirectLink = "http://localhost:8080/authorisation";
            result.ClearCookies = true;
            return result;
        }
        
        if (method.Name == "BeginPage")
        {
            var result = new ServerResponse(Array.Empty<byte>(), "text/html", HttpStatusCode.Redirect);
            result.RedirectLink = "http://localhost:8080/registration";
            return result;
        }

        if (method.Name == "CheckAuthorisation")
        {
            var info = ((bool, User?, bool))ret!;
            if (info.Item1)
            {
                var result = new ServerResponse(Array.Empty<byte>(), "text/html", HttpStatusCode.Redirect);
                result.AddCookies = true;
                result.RedirectLink = "http://localhost:8080/general";
                result.Instance = info;
                return result;
            }
            else
            {
                var result = new ServerResponse(Array.Empty<byte>(), "text/html", HttpStatusCode.Redirect);
                result.RedirectLink = "http://localhost:8080/authorisation";
                return result;
            }
        }

        if (method.Name == "SaveUser")
        {
            var acc = ((bool, User))ret!;
            if (!acc.Item1)
            {
                var result = new ServerResponse(Array.Empty<byte>(), "text/html", HttpStatusCode.Redirect);
                result.RedirectLink = "http://localhost:8080/registration";
                return result;
            }
        }


        ServerResponse res;
        if (request.HttpMethod == "POST")
        {
            res = new ServerResponse(Array.Empty<byte>(), "text/html", HttpStatusCode.Redirect);
            res.RedirectLink = GetRedirect(method.Name);
            return res;
        }

        var buffer = ret as byte[];
        res = new ServerResponse(buffer!, DictionaryOfTypes["html"], HttpStatusCode.OK);
        return res;


    }

    private static string GetRedirect(string name) => name switch
    {
        "SaveUser" => "http://localhost:8080/authorisation",
        "PostFavBook" => "http://localhost:8080/books",
        "DeleteFavBook" => "http://localhost:8080/favBooks",
        "UpdateProfilePage" => "http://localhost:8080/profile",
        _ => "http://localhost:8080/post"
    };
    
    private ServerResponse GetStaticFile(string? rawUrl, string fullPath, string urlRefer)
    {
        var buffer = GetFile(rawUrl, fullPath, urlRefer, out var contentType);
        return buffer is null ? 
            GetResponseNotFound(rawUrl) :
            new ServerResponse(buffer, contentType, HttpStatusCode.OK);
    }

    private ServerResponse GetResponseNotFound(string? rawUrl) =>
        new ServerResponse(Encoding.UTF8.GetBytes($"File {rawUrl} not found 404."), 
            "text/plain", HttpStatusCode.NotFound);
    
    private static byte[]? GetFile(string? rawUrl, string directory, string? urlRefer, out string contentType)
    {
        rawUrl = rawUrl!.Replace("/Application", "");
        urlRefer = urlRefer?.Replace("/Application", "");
        byte[]? result = null;
        var filePath = directory + urlRefer + rawUrl;
        if (Directory.Exists(filePath))
        {
            filePath += "/index.sbnhtml";
            if (File.Exists(filePath))
                result = File.ReadAllBytes(filePath);
        }
        else if (File.Exists(filePath))
            result = File.ReadAllBytes(filePath);
        
        contentType = GetContentType(rawUrl);
        return result;
    }
    
    private static string GetContentType(string path)
    {
        var ext = path.Contains('.') ? path.Split('.')[^1] : "html";
        return DictionaryOfTypes.ContainsKey(ext) ? DictionaryOfTypes[ext] : "text/plain";
    }
    
    private static bool IsAuthorised(HttpListenerRequest request)
    {
        var sessionCookie = request.Cookies["SessionId"];
        var cookie = request.Cookies["Cookie"];
        User? user = null;
        if (sessionCookie is not null && sessionCookie.Value != "" &&
            SessionProvider.CheckSession(new Guid(sessionCookie.Value)))
        {
            var cookieAuthInfo = sessionCookie.Value;
            user = Application.GetById(cookieAuthInfo);
            if (user != null)
                return true;
        }

        if (cookie is not null && cookie.Value != "")
        {
            user = Application.GetById(cookie.Value);
        }

        return user != null;
    }
    
  
    
    private static string?[] AddAuthorisation(string?[] strParams, HttpListenerRequest request)
    {
        var sessionCookie = request.Cookies["SessionId"];
        var cookie = request.Cookies["Cookie"];
        if (sessionCookie is not null && sessionCookie.Value != "" &&
            SessionProvider.CheckSession(new Guid(sessionCookie.Value)))
        {
            var cookieAuthInfo = sessionCookie.Value;
            if (strParams[0] == "")
            {
                return new[] { cookieAuthInfo };
            }

            var res = strParams.Concat(new[] { cookieAuthInfo }).ToArray();
            return res;
        } 
        
        if (cookie is not null && cookie.Value != "")
        {
            if (strParams[0] == "")
            {
                return new[] { cookie.Value };
            }
            var res = strParams.Concat(new[] { cookie.Value }).ToArray();
            return res;
        }
        
        return strParams;
    }

    private static bool IsRequiredAuthentication(string controllerName) => 
        controllerName == "/profile";
    
    private static bool IsRequiredRedirect(string controllerName) => controllerName switch
    {
        "authorisation" or "registration" => true,
        _ => false
    };
}