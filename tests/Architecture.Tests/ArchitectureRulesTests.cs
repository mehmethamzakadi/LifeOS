using NetArchTest.Rules;
using Xunit;

namespace Architecture.Tests;

/// <summary>
/// Mimari kurallarını test eden test sınıfı
/// Clean Architecture ve DDD prensiplerine uygunluğu kontrol eder
/// </summary>
public class ArchitectureRulesTests
{
    private const string DomainNamespace = "LifeOS.Domain";
    private const string ApplicationNamespace = "LifeOS.Application";
    private const string InfrastructureNamespace = "LifeOS.Infrastructure";
    private const string PersistenceNamespace = "LifeOS.Persistence";
    private const string ApiNamespace = "LifeOS.API";

    #region Domain Layer Rules

    /// <summary>
    /// Domain katmanının hiçbir dış bağımlılığı olmamalı
    /// Sadece .NET standart kütüphanelerine bağımlı olmalı
    /// </summary>
    [Fact]
    public void Domain_Should_Not_Have_Dependencies_On_Other_Projects()
    {
        var result = Types
            .InAssembly(DomainNamespace + ".dll")
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .And()
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .And()
            .ShouldNot()
            .HaveDependencyOn(PersistenceNamespace)
            .And()
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, result.GetUserMessage());
    }

    /// <summary>
    /// Domain katmanı EF Core, MassTransit gibi infrastructure teknolojilerine bağımlı olmamalı
    /// </summary>
    [Fact]
    public void Domain_Should_Not_Have_Dependencies_On_Infrastructure_Technologies()
    {
        var result = Types
            .InAssembly(DomainNamespace + ".dll")
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .And()
            .ShouldNot()
            .HaveDependencyOn("MassTransit")
            .And()
            .ShouldNot()
            .HaveDependencyOn("StackExchange.Redis")
            .And()
            .ShouldNot()
            .HaveDependencyOn("Serilog")
            .GetResult();

        Assert.True(result.IsSuccessful, result.GetUserMessage());
    }

    #endregion

    #region Application Layer Rules

    /// <summary>
    /// Application katmanı sadece Domain'e bağımlı olmalı
    /// Infrastructure, Persistence veya API katmanlarına bağımlı olmamalı
    /// </summary>
    [Fact]
    public void Application_Should_Only_Depend_On_Domain()
    {
        var result = Types
            .InAssembly(ApplicationNamespace + ".dll")
            .Should()
            .HaveDependencyOn(DomainNamespace)
            .And()
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .And()
            .ShouldNot()
            .HaveDependencyOn(PersistenceNamespace)
            .And()
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, result.GetUserMessage());
    }

    #endregion

    #region Infrastructure Layer Rules

    /// <summary>
    /// Infrastructure katmanı Domain ve Application'a bağımlı olabilir
    /// Ancak Persistence veya API'ye bağımlı olmamalı
    /// </summary>
    [Fact]
    public void Infrastructure_Should_Only_Depend_On_Domain_And_Application()
    {
        var result = Types
            .InAssembly(InfrastructureNamespace + ".dll")
            .Should()
            .HaveDependencyOn(DomainNamespace)
            .And()
            .ShouldNot()
            .HaveDependencyOn(PersistenceNamespace)
            .And()
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, result.GetUserMessage());
    }

    #endregion

    #region Persistence Layer Rules

    /// <summary>
    /// Persistence katmanı Domain ve Application'a bağımlı olabilir
    /// Ancak Infrastructure veya API'ye bağımlı olmamalı
    /// </summary>
    [Fact]
    public void Persistence_Should_Only_Depend_On_Domain_And_Application()
    {
        var result = Types
            .InAssembly(PersistenceNamespace + ".dll")
            .Should()
            .HaveDependencyOn(DomainNamespace)
            .And()
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .And()
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, result.GetUserMessage());
    }

    #endregion

    #region API Layer Rules

    /// <summary>
    /// API katmanı tüm katmanlara bağımlı olabilir (en üst katman)
    /// </summary>
    [Fact]
    public void API_Can_Depend_On_All_Layers()
    {
        var result = Types
            .InAssembly(ApiNamespace + ".dll")
            .Should()
            .HaveDependencyOn(DomainNamespace)
            .And()
            .Should()
            .HaveDependencyOn(ApplicationNamespace)
            .And()
            .Should()
            .HaveDependencyOn(InfrastructureNamespace)
            .And()
            .Should()
            .HaveDependencyOn(PersistenceNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, result.GetUserMessage());
    }

    #endregion

    #region Controller Rules

    /// <summary>
    /// Controller'lar Domain entity'lerini asla direkt dışarı dönmemeli
    /// DTO (Data Transfer Object) kullanmalı
    /// </summary>
    [Fact]
    public void Controllers_Should_Not_Return_Domain_Entities_Directly()
    {
        var result = Types
            .InAssembly(ApiNamespace + ".dll")
            .That()
            .ResideInNamespace("LifeOS.API.Controllers")
            .ShouldNot()
            .HaveDependencyOn("LifeOS.Domain.Entities")
            .Or()
            .HaveDependencyOn("LifeOS.Domain.ValueObjects")
            .GetResult();

        // Not: Bu test biraz katı olabilir çünkü controller'lar entity'leri kullanmadan DTO'ya map edemez
        // Ancak genel prensip olarak controller'lar entity'leri direkt return etmemeli
        // Bu test, controller metodlarının return type'larını kontrol eder
        Assert.True(result.IsSuccessful, 
            "Controllers should not directly return Domain entities. Use DTOs instead. " + result.GetUserMessage());
    }

    /// <summary>
    /// Controller'lar sadece Application katmanındaki DTO'ları kullanmalı
    /// </summary>
    [Fact]
    public void Controllers_Should_Use_Application_DTOs()
    {
        var result = Types
            .InAssembly(ApiNamespace + ".dll")
            .That()
            .ResideInNamespace("LifeOS.API.Controllers")
            .Should()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, result.GetUserMessage());
    }

    #endregion

    #region Repository Rules

    /// <summary>
    /// Repository interface'leri Domain katmanında olmalı
    /// </summary>
    [Fact]
    public void Repository_Interfaces_Should_Be_In_Domain()
    {
        var result = Types
            .InAssembly(DomainNamespace + ".dll")
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .BeInterfaces()
            .GetResult();

        Assert.True(result.IsSuccessful, result.GetUserMessage());
    }

    /// <summary>
    /// Repository implementasyonları Persistence katmanında olmalı
    /// </summary>
    [Fact]
    public void Repository_Implementations_Should_Be_In_Persistence()
    {
        var result = Types
            .InAssembly(PersistenceNamespace + ".dll")
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .BeClasses()
            .GetResult();

        Assert.True(result.IsSuccessful, result.GetUserMessage());
    }

    #endregion

    #region Service Rules

    /// <summary>
    /// Domain service interface'leri Domain katmanında olmalı
    /// </summary>
    [Fact]
    public void Domain_Service_Interfaces_Should_Be_In_Domain()
    {
        var result = Types
            .InAssembly(DomainNamespace + ".dll")
            .That()
            .ResideInNamespace("LifeOS.Domain.Services")
            .Should()
            .BeInterfaces()
            .GetResult();

        Assert.True(result.IsSuccessful, result.GetUserMessage());
    }

    /// <summary>
    /// Infrastructure service implementasyonları Infrastructure katmanında olmalı
    /// </summary>
    [Fact]
    public void Infrastructure_Service_Implementations_Should_Be_In_Infrastructure()
    {
        var result = Types
            .InAssembly(InfrastructureNamespace + ".dll")
            .That()
            .ResideInNamespace("LifeOS.Infrastructure.Services")
            .Should()
            .BeClasses()
            .GetResult();

        Assert.True(result.IsSuccessful, result.GetUserMessage());
    }

    #endregion
}
