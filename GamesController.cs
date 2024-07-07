using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Impostor_Router;

[ApiController]
[Route("/api/games")]
public class GamesController : ControllerBase
{
    private readonly ILogger<GamesController> _logger;
    private readonly HttpClient _httpClient;
    private readonly ForwardingOptions _forwardingOptions;

    public GamesController(ILogger<GamesController> logger, HttpClient httpClient, ForwardingOptions forwardingOptions)
    {
        _logger = logger;
        _httpClient = httpClient;
        _forwardingOptions = forwardingOptions;
    }

    private int GetVersion(string bearerToken)
    {
        try
        {
            // Check if the token starts with "Bearer "
            if (!bearerToken.StartsWith("Bearer "))
            {
                throw new ArgumentException("Invalid bearer token");
            }

            // Remove the "Bearer " prefix
            var jwt = bearerToken.Substring("Bearer ".Length);

            // Decode the base64 encoded JSON
            var bytes = Convert.FromBase64String(jwt);
            var json = Encoding.UTF8.GetString(bytes);

            // Parse the JSON
            var jsonDocument = JsonDocument.Parse(json);
            var root = jsonDocument.RootElement;

            // Extract the `Content` object
            if (root.TryGetProperty("Content", out var contentProperty))
            {
                // Extract the `ClientVersion` value
                if (contentProperty.TryGetProperty("ClientVersion", out var clientVersionProperty))
                {
                    return clientVersionProperty.GetInt32();
                }
            }
            throw new ArgumentException("ClientVersion not found in token");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse token", ex);
        }
    }

    private int getPort(int version)
    {
        if (version >= 50605450) // 2024.6.18
        {
            return _forwardingOptions.Forward1;
        }

        if (version >= 50593050 && version <= 50603775) // 2024.6.4
        {
            return _forwardingOptions.Forward2;
        }

        if (version >= 50545250 && version <= 505559625) // 172
        {
            return _forwardingOptions.Forward3;
        }

        return 404;
    }

    private async Task<(int StatusCode, string ResponseContent)> ForwardRequest(int port, HttpMethod method, Dictionary<string, string>? queryParams = null)
    {
        var baseUrl = $"http://localhost:{port}/api/games";
        var query = queryParams != null ? "?" + string.Join("&", queryParams.Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}")) : string.Empty;
        var url = baseUrl + query;

        var content = new StringContent(""); // Games requests have null body

        var httpRequestMessage = new HttpRequestMessage(method, url)
        {
            Content = content,
            Headers =
            {
                { "Authorization", Request.Headers["Authorization"].ToArray() },
                { "X-Forwarded-For", Request.Headers["X-Forwarded-For"].ToArray()}
            }
        };
        _logger.LogInformation(httpRequestMessage.ToString());
        var response = await _httpClient.SendAsync(httpRequestMessage);
        var responseBody = await response.Content.ReadAsStringAsync();

        return ((int)response.StatusCode, responseBody);
    }

    [HttpGet]
    public async Task<IActionResult> Get(int mapId, GameKeywords lang, int numImpostors, [FromHeader] AuthenticationHeaderValue authorization)
    {
        var clientVersion = GetVersion(authorization.ToString());
        var port = getPort(clientVersion);
        if (port == 404)
        {
            return BadRequest(new { Errors = new[] { new { Reason = 5 } } });
        }

        var queryParams = new Dictionary<string, string>
    {
        { "mapId", mapId.ToString() },
        { "lang", lang.ToString() },
        { "numImpostors", numImpostors.ToString() }
    };
        var (statusCode, responseContent) = await ForwardRequest(port, HttpMethod.Get, queryParams);

        // 反序列化 JSON 字符串为对象
        var responseObject = JsonSerializer.Deserialize<object>(responseContent);

        // 将对象作为 JSON 返回
        return StatusCode(statusCode, responseObject);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromHeader] AuthenticationHeaderValue authorization)
    {
        var clientVersion = GetVersion(authorization.ToString());
        // 对于没有查询参数的PUT请求，可以传递null或空字典
        var port = getPort(clientVersion);
        if (port == 404)
        {
            return BadRequest(new { Errors = new[] { new { Reason = 5 } } });
        }

        var (statusCode, responseContent) = await ForwardRequest(port, HttpMethod.Put, null); // Assuming port 8080 for example
        // 反序列化 JSON 字符串为对象
        var responseObject = JsonSerializer.Deserialize<object>(responseContent);

        // 将对象作为 JSON 返回
        return StatusCode(statusCode, responseObject);
    }

    [HttpPost]
    public async Task<IActionResult> Post(int gameId, [FromHeader] AuthenticationHeaderValue authorization)
    {
        var clientVersion = GetVersion(authorization.ToString());
        var port = getPort(clientVersion);
        if (port == 404)
        {
            return BadRequest(new { Errors = new[] { new { Reason = 5 } } });
        }

        var queryParams = new Dictionary<string, string>
    {
        { "gameId", gameId.ToString() }
    };
        var (statusCode, responseContent) = await ForwardRequest(port, HttpMethod.Post, queryParams); // Assuming port 8080 for example
        // 反序列化 JSON 字符串为对象
        var responseObject = JsonSerializer.Deserialize<object>(responseContent);

        // 将对象作为 JSON 返回
        return StatusCode(statusCode, responseObject);
    }

    public enum GameKeywords : uint
    {
        All = 0,
        Other = 1 << 0,
        SpanishLA = 1 << 1,
        Korean = 1 << 2,
        Russian = 1 << 3,
        Portuguese = 1 << 4,
        Arabic = 1 << 5,
        Filipino = 1 << 6,
        Polish = 1 << 7,
        English = 1 << 8,
        Japanese = 1 << 9,
        SpanishEU = 1 << 10,
        Brazilian = 1 << 11,
        Dutch = 1 << 12,
        French = 1 << 13,
        German = 1 << 14,
        Italian = 1 << 15,
        SChinese = 1 << 16,
        TChinese = 1 << 17,
        Irish = 1 << 18,
    }
}
