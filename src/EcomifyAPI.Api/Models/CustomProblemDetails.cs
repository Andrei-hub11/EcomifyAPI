using EcomifyAPI.Contracts.Models;

using Microsoft.AspNetCore.Mvc;

namespace EcomifyAPI.Api.Models;

public sealed class CustomProblemDetails : ProblemDetails
{
    public Dictionary<string, ValidationErrorDetail[]> Errors { get; set; }
    public CustomProblemDetails(Dictionary<string, ValidationErrorDetail[]> errors)
    {
        Errors = errors;
    }

}