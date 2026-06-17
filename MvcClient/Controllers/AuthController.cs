using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Uam.AdvancedProgramming.MvcClient.Models;

namespace Uam.AdvancedProgramming.MvcClient.Controllers;

public class AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration) : Controller
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequestDto dto, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient();

        var endpoint = $"{configuration["ApiSettings:BaseUrl"]}{configuration["ApiSettings:LoginEndpoint"]}";

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json")
        };

        using var response = await client.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Credenciales inválidas.";
            return View(dto);
        }

        var apiResult = JsonSerializer.Deserialize<ApiResponse<LoginResponseDto>>(content, JsonOptions);

        if (apiResult?.Result is null || !apiResult.Success)
        {
            ViewBag.Error = apiResult?.Message ?? "Credenciales inválidas.";
            return View(dto);
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

        return RedirectToAction("Index", "Maintenance");
    }

    [HttpPost]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["RefreshToken"];

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var client = httpClientFactory.CreateClient();

            var endpoint = $"{configuration["ApiSettings:BaseUrl"]}/api/Auth/Logout";

            var payload = new LogoutRequestDto
            {
                RefreshToken = refreshToken
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            await client.SendAsync(request, cancellationToken);
        }

        Response.Cookies.Delete("AccessToken");
        Response.Cookies.Delete("RefreshToken");

        return RedirectToAction("Login", "Auth");
    }
}