using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Uam.AdvancedProgramming.MvcClient.Models;

namespace Uam.AdvancedProgramming.MvcClient.Controllers;

public class RolesController : BaseApiController
{
    private readonly IConfiguration _configuration;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RolesController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration
    ) : base(httpClientFactory, configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        var accessToken = HttpContext.Session.GetString("AccessToken")
                          ?? Request.Cookies["AccessToken"];

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var endpoint = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:RolesBaseEndpoint"]}/GetAllRoles";

        var response = await SendApiRequestAsync(HttpMethod.Get, endpoint, null, cancellationToken);

        if (response is null)
        {
            return Unauthorized();
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, content);
        }

        var apiResult = JsonSerializer.Deserialize<ApiResponse<List<RoleDto>>>(content, JsonOptions);

        return Json(apiResult?.Result ?? new List<RoleDto>());
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] RoleUpsertDto dto, CancellationToken cancellationToken)
    {
        var endpoint = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:RolesBaseEndpoint"]}/CreateRole";

        var response = await SendApiRequestAsync(HttpMethod.Post, endpoint, dto, cancellationToken);

        if (response is null)
        {
            return Unauthorized();
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return StatusCode((int)response.StatusCode, content);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleUpsertDto dto, CancellationToken cancellationToken)
    {
        var endpoint = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:RolesBaseEndpoint"]}/UpdateRole/{id}";

        var response = await SendApiRequestAsync(HttpMethod.Put, endpoint, dto, cancellationToken);

        if (response is null)
        {
            return Unauthorized();
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return StatusCode((int)response.StatusCode, content);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteRole(int id, CancellationToken cancellationToken)
    {
        var endpoint = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:RolesBaseEndpoint"]}/DeleteRole/{id}";

        var response = await SendApiRequestAsync(HttpMethod.Delete, endpoint, null, cancellationToken);

        if (response is null)
        {
            return Unauthorized();
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return StatusCode((int)response.StatusCode, content);
    }
}