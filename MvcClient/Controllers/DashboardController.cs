using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Uam.AdvancedProgramming.MvcClient.Models;
using Uam.AdvancedProgramming.MvcClient.Models.Dashboard;

namespace Uam.AdvancedProgramming.MvcClient.Controllers;

public class DashboardController(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration)
    : BaseApiController(httpClientFactory, configuration)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var baseUrl = configuration["ApiSettings:BaseUrl"];
        var dashboardEndpoint = configuration["ApiSettings:DashboardBaseEndpoint"];

        if (string.IsNullOrWhiteSpace(baseUrl) ||
            string.IsNullOrWhiteSpace(dashboardEndpoint))
        {
            return View(new DashboardViewModel
            {
                ErrorMessage = "No se encontró la configuración del Dashboard."
            });
        }

        var model = new DashboardViewModel();

        var summaryEndpoint =
            $"{baseUrl}{dashboardEndpoint}/GetGeneralSummary";

        var labsEndpoint =
            $"{baseUrl}{dashboardEndpoint}/GetReportsByLab";

        var statusEndpoint =
            $"{baseUrl}{dashboardEndpoint}/GetReportsByStatus";

        var techniciansEndpoint =
            $"{baseUrl}{dashboardEndpoint}/GetReportsByTechnician";

        var resolutionEndpoint =
            $"{baseUrl}{dashboardEndpoint}/GetAverageResolutionTime";

        var summaryResponse = await SendApiRequestAsync(
            HttpMethod.Get,
            summaryEndpoint,
            null,
            cancellationToken);

        if (summaryResponse is null)
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!summaryResponse.IsSuccessStatusCode)
        {
            model.ErrorMessage = await GetErrorMessageAsync(
                summaryResponse,
                cancellationToken);

            return View(model);
        }

        model.GeneralSummary =
            await ReadResultAsync<GeneralSummaryViewModel>(
                summaryResponse,
                cancellationToken) ?? new GeneralSummaryViewModel();

        var labsResponse = await SendApiRequestAsync(
            HttpMethod.Get,
            labsEndpoint,
            null,
            cancellationToken);

        if (labsResponse is not null && labsResponse.IsSuccessStatusCode)
        {
            model.ReportsByLab =
                await ReadResultAsync<List<ReportsByLabViewModel>>(
                    labsResponse,
                    cancellationToken) ?? [];
        }

        var statusResponse = await SendApiRequestAsync(
            HttpMethod.Get,
            statusEndpoint,
            null,
            cancellationToken);

        if (statusResponse is not null && statusResponse.IsSuccessStatusCode)
        {
            model.ReportsByStatus =
                await ReadResultAsync<List<ReportsByStatusViewModel>>(
                    statusResponse,
                    cancellationToken) ?? [];
        }

        var techniciansResponse = await SendApiRequestAsync(
            HttpMethod.Get,
            techniciansEndpoint,
            null,
            cancellationToken);

        if (techniciansResponse is not null &&
            techniciansResponse.IsSuccessStatusCode)
        {
            model.ReportsByTechnician =
                await ReadResultAsync<List<ReportsByTechnicianViewModel>>(
                    techniciansResponse,
                    cancellationToken) ?? [];
        }

        var resolutionResponse = await SendApiRequestAsync(
            HttpMethod.Get,
            resolutionEndpoint,
            null,
            cancellationToken);

        if (resolutionResponse is not null &&
            resolutionResponse.IsSuccessStatusCode)
        {
            model.AverageResolutionTime =
                await ReadResultAsync<AverageResolutionTimeViewModel>(
                    resolutionResponse,
                    cancellationToken) ?? new AverageResolutionTimeViewModel();
        }

        return View(model);
    }

    private static async Task<T?> ReadResultAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var apiResponse =
            JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions);

        if (apiResponse is null || !apiResponse.Success)
        {
            return default;
        }

        return apiResponse.Result;
    }

    private static async Task<string> GetErrorMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var apiResponse =
            JsonSerializer.Deserialize<ApiResponse<object>>(content, JsonOptions);

        return apiResponse?.Message
               ?? "No fue posible cargar la información del Dashboard.";
    }
}