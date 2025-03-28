﻿using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;

namespace EcomifyAPI.Application.Contracts.Services;

public interface IOrderService
{
    Task<Result<IReadOnlyList<OrderResponseDTO>>> GetOrdersAsync(CancellationToken cancellationToken = default);
    Task<Result<OrderResponseDTO>> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<bool>> CreateOrderAsync(CreateOrderRequestDTO request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default);
}