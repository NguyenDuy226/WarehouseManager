using NetArchTest.Rules;

namespace Warehouse.ArchitectureTests;

public class ArchitectureTests
{
    private const string Domain = "Warehouse.Domain";
    private const string Infrastructure = "Warehouse.Infrastructure";
    private const string App = "Warehouse.Application";
    private const string Api = "Warehouse.Api";
    
    
    [Fact]
    public void Domain_dont_depend()
    {
        var domainAssembly = typeof(Warehouse.Domain.DomainMarker).Assembly;
        var tmp = Types.InAssembly(domainAssembly)
                        .ShouldNot()
                        .HaveDependencyOnAny(Infrastructure, App, Api)
                        .GetResult();
        Assert.True(tmp.IsSuccessful);
    }
    [Fact]
    public void App_dont_depend_on_Api_and_Infras()
    {
        var appAssembly = typeof(Warehouse.Application.ApplicationMarker).Assembly;
        var tmp = Types.InAssembly(appAssembly)
                        .ShouldNot()
                        .HaveDependencyOnAny(Api, Infrastructure)
                        .GetResult();
                    
        Assert.True(tmp.IsSuccessful);
    }
    [Fact]
    public void Infras_dont_depend_on_Api()
    {
       var infrasAssembly = typeof(Warehouse.Infrastructure.InfrastructureMarker).Assembly;
       var tmp = Types.InAssembly(infrasAssembly)
                        .ShouldNot()
                        .HaveDependencyOn(Api)
                        .GetResult();
        Assert.True(tmp.IsSuccessful);
    }
}
