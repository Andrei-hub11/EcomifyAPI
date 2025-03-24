using System.Data;

namespace EcomifyAPI.Application.Contracts.Repositories;

public interface IRepository
{
    void Initialize(IDbConnection connection, IDbTransaction transaction);
}