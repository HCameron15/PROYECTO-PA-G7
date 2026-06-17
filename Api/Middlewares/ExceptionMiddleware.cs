using Uam.AdvancedProgramming.Api.DTOs;

namespace Uam.AdvancedProgramming.Api.Middlewares;

/// <summary>
/// Middleware para manejar excepciones no controladas y responder JSON uniforme.
/// </summary>
public class ExceptionMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Ejecuta el siguiente middleware y captura errores globales.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new ApiOperationResultDto<object>
            {
                Success = false,
                Code = StatusCodes.Status500InternalServerError.ToString(),
                Message = ex.Message,
                Result = null
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
