using System.Reflection;

using NetArchTest.Rules;

using Shouldly;

namespace EcomifyAPI.UnitTests.Architecture;

public class ArchitectureTests
{

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        // verify if Infrastructure not depends on Api
        var shouldNotDependResult = Types.InAssembly(typeof(Infrastructure.ServiceCollectionExtensions).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("EcomifyAPI.Api").GetResult();

        shouldNotDependResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void On_Api_Only_Program_Should_Depend_On_Infrastructure()
    {
        var onlyProgramResult = Types.InAssembly(typeof(Api.DependencyInjection.ServiceCollection).Assembly)
         .That()
         .HaveDependencyOn("EcomifyAPI.Infrastructure")
         .Should()
         .HaveNameEndingWith("Program")
         .GetResult();

        onlyProgramResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Domain_Should_Only_Depend_On_Common()
    {
        // verify if Domain not depends on forbidden layers
        var shouldNotDependResult = Types.InAssembly(typeof(Domain.Entities.Order).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "EcomifyAPI.Api",
                "EcomifyAPI.Application",
                "EcomifyAPI.Infrastructure",
                "EcomifyAPI.Contracts",
                "EcomifyAPI.Application"
            ).GetResult();

        var allowedNamespaces = new[]
     {
        "System",
        "EcomifyAPI.Domain",
        "EcomifyAPI.Common"
    };

        var allReferencedNamespaces = GetAllReferencedNamespaces(typeof(Domain.Entities.Order).Assembly);

        // verify if all referenced namespaces are in the allowed list
        var hasOnlyAllowedDependencies = allReferencedNamespaces.All(ns =>
            allowedNamespaces.Any(allowed => ns.StartsWith(allowed)));

        shouldNotDependResult.IsSuccessful.ShouldBeTrue();
        hasOnlyAllowedDependencies.ShouldBeTrue();
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        var shouldNotDependResult = Types.InAssembly(typeof(Application.Services.Products.ProductService).Assembly)
            .ShouldNot()
            .HaveDependencyOn("EcomifyAPI.Infrastructure").GetResult();

        shouldNotDependResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Common_Should_Depend_Be_Independent()
    {
        // verify if Common is independent
        var shouldNotDependResult = Types.InAssembly(typeof(Common.Validation.PasswordValidation).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "EcomifyAPI.Api",
                "EcomifyAPI.Application",
                "EcomifyAPI.Infrastructure",
                "EcomifyAPI.Contracts",
                "EcomifyAPI.Domain"
            ).GetResult();

        shouldNotDependResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Contracts_Should_Be_Independent()
    {
        var shouldNotDependResult = Types.InAssembly(typeof(Contracts.Request.CreateCategoryRequestDTO).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "EcomifyAPI.Api",
                "EcomifyAPI.Application",
                "EcomifyAPI.Infrastructure",
                "EcomifyAPI.Domain",
                "EcomifyAPI.Common"
            ).GetResult();

        shouldNotDependResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Services_Should_Use_UnitOfWork_Instead_Of_Direct_Repositories()
    {
        var shouldNotDependResult = Types.InAssembly(typeof(Application.Services.Products.ProductService).Assembly)
            .ShouldNot()
            .HaveDependencyOn("EcomifyAPI.Infrastructure.Persistence")
            .And()
            .HaveDependencyOn("EcomifyAPI.Application.Contracts.Data")
            .GetResult();

        shouldNotDependResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Controllers_Should_Not_Depend_Directly_On_Repositories()
    {
        var shouldNotDependOnRepositories = Types.InAssembly(typeof(Api.Controllers.ProductController).Assembly)
            .That()
            .AreClasses()
            .And()
            .HaveNameEndingWith("Controller")
            .ShouldNot()
            .HaveDependencyOn("EcomifyAPI.Infrastructure.Persistence")
            .GetResult();

        shouldNotDependOnRepositories.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Controllers_Should_Only_Depend_On_Application_Or_Contracts()
    {
        var controllersTypes = Types.InAssembly(typeof(Api.Controllers.ProductController).Assembly)
            .That()
            .AreClasses()
            .And()
            .HaveNameEndingWith("Controller")
            .GetTypes();

        var allowedNamespaces = new List<string>
        {
            "System",
            "Microsoft",
            "EcomifyAPI.Api.Extensions",
            "EcomifyAPI.Api.Middleware",
            "EcomifyAPI.Application",
            "EcomifyAPI.Contracts"
        };

        foreach (var controllerType in controllersTypes)
        {
            controllerType.AssertHasValidDependencies(allowedNamespaces);
        }
    }

    /*     [Fact]
        public void DTOs_Should_Only_Have_Properties()
        {
            var allowedNamespaces = new List<string>
            {
                "EcomifyAPI.Contracts.Request",
                "EcomifyAPI.Contracts.Response",
                "EcomifyAPI.Contracts.Enums"
            };

            var dtoTypes = Assembly.GetAssembly(typeof(Contracts.Request.CreateCategoryRequestDTO))!
                           .GetTypes()
                           .Where(t => t.Name.EndsWith("DTO") && t.IsClass
                           && allowedNamespaces.Any(ns => t.Namespace?.StartsWith(ns) ?? false));

            foreach (var dto in dtoTypes)
            {
                if (!dto.IsSealed)
                {
                    throw new Exception($"❌ [ERRO] The DTO {dto.Name} is not sealed.");
                }

                if (!dto.ContainsOnlyProperties())
                {
                    throw new Exception($"❌ [ERRO] The DTO {dto.Name} contains methods or fields that are not allowed.");
                }
            }
        } */

    private static IEnumerable<string> GetAllReferencedNamespaces(Assembly assembly)
    {
        return assembly.GetReferencedAssemblies()
            .Select(a => a.FullName.Split(',')[0])
            .Distinct();
    }
}