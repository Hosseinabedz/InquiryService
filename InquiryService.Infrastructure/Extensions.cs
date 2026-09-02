using InquiryService.Application.Providers;
using InquiryService.Infrastructure.Providers.Mellat;
using InquiryService.Infrastructure.Providers.Saman;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Infrastructure
{
    public static class Extensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IPaymentProvider, MellatPaymentProvider>();
            services.AddScoped<IPaymentProvider, SamanPaymentProvider>();

            return services;
        }
    }
}
