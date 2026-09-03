using InquiryService.Application.Abstractions;
using InquiryService.Application.Inquiries;
using InquiryService.Application.Providers.Abstractions;
using InquiryService.Application.Providers.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Application
{
    public static class Extensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(Extensions).Assembly));

            services.AddScoped<IPaymentProviderExecutor, PaymentProviderExecutor>();

            services.AddSingleton<InquiryProcessingLock>();

            return services;
        }
    }
}
