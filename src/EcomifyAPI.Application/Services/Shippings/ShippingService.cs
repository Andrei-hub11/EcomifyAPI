using EcomifyAPI.Application.Contracts.Logging;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;

namespace EcomifyAPI.Application.Services.Shippings;

public class ShippingService : IShippingService
{
    private readonly HttpClient _httpClient;
    private readonly ILoggerHelper<ShippingService> _logger;

    public ShippingService(HttpClient httpClient, ILoggerHelper<ShippingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<FreightEstimateResponseDTO>> EstimateShippingAsync(EstimateShippingRequestDTO request,
    CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"https://viacep.com.br/ws/{request.ZipCode}/json/", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Result.Fail("Invalid zip code");
        }

        var shippingCost = CalculateShippingCost(request);
        var estimatedDeliveryDays = CalculateEstimatedDeliveryDays(request);
        var shippingMethod = "PAC";

        return Result.Ok(new FreightEstimateResponseDTO(new MoneyDTO("BRL", shippingCost),
        estimatedDeliveryDays, shippingMethod));
    }

    private static decimal CalculateShippingCost(EstimateShippingRequestDTO request)
    {
        return request.State switch
        {
            "SP" or "São Paulo" => 10,
            "RJ" or "Rio de Janeiro" => 10,
            "MG" or "Minas Gerais" => 10,
            "ES" or "Espírito Santo" => 13,
            "BA" or "Bahia" => 14,
            "PR" or "Paraná" => 14,
            "SC" or "Santa Catarina" => 14,
            "RS" or "Rio Grande do Sul" => 14,
            "AM" or "Amazonas" => 14,
            "PA" or "Pará" => 14,
            "TO" or "Tocantins" => 14,
            "RO" or "Rondônia" => 14,
            "AC" or "Acre" => 14,
            "AP" or "Amapá" => 14,
            "MA" or "Maranhão" => 14,
            "PI" or "Piauí" => 14,
            "CE" or "Ceará" => 14,
            "RN" or "Rio Grande do Norte" => 14,
            "PB" or "Paraíba" => 14,
            "PE" or "Pernambuco" => 14,
            "AL" or "Alagoas" => 14,
            "SE" or "Sergipe" => 14,
            _ => throw new InvalidOperationException("Invalid state")
        };
    }

    private static int CalculateEstimatedDeliveryDays(EstimateShippingRequestDTO request)
    {
        return request.State switch
        {
            "SP" or "São Paulo" => 5,
            "RJ" or "Rio de Janeiro" => 7,
            "MG" or "Minas Gerais" => 6,
            "ES" or "Espírito Santo" => 7,
            "BA" or "Bahia" => 8,
            "PR" or "Paraná" => 9,
            "SC" or "Santa Catarina" => 10,
            "RS" or "Rio Grande do Sul" => 11,
            "AM" or "Amazonas" => 12,
            "PA" or "Pará" => 13,
            "TO" or "Tocantins" => 14,
            "RO" or "Rondônia" => 15,
            "AC" or "Acre" => 16,
            "AP" or "Amapá" => 17,
            "MA" or "Maranhão" => 18,
            "PI" or "Piauí" => 19,
            "CE" or "Ceará" => 20,
            "RN" or "Rio Grande do Norte" => 21,
            "PB" or "Paraíba" => 22,
            "PE" or "Pernambuco" => 23,
            "AL" or "Alagoas" => 24,
            "SE" or "Sergipe" => 25,
            _ => throw new InvalidOperationException("Invalid state")
        };
    }
}