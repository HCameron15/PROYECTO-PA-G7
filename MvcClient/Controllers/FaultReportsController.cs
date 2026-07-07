using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Uam.AdvancedProgramming.MvcClient.Models;
using Uam.AdvancedProgramming.MvcClient.Models.FaultReports;

namespace Uam.AdvancedProgramming.MvcClient.Controllers;

public class FaultReportsController(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration
) : Controller
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private string BaseUrl =>
        configuration["ApiSettings:BaseUrl"] ?? string.Empty;

    private string FaultReportsEndpoint =>
        configuration["ApiSettings:FaultReportsBaseEndpoint"] ?? "/api/FaultReports";

    private string FaultReportStatusLogsEndpoint =>
        configuration["ApiSettings:FaultReportStatusLogsBaseEndpoint"] ?? "/api/FaultReportStatusLogs";

    private string EquipmentEndpoint =>
        configuration["ApiSettings:EquipmentBaseEndpoint"] ?? "/api/Equipment";

    private string? GetAccessToken()
    {
        return HttpContext.Session.GetString("AccessToken")
            ?? Request.Cookies["AccessToken"];
    }

    private HttpClient CreateAuthorizedClient()
    {
        var client = httpClientFactory.CreateClient();
        var token = GetAccessToken();

        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    private IActionResult? RedirectToLoginIfNeeded()
    {
        var token = GetAccessToken();

        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        return null;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? status,
        CancellationToken cancellationToken)
    {
        var redirect = RedirectToLoginIfNeeded();

        if (redirect is not null)
        {
            return redirect;
        }

        var client = CreateAuthorizedClient();

        var endpoint = string.IsNullOrWhiteSpace(status)
            ? $"{BaseUrl}{FaultReportsEndpoint}/GetAllFaultReports"
            : $"{BaseUrl}{FaultReportsEndpoint}/GetFaultReportsByStatus/{status}";

        using var response = await client.GetAsync(endpoint, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var apiResult = JsonSerializer.Deserialize<ApiResponse<List<FaultReportDto>>>(
            content,
            JsonOptions);

        ViewBag.SelectedStatus = status ?? string.Empty;

        if (!response.IsSuccessStatusCode || apiResult is null || !apiResult.Success)
        {
            ViewBag.Error = apiResult?.Message ?? "No se pudieron cargar los reportes.";
            return View(new List<FaultReportDto>());
        }

        return View(apiResult.Result ?? new List<FaultReportDto>());
    }
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var redirect = RedirectToLoginIfNeeded();

        if (redirect is not null)
        {
            return redirect;
        }

        var model = new CreateFaultReportViewModel();

        var client = CreateAuthorizedClient();

        var endpoint =
            $"{BaseUrl}{EquipmentEndpoint}/GetAllEquipment";

        using var response =
            await client.GetAsync(endpoint, cancellationToken);

        var content =
            await response.Content.ReadAsStringAsync(cancellationToken);

        var apiResult =
            JsonSerializer.Deserialize<ApiResponse<List<EquipmentDto>>>(
                content,
                JsonOptions);

        var equipment =
            apiResult?.Result?
                .Where(x => x.IsActive && x.Status == "Operational")
                .ToList()
            ?? new List<EquipmentDto>();

        model.EquipmentOptions = equipment
            .Select(x => new SelectListItem(
                $"{x.Code} - {x.LaboratoryName}",
                x.Id.ToString()))
            .ToList();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateFaultReportViewModel model,
        CancellationToken cancellationToken)
    {
        var redirect = RedirectToLoginIfNeeded();

        if (redirect is not null)
        {
            return redirect;
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = CreateAuthorizedClient();

        var endpoint =
            $"{BaseUrl}{FaultReportsEndpoint}/CreateFaultReport";

        var payload = new
        {
            model.EquipmentId,
            model.Title,
            model.Description,
            model.Priority
        };

        using var request =
            new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json")
            };

        using var response =
            await client.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            TempData["Message"] =
                "Reporte creado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        var error =
            await response.Content.ReadAsStringAsync(cancellationToken);

        ViewBag.Error = error;

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken)
    {
        var redirect = RedirectToLoginIfNeeded();

        if (redirect is not null)
        {
            return redirect;
        }

        var client = CreateAuthorizedClient();

        var endpoint =
            $"{BaseUrl}{FaultReportsEndpoint}/GetFaultReportById/{id}";

        using var response =
            await client.GetAsync(endpoint, cancellationToken);

        var content =
            await response.Content.ReadAsStringAsync(cancellationToken);

        var apiResult =
            JsonSerializer.Deserialize<ApiResponse<FaultReportDto>>(
                content,
                JsonOptions);

        if (!response.IsSuccessStatusCode ||
            apiResult?.Result is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var report = apiResult.Result;

        var model = new UpdateFaultReportViewModel
        {
            Id = report.Id,
            Title = report.Title,
            Description = report.Description,
            Priority = report.Priority,
            Status = report.Status
        };

        return View(model);
    }
    [HttpPost]
    public async Task<IActionResult> Edit(
    UpdateFaultReportViewModel model,
    CancellationToken cancellationToken)
    {
        var redirect = RedirectToLoginIfNeeded();

        if (redirect is not null)
        {
            return redirect;
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = CreateAuthorizedClient();

        var endpoint =
            $"{BaseUrl}{FaultReportsEndpoint}/UpdateFaultReport/{model.Id}";

        var payload = new
        {
            model.Title,
            model.Description,
            model.Priority,
            model.Status
        };

        using var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json")
        };

        using var response =
            await client.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            TempData["Message"] = "Reporte actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        var content =
            await response.Content.ReadAsStringAsync(cancellationToken);

        ViewBag.Error = content;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Close(
        int id,
        CancellationToken cancellationToken)
    {
        var redirect = RedirectToLoginIfNeeded();

        if (redirect is not null)
        {
            return redirect;
        }

        var client = CreateAuthorizedClient();

        var endpoint =
            $"{BaseUrl}{FaultReportsEndpoint}/CloseFaultReport/{id}";

        using var response =
            await client.PostAsync(endpoint, null, cancellationToken);

        TempData["Message"] = response.IsSuccessStatusCode
            ? "Reporte cerrado correctamente."
            : "No fue posible cerrar el reporte.";

        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Details(
    int id,
    CancellationToken cancellationToken)
    {
        var redirect = RedirectToLoginIfNeeded();

        if (redirect is not null)
        {
            return redirect;
        }

        var client = CreateAuthorizedClient();

        var reportEndpoint =
            $"{BaseUrl}{FaultReportsEndpoint}/GetFaultReportById/{id}";

        var logsEndpoint =
            $"{BaseUrl}{FaultReportStatusLogsEndpoint}/GetLogsByFaultReport/{id}";

        using var reportResponse =
            await client.GetAsync(reportEndpoint, cancellationToken);

        using var logsResponse =
            await client.GetAsync(logsEndpoint, cancellationToken);

        var reportContent =
            await reportResponse.Content.ReadAsStringAsync(cancellationToken);

        var logsContent =
            await logsResponse.Content.ReadAsStringAsync(cancellationToken);

        var reportResult =
            JsonSerializer.Deserialize<ApiResponse<FaultReportDto>>(
                reportContent,
                JsonOptions);

        var logsResult =
            JsonSerializer.Deserialize<ApiResponse<List<FaultReportStatusLogDto>>>(
                logsContent,
                JsonOptions);

        if (!reportResponse.IsSuccessStatusCode ||
            reportResult is null ||
            !reportResult.Success ||
            reportResult.Result is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var model = new FaultReportDetailsViewModel
        {
            Report = reportResult.Result,
            Logs = logsResult?.Result ?? new List<FaultReportStatusLogDto>()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Assign(
        int id,
        CancellationToken cancellationToken)
    {
        var redirect = RedirectToLoginIfNeeded();

        if (redirect is not null)
        {
            return redirect;
        }

        var client = CreateAuthorizedClient();

        var endpoint =
            $"{BaseUrl}{FaultReportsEndpoint}/AssignFaultReport/{id}";

        using var response =
            await client.PostAsync(endpoint, null, cancellationToken);

        TempData["Message"] = response.IsSuccessStatusCode
            ? "Reporte asignado correctamente."
            : "No fue posible asignar el reporte.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> UpdateStatus(
        int id,
        CancellationToken cancellationToken)
    {
        var redirect = RedirectToLoginIfNeeded();

        if (redirect is not null)
        {
            return redirect;
        }

        var client = CreateAuthorizedClient();

        var endpoint =
            $"{BaseUrl}{FaultReportsEndpoint}/GetFaultReportById/{id}";

        using var response =
            await client.GetAsync(endpoint, cancellationToken);

        var content =
            await response.Content.ReadAsStringAsync(cancellationToken);

        var apiResult =
            JsonSerializer.Deserialize<ApiResponse<FaultReportDto>>(
                content,
                JsonOptions);

        if (!response.IsSuccessStatusCode ||
            apiResult?.Result is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var model = new UpdateFaultReportStatusViewModel
        {
            FaultReportId = apiResult.Result.Id,
            CurrentStatus = apiResult.Result.Status
        };

        if (apiResult.Result.Status == "InProgress")
        {
            model.ValidStatuses.Add("Resolved");
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(
        UpdateFaultReportStatusViewModel model,
        CancellationToken cancellationToken)
    {
        var redirect = RedirectToLoginIfNeeded();

        if (redirect is not null)
        {
            return redirect;
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = CreateAuthorizedClient();

        var endpoint =
            $"{BaseUrl}{FaultReportsEndpoint}/UpdateFaultReportStatus/{model.FaultReportId}";

        var payload = new
        {
            model.NewStatus,
            model.Notes
        };

        using var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json")
        };

        using var response =
            await client.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            TempData["Message"] = "Estado actualizado correctamente.";

            return RedirectToAction(nameof(Details),
                new { id = model.FaultReportId });
        }

        ViewBag.Error =
            await response.Content.ReadAsStringAsync(cancellationToken);

        return View(model);
    }
}