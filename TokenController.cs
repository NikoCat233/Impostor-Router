using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Impostor_Router;

public class TokenController
{
    [ApiController]
    [Route("/api/user")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly HttpClient _httpClient;
        private readonly ForwardingOptions _forwardingOptions;

        public UserController(ILogger<UserController> logger, HttpClient httpClient, ForwardingOptions forwardingOptions)
        {
            _logger = logger;
            _httpClient = httpClient;
            _forwardingOptions = forwardingOptions;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TokenRequest request)
        {
            if (request == null || !Request.Headers.ContainsKey("Authorization"))
            {
                var unAuthorizedResponse = new { Errors = new[] { new { Reason = 11 } } };
                return BadRequest(unAuthorizedResponse);
            }

            var clientVersion = request.ClientVersion;

            if (clientVersion >= 50605450) // 2024.6.18
            {
                var (statusCode, responseContent) = await ForwardRequest(request, _forwardingOptions.Forward1);

                return StatusCode(statusCode, new { Message = "Forwarded to 2024.6.18.", ResponseContent = responseContent });
            }

            if (clientVersion >= 50593050 && clientVersion <= 50603775)
            {
                var (statusCode, responseContent) = await ForwardRequest(request, _forwardingOptions.Forward2);

                return StatusCode(statusCode, new { Message = "Forwarded to 2023.10.1 - 2024.2.3.", ResponseContent = responseContent });
            }

            if (clientVersion >= 50545250 && clientVersion <= 505559625)
            {
                var (statusCode, responseContent) = await ForwardRequest(request, _forwardingOptions.Forward3);

                return StatusCode(statusCode, new { Message = "Forwarded to Impostor 172.", ResponseContent = responseContent });
            }

            var unSupportedVersionResponse = new { Errors = new[] { new { Reason = 5 } } };
            return BadRequest(unSupportedVersionResponse);
        }

        private async Task<(int StatusCode, string ResponseContent)> ForwardRequest(TokenRequest request, int port)
        {
            var url = $"http://localhost:{port}/api/user"; // 将路径替换为目标路径

            var jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            var originalRequestHeaders = Request.Headers;
            foreach (var header in originalRequestHeaders)
            {
                if (!content.Headers.Contains(header.Key))
                {
                    content.Headers.Add(header.Key, header.Value.ToArray());
                }
            }

            var response = await _httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            return ((int)response.StatusCode, responseBody);
        }

    }

    public class TokenRequest
    {
        [JsonPropertyName("Puid")]
        public required string ProductUserId { get; init; }

        [JsonPropertyName("Username")]
        public required string Username { get; init; }

        [JsonPropertyName("ClientVersion")]
        public required int ClientVersion { get; init; }

        [JsonPropertyName("Language")]
        public required Language Language { get; init; }
    }

    public enum Language
    {
        English = 0,
        Latam = 1,
        Brazilian = 2,
        Portuguese = 3,
        Korean = 4,
        Russian = 5,
        Dutch = 6,
        Filipino = 7,
        French = 8,
        German = 9,
        Italian = 10,
        Japanese = 11,
        Spanish = 12,
        SChinese = 13,
        TChinese = 14,
        Irish = 15,
    }
}
