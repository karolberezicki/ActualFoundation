using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Features.Ucp;

[ModuleDependency(typeof(Infrastructure.Commerce.Initialize))]
public class UcpInitialize : IConfigurableModule
{
    void IConfigurableModule.ConfigureContainer(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<IUcpCheckoutService, UcpCheckoutService>();
        context.Services.AddSingleton<GooglePayPaymentGateway>();
    }

    void IInitializableModule.Initialize(InitializationEngine context)
    {
    }

    void IInitializableModule.Uninitialize(InitializationEngine context)
    {
    }
}