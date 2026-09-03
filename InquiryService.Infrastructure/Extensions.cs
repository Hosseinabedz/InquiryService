using InquiryService.Application.Abstractions;
using InquiryService.Application.Providers.Abstractions;
using InquiryService.Application.Providers.Configurations;
using InquiryService.Domain.Repositories;
using InquiryService.Infrastructure.Persistence;
using InquiryService.Infrastructure.Persistence.Repositories;
using InquiryService.Infrastructure.Providers.Mellat;
using InquiryService.Infrastructure.Providers.Saman;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InquiryService.Infrastructure
{
    public static class Extensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<AppDbContext>(options =>
               options.UseSqlServer(connectionString));

            services.Configure<PaymentProviderOptions>(configuration.GetSection("PaymentProviders"));

            services.AddScoped<IPaymentProvider, MellatPaymentProvider>();
            services.AddScoped<IPaymentProvider, SamanPaymentProvider>();
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IInquiryRepository, InquiryRepository>();

            return services;
        }
    }
}
