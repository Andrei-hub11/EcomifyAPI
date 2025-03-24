using System.Text;

using EcomifyAPI.Contracts.Models;

using Microsoft.AspNetCore.Mvc;

namespace EcomifyAPI.Api.Utils;

public class ExceptionDetailsHelper
{
    public static string GetExceptionDetails(Exception ex, HttpContext context)
    {
        var exceptionDetails = new StringBuilder();
        exceptionDetails.AppendLine($"[Error] Path: {context.Request.Path}");
        exceptionDetails.AppendLine($"[Error] Method: {context.Request.Method}");
        exceptionDetails.AppendLine($"[Error] Exception Type: {ex.GetType().FullName}");
        exceptionDetails.AppendLine($"[Error] Message: {ex.Message}");
        exceptionDetails.AppendLine($"[Error] Stack Trace: {ex.StackTrace}");

        if (ex.InnerException != null)
        {
            exceptionDetails.AppendLine("[Error] Inner Exception:");
            exceptionDetails.AppendLine($"[Error] Type: {ex.InnerException.GetType().FullName}");
            exceptionDetails.AppendLine($"[Error] Message: {ex.InnerException.Message}");
            exceptionDetails.AppendLine($"[Error] Stack Trace: {ex.InnerException.StackTrace}");
        }

        return exceptionDetails.ToString();
    }

    public static string GetBadRequestDetails(
        BadRequestObjectResult badRequestResult,
        HttpContext context
    )
    {
        var details = new StringBuilder();

        var statusCode = badRequestResult.StatusCode;

        details.AppendLine($"Erro ao processar a solicitação na rota '{context.Request.Path}'.");
        details.AppendLine($"Código HTTP: {statusCode ?? 400}");

        // Extract and format the errors if any
        if (badRequestResult.Value is ValidationErrorDetails validationErrors)
        {
            details.AppendLine("Detalhes dos erros de validação:");

            foreach (var error in validationErrors.Errors)
            {
                details.AppendLine($"Campo: {error.Key}");
                foreach (var validationError in error.Value)
                {
                    details.AppendLine(
                        $"- Código: {validationError.Code}, Mensagem: {validationError.Description}"
                    );
                }
            }
        }
        else
        {
            details.AppendLine($"Mensagem de erro: {badRequestResult.Value?.ToString()}");
        }

        return details.ToString();
    }

    public static string GetProblemDetails(ProblemDetails problemDetails, HttpContext context)
    {
        var details = new StringBuilder();

        details.AppendLine($"Erro ao processar a solicitação na rota '{context.Request.Path}'.");
        details.AppendLine(
            $"Código HTTP: {problemDetails.Status ?? StatusCodes.Status500InternalServerError}"
        );
        details.AppendLine($"Título: {problemDetails.Title ?? "Erro desconhecido"}");
        details.AppendLine(
            $"Detalhes: {problemDetails.Detail ?? "Nenhuma informação adicional disponível."}"
        );
        details.AppendLine($"Instância: {$"{context.Request.Method} {context.Request.Path}"}");

        if (problemDetails.Extensions != null && problemDetails.Extensions.Count > 0)
        {
            details.AppendLine("Informações adicionais:");
            foreach (var extension in problemDetails.Extensions)
            {
                details.AppendLine($"- {extension.Key}: {extension.Value}");
            }
        }

        return details.ToString();
    }
}