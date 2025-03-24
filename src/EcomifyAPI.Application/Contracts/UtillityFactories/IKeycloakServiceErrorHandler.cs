using EcomifyAPI.Common.Utils.Result;

namespace EcomifyAPI.Application.Contracts.UtillityFactories;

public interface IKeycloakServiceErrorHandler
{
    Task<Result> ExtractErrorFromResponse(HttpResponseMessage response);
}