using InquiryService.Application.Providers;
using InquiryService.Infrastructure.Providers.Mellat;
using InquiryService.Infrastructure.Providers.Saman;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InquiryService.Infrastructure
{
    public static class Extensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            services.Configure<PaymentProviderOptions>(configuration.GetSection("PaymentProviders"));

            services.AddScoped<IPaymentProvider, MellatPaymentProvider>();
            services.AddScoped<IPaymentProvider, SamanPaymentProvider>();

            return services;
        }
    }
}
