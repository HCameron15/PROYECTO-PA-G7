using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Uam.AdvancedProgramming.MvcClient.Models;

namespace Uam.AdvancedProgramming.MvcClient.Controllers;

public class AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration) : Controller
{
    /// <summary>
    /// 
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

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
            Content = new StringContent(
                JsonSerializer.Serialize(dto),
                Encoding.UTF8,
                MediaTypeHeaderValue.Parse("application/json"))
        };

        using var response = await client.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var apiResult = JsonSerializer.Deserialize<ApiResponse<LoginResponseDto>>(content, JsonOptions);

        if (!response.IsSuccessStatusCode || apiResult?.Result is null || !apiResult.Success)
        {
            ViewBag.Error = apiResult?.Message ?? "Credenciales inválidas.";
            return View(dto);
        }

        HttpContext.Session.SetString("OtpSessionToken", apiResult.Result.SessionToken);

        return RedirectToAction("VerifyOtp", "Auth");
    }

    [HttpGet]
    public IActionResult VerifyOtp()
    {
        var sessionToken = HttpContext.Session.GetString("OtpSessionToken");

        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequestDto dto, CancellationToken cancellationToken)
    {
        var sessionToken = HttpContext.Session.GetString("OtpSessionToken");

        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var client = httpClientFactory.CreateClient();

        var endpoint = $"{configuration["ApiSettings:BaseUrl"]}{configuration["ApiSettings:VerifyOtpEndpoint"]}";

        var payload = new VerifyOtpApiRequestDto
        {
            SessionToken = sessionToken,
            Code = dto.Code
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                MediaTypeHeaderValue.Parse("application/json"))
        };

        using var response = await client.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var apiResult = JsonSerializer.Deserialize<ApiResponse<VerifyOtpResponseDto>>(content, JsonOptions);

        if (!response.IsSuccessStatusCode || apiResult?.Result is null || !apiResult.Success)
        {
            ViewBag.Error = apiResult?.Message ?? "Código OTP inválido, vencido o ya utilizado.";
            return View(dto);
        }

        var expiresIn = apiResult.Result.ExpiresIn > 0
            ? apiResult.Result.ExpiresIn
            : 1800;

        Response.Cookies.Append("AccessToken", apiResult.Result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddSeconds(expiresIn)
        });

        Response.Cookies.Append("RefreshToken", apiResult.Result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        HttpContext.Session.Remove("OtpSessionToken");

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
                Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    MediaTypeHeaderValue.Parse("application/json"))
            };

            await client.SendAsync(request, cancellationToken);
        }

        Response.Cookies.Delete("AccessToken");
        Response.Cookies.Delete("RefreshToken");
        HttpContext.Session.Clear();

        return RedirectToAction("Login", "Auth");
    }
}