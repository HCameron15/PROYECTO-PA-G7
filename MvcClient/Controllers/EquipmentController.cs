using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Uam.AdvancedProgramming.MvcClient.Models;

namespace Uam.AdvancedProgramming.MvcClient.Controllers;

public class EquipmentController : BaseApiController
{
    private readonly IConfiguration _configuration;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EquipmentController(
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
    public async Task<IActionResult> GetEquipment(CancellationToken cancellationToken)
    {
        var endpoint = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:EquipmentBaseEndpoint"]}/GetAllEquipment";

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

        var apiResult = JsonSerializer.Deserialize<ApiResponse<List<EquipmentDto>>>(content, JsonOptions);

        return Json(apiResult?.Result ?? new List<EquipmentDto>());
    }

    [HttpGet]
    public async Task<IActionResult> GetEquipmentByLaboratory(int id, CancellationToken cancellationToken)
    {
        var endpoint = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:EquipmentBaseEndpoint"]}/GetEquipmentByLaboratory/{id}";

        var response = await SendApiRequestAsync(HttpMethod.Get, endpoint, null, cancellationToken);

        if (response is null)
        {
            return Json(new List<EquipmentDto>());
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Json(new List<EquipmentDto>());
        }

        var apiResult = JsonSerializer.Deserialize<ApiResponse<List<EquipmentDto>>>(content, JsonOptions);

        return Json(apiResult?.Result ?? new List<EquipmentDto>());
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

        var activeLabs = apiResult?.Result?.Where(x => x.IsActive).ToList()
                         ?? new List<LaboratoryDto>();

        return Json(activeLabs);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEquipment([FromBody] EquipmentUpsertDto dto, CancellationToken cancellationToken)
    {
        var endpoint = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:EquipmentBaseEndpoint"]}/CreateEquipment";

        var response = await SendApiRequestAsync(HttpMethod.Post, endpoint, dto, cancellationToken);

        if (response is null)
        {
            return Unauthorized();
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return StatusCode((int)response.StatusCode, content);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateEquipment(int id, [FromBody] EquipmentUpsertDto dto, CancellationToken cancellationToken)
    {
        var endpoint = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:EquipmentBaseEndpoint"]}/UpdateEquipment/{id}";

        var response = await SendApiRequestAsync(HttpMethod.Put, endpoint, dto, cancellationToken);

        if (response is null)
        {
            return Unauthorized();
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return StatusCode((int)response.StatusCode, content);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteEquipment(int id, CancellationToken cancellationToken)
    {
        var endpoint = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:EquipmentBaseEndpoint"]}/DeleteEquipment/{id}";

        var response = await SendApiRequestAsync(HttpMethod.Delete, endpoint, null, cancellationToken);

        if (response is null)
        {
            return Unauthorized();
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return StatusCode((int)response.StatusCode, content);
    }
}