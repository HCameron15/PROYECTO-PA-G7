using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Uam.AdvancedProgramming.MvcClient.Models;

namespace Uam.AdvancedProgramming.MvcClient.Controllers;

public abstract class BaseApiController(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration
) : Controller
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected async Task<HttpResponseMessage?> SendApiRequestAsync(
        HttpMethod method,
        string endpoint,
        object? body,
        CancellationToken cancellationToken)
    {
        var accessToken = Request.Cookies["AccessToken"];

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            Response.Redirect("/Auth/Login");
            return null;
        }

        var client = httpClientFactory.CreateClient();

        var request = BuildRequest(method, endpoint, accessToken, body);

        var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
        {
            return response;
        }

        var newAccessToken = await TryRefreshTokenAsync(client, cancellationToken);

        if (string.IsNullOrWhiteSpace(newAccessToken))
        {
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");
            Response.Redirect("/Auth/Login");
            return null;
        }

        var retryRequest = BuildRequest(method, endpoint, newAccessToken, body);

        return await client.SendAsync(retryRequest, cancellationToken);
    }

    private HttpRequestMessage BuildRequest(
        HttpMethod method,
        string endpoint,
        string accessToken,
        object? body)
    {
        var request = new HttpRequestMessage(method, endpoint);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        if (body is not null)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");
        }

        return request;
    }

    private async Task<string?> TryRefreshTokenAsync(
        HttpClient client,
        CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["RefreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var endpoint = $"{configuration["ApiSettings:BaseUrl"]}/api/Auth/RefreshToken";

        var payload = new RefreshTokenRequestDto
        {
            RefreshToken = refreshToken
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json")
        };

        using var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var apiResult = JsonSerializer.Deserialize<ApiResponse<LoginResponseDto>>(content, JsonOptions);

        if (apiResult?.Result is null || !apiResult.Success)
        {
            return null;
        }

        Response.Cookies.Append("AccessToken", apiResult.Result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddSeconds(apiResult.Result.ExpiresIn)
        });

        Response.Cookies.Append("RefreshToken", apiResult.Result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return apiResult.Result.AccessToken;
    }
}