using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Uam.AdvancedProgramming.MvcClient.Models;

namespace Uam.AdvancedProgramming.MvcClient.Controllers;

public class AuthController(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration
) : Controller
{
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
    public async Task<IActionResult> Login(
        LoginRequestDto dto,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient();

        var endpoint =
            $"{configuration["ApiSettings:BaseUrl"]}" +
            $"{configuration["ApiSettings:LoginEndpoint"]}";

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            endpoint)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(dto),
                Encoding.UTF8,
                MediaTypeHeaderValue.Parse("application/json"))
        };

        using var response = await client.SendAsync(
            request,
            cancellationToken);

        var content = await response.Content.ReadAsStringAsync(
            cancellationToken);

        var apiResult =
            JsonSerializer.Deserialize<ApiResponse<LoginResponseDto>>(
                content,
                JsonOptions);

        if (!response.IsSuccessStatusCode ||
            apiResult?.Result is null ||
            !apiResult.Success)
        {
            ViewBag.Error =
                apiResult?.Message ?? "Credenciales inválidas.";

            return View(dto);
        }

        HttpContext.Session.SetString(
            "OtpSessionToken",
            apiResult.Result.SessionToken);

        return RedirectToAction("VerifyOtp", "Auth");
    }

    [HttpGet]
    public IActionResult VerifyOtp()
    {
        var sessionToken =
            HttpContext.Session.GetString("OtpSessionToken");

        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> VerifyOtp(
        VerifyOtpRequestDto dto,
        CancellationToken cancellationToken)
    {
        var sessionToken =
            HttpContext.Session.GetString("OtpSessionToken");

        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var client = httpClientFactory.CreateClient();

        var endpoint =
            $"{configuration["ApiSettings:BaseUrl"]}" +
            $"{configuration["ApiSettings:VerifyOtpEndpoint"]}";

        var payload = new VerifyOtpApiRequestDto
        {
            SessionToken = sessionToken,
            Code = dto.Code
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            endpoint)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                MediaTypeHeaderValue.Parse("application/json"))
        };

        using var response = await client.SendAsync(
            request,
            cancellationToken);

        var content = await response.Content.ReadAsStringAsync(
            cancellationToken);

        var apiResult =
            JsonSerializer.Deserialize<ApiResponse<VerifyOtpResponseDto>>(
                content,
                JsonOptions);

        if (!response.IsSuccessStatusCode ||
            apiResult?.Result is null ||
            !apiResult.Success)
        {
            ViewBag.Error =
                apiResult?.Message ??
                "Código OTP inválido, vencido o ya utilizado.";

            return View(dto);
        }

        var expiresIn =
            apiResult.Result.ExpiresIn > 0
                ? apiResult.Result.ExpiresIn
                : 1800;

        Response.Cookies.Append(
            "AccessToken",
            apiResult.Result.AccessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddSeconds(expiresIn)
            });

        Response.Cookies.Append(
            "RefreshToken",
            apiResult.Result.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

        HttpContext.Session.SetString(
            "AccessToken",
            apiResult.Result.AccessToken);

        HttpContext.Session.SetString(
            "RefreshToken",
            apiResult.Result.RefreshToken);

        var userRole =
            GetRoleFromJwt(apiResult.Result.AccessToken);
        

        if (!string.IsNullOrWhiteSpace(userRole))
        {
            HttpContext.Session.SetString(
                "UserRole",
                userRole);
        }
        else
        {
            HttpContext.Session.Remove("UserRole");
        }

        HttpContext.Session.Remove("OtpSessionToken");

        return RedirectToAction("Index", "Maintenance");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequestDto dto,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient();

        var endpoint =
            $"{configuration["ApiSettings:BaseUrl"]}" +
            $"{configuration["ApiSettings:ForgotPasswordEndpoint"]}";

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            endpoint)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(dto),
                Encoding.UTF8,
                MediaTypeHeaderValue.Parse("application/json"))
        };

        using var response = await client.SendAsync(
            request,
            cancellationToken);

        var content = await response.Content.ReadAsStringAsync(
            cancellationToken);

        var apiResult =
            JsonSerializer.Deserialize<
                ApiResponse<ForgotPasswordResponseDto>>(
                content,
                JsonOptions);

        ViewBag.Message =
            apiResult?.Message ??
            "Si el correo está registrado, se enviarán instrucciones de recuperación.";

        if (response.IsSuccessStatusCode &&
            apiResult?.Success == true &&
            !string.IsNullOrWhiteSpace(
                apiResult.Result?.SessionToken))
        {
            HttpContext.Session.SetString(
                "PasswordResetSessionToken",
                apiResult.Result.SessionToken);

            return RedirectToAction(
                "ResetPassword",
                "Auth");
        }

        return View(dto);
    }

    [HttpGet]
    public IActionResult ResetPassword()
    {
        var sessionToken =
            HttpContext.Session.GetString(
                "PasswordResetSessionToken");

        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            return RedirectToAction(
                "ForgotPassword",
                "Auth");
        }

        return View(new ResetPasswordRequestDto
        {
            SessionToken = sessionToken
        });
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequestDto dto,
        CancellationToken cancellationToken)
    {
        var sessionToken =
            HttpContext.Session.GetString(
                "PasswordResetSessionToken");

        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            return RedirectToAction(
                "ForgotPassword",
                "Auth");
        }

        dto.SessionToken = sessionToken;

        var client = httpClientFactory.CreateClient();

        var endpoint =
            $"{configuration["ApiSettings:BaseUrl"]}" +
            $"{configuration["ApiSettings:ResetPasswordEndpoint"]}";

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            endpoint)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(dto),
                Encoding.UTF8,
                MediaTypeHeaderValue.Parse("application/json"))
        };

        using var response = await client.SendAsync(
            request,
            cancellationToken);

        var content = await response.Content.ReadAsStringAsync(
            cancellationToken);

        var apiResult =
            JsonSerializer.Deserialize<ApiResponse<object>>(
                content,
                JsonOptions);

        if (!response.IsSuccessStatusCode ||
            apiResult is null ||
            !apiResult.Success)
        {
            ViewBag.Error =
                apiResult?.Message ??
                "El código OTP es inválido, vencido o ya fue usado.";

            return View(dto);
        }

        HttpContext.Session.Remove(
            "PasswordResetSessionToken");

        TempData["Success"] =
            apiResult.Message ??
            "Contraseña restablecida correctamente.";

        return RedirectToAction("Login", "Auth");
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        var accessToken =
            HttpContext.Session.GetString("AccessToken")
            ?? Request.Cookies["AccessToken"];

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequestDto dto,
        CancellationToken cancellationToken)
    {
        var accessToken =
            HttpContext.Session.GetString("AccessToken")
            ?? Request.Cookies["AccessToken"];

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var client = httpClientFactory.CreateClient();

        var endpoint =
            $"{configuration["ApiSettings:BaseUrl"]}" +
            $"{configuration["ApiSettings:ChangePasswordEndpoint"]}";

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            endpoint)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(dto),
                Encoding.UTF8,
                MediaTypeHeaderValue.Parse("application/json"))
        };

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        using var response = await client.SendAsync(
            request,
            cancellationToken);

        var content = await response.Content.ReadAsStringAsync(
            cancellationToken);

        var apiResult =
            JsonSerializer.Deserialize<ApiResponse<object>>(
                content,
                JsonOptions);

        if (!response.IsSuccessStatusCode ||
            apiResult is null ||
            !apiResult.Success)
        {
            ViewBag.Error =
                apiResult?.Message ??
                "No se pudo cambiar la contraseña.";

            return View(dto);
        }

        Response.Cookies.Delete("AccessToken");
        Response.Cookies.Delete("RefreshToken");

        HttpContext.Session.Clear();

        TempData["Success"] =
            apiResult.Message ??
            "Contraseña cambiada correctamente. Inicie sesión nuevamente.";

        return RedirectToAction("Login", "Auth");
    }

    [HttpGet]
    public async Task<IActionResult> MySessions(
        CancellationToken cancellationToken)
    {
        var accessToken =
            HttpContext.Session.GetString("AccessToken")
            ?? Request.Cookies["AccessToken"];

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var client = httpClientFactory.CreateClient();

        var endpoint =
            $"{configuration["ApiSettings:BaseUrl"]}" +
            $"{configuration["ApiSettings:MySessionsEndpoint"]}";

        using var request =
            new HttpRequestMessage(HttpMethod.Get, endpoint);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        using var response = await client.SendAsync(
            request,
            cancellationToken);

        var content = await response.Content.ReadAsStringAsync(
            cancellationToken);

        var apiResult =
            JsonSerializer.Deserialize<
                ApiResponse<List<SessionDto>>>(
                content,
                JsonOptions);

        if (!response.IsSuccessStatusCode ||
            apiResult is null ||
            !apiResult.Success)
        {
            ViewBag.Error =
                apiResult?.Message ??
                "No se pudieron cargar las sesiones.";

            return View(new List<SessionDto>());
        }

        return View(
            apiResult.Result ?? new List<SessionDto>());
    }

    [HttpPost]
    public async Task<IActionResult> RevokeSession(
        int refreshTokenId,
        CancellationToken cancellationToken)
    {
        var accessToken =
            HttpContext.Session.GetString("AccessToken")
            ?? Request.Cookies["AccessToken"];

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var client = httpClientFactory.CreateClient();

        var endpoint =
            $"{configuration["ApiSettings:BaseUrl"]}" +
            $"{configuration["ApiSettings:RevokeSessionEndpoint"]}" +
            $"/{refreshTokenId}";

        using var request =
            new HttpRequestMessage(HttpMethod.Post, endpoint);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        await client.SendAsync(
            request,
            cancellationToken);

        return RedirectToAction(
            "MySessions",
            "Auth");
    }

    [HttpPost]
    public async Task<IActionResult> RevokeAllSessions(
        CancellationToken cancellationToken)
    {
        var accessToken =
            HttpContext.Session.GetString("AccessToken")
            ?? Request.Cookies["AccessToken"];

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var client = httpClientFactory.CreateClient();

        var endpoint =
            $"{configuration["ApiSettings:BaseUrl"]}" +
            $"{configuration["ApiSettings:RevokeAllSessionsEndpoint"]}";

        using var request =
            new HttpRequestMessage(HttpMethod.Post, endpoint);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        await client.SendAsync(
            request,
            cancellationToken);

        Response.Cookies.Delete("AccessToken");
        Response.Cookies.Delete("RefreshToken");

        HttpContext.Session.Clear();

        return RedirectToAction("Login", "Auth");
    }

    [HttpPost]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken)
    {
        var refreshToken =
            HttpContext.Session.GetString("RefreshToken")
            ?? Request.Cookies["RefreshToken"];

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var client = httpClientFactory.CreateClient();

            var endpoint =
                $"{configuration["ApiSettings:BaseUrl"]}" +
                "/api/Auth/Logout";

            var payload = new LogoutRequestDto
            {
                RefreshToken = refreshToken
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                endpoint)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    MediaTypeHeaderValue.Parse("application/json"))
            };

            await client.SendAsync(
                request,
                cancellationToken);
        }

        Response.Cookies.Delete("AccessToken");
        Response.Cookies.Delete("RefreshToken");

        HttpContext.Session.Clear();

        return RedirectToAction("Login", "Auth");
    }

    private static string? GetRoleFromJwt(string accessToken)
    {
        try
        {
            var tokenParts = accessToken.Split('.');

            if (tokenParts.Length != 3)
            {
                return null;
            }

            var payload = tokenParts[1]
                .Replace('-', '+')
                .Replace('_', '/');

            while (payload.Length % 4 != 0)
            {
                payload += "=";
            }

            var payloadBytes = Convert.FromBase64String(payload);
            var payloadJson = Encoding.UTF8.GetString(payloadBytes);

            using var document = JsonDocument.Parse(payloadJson);

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (!property.Name.Contains(
                        "role",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    return property.Value.GetString()?.Trim();
                }

                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var role in property.Value.EnumerateArray())
                    {
                        if (role.ValueKind == JsonValueKind.String)
                        {
                            return role.GetString()?.Trim();
                        }
                    }
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
