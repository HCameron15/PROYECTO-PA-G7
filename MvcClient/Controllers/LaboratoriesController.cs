using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Uam.AdvancedProgramming.MvcClient.Models;

namespace Uam.AdvancedProgramming.MvcClient.Controllers;

public class LaboratoriesController : BaseApiController
{
    private readonly IConfiguration _configuration;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public LaboratoriesController(
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
    public async Task<IActionResult> GetLaboratories(CancellationToken cancellationToken)
    {
        var endpoint = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:LaboratoriesBaseEndpoint"]}/GetAllLaboratories";

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

        var apiResult = JsonSerializer.Deserialize<ApiResponse<List<LaboratoryDto>>>(content, JsonOptions);

        return Json(apiResult?.Result ?? new List<LaboratoryDto>());
    }

    [HttpPost]
    public async Task<IActionResult> CreateLaboratory([FromBody] LaboratoryUpsertDto dto, CancellationToken cancellationToken)
    {
        var endpoint = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:LaboratoriesBaseEndpoint"]}/CreateLaboratory";

        var response = await SendApiRequestAsync(HttpMethod.Post, endpoint, dto, cancellationToken);

        if (response is null)
        {
            return Unauthorized();
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return StatusCode((int)response.StatusCode, content);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateLaboratory(int id, [FromBody] LaboratoryUpsertDto dto, CancellationToken cancellationToken)
    {
        var endpoint = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:LaboratoriesBaseEndpoint"]}/UpdateLaboratory/{id}";

        var response = await SendApiRequestAsync(HttpMethod.Put, endpoint, dto, cancellationToken);

        if (response is null)
        {
            return Unauthorized();
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return StatusCode((int)response.StatusCode, content);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteLaboratory(int id, CancellationToken cancellationToken)
    {
        var endpoint = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:LaboratoriesBaseEndpoint"]}/DeleteLaboratory/{id}";

        var response = await SendApiRequestAsync(HttpMethod.Delete, endpoint, null, cancellationToken);

        if (response is null)
        {
            return Unauthorized();
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return StatusCode((int)response.StatusCode, content);
    }
}