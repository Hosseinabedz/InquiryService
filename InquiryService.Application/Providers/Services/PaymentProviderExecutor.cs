using InquiryService.Application.Providers.Abstractions;
using InquiryService.Application.Providers.Configurations;
using InquiryService.Application.Providers.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Application.Providers.Services
{
    public class PaymentProviderExecutor(
        IEnumerable<IPaymentProvider> providers,
        IOptions<PaymentProviderOptions> options,
        ILogger<PaymentProviderExecutor> logger) : IPaymentProviderExecutor
    {
        private readonly IEnumerable<IPaymentProvider> _providers = providers;
        private readonly ILogger<PaymentProviderExecutor> _logger = logger;
        private readonly IOptions<PaymentProviderOptions >_options = options;

        public async Task<PaymentProviderExecutionResult> ExecuteAsync(PaymentInquiryRequest request, CancellationToken cancellationToken)
        {
            var attempts = new List<ProviderAttemptResult>();

            var providerSettings = _options.Value.Providers
                .OrderBy(x => x.Priority)
                .ToList();

            foreach (var setting in providerSettings)
            {
                var provider = _providers
                    .FirstOrDefault(x => x.Name == setting.Name);

                if (provider is null)
                {
                    _logger.LogWarning("Payment provider {ProviderName} is configured but not registered.", setting.Name);
                    continue;
                }

                _logger.LogInformation("Executing payment inquiry using provider {ProviderName}.", provider.Name);

                var startedAt = DateTime.UtcNow;

                var result = await ExecuteProviderAsync(provider, request, setting.TimeoutSeconds, cancellationToken);

                var completedAt = DateTime.UtcNow;

                attempts.Add(new ProviderAttemptResult(
                    provider.Name,
                    result.Status,
                    result.ErrorMessage,
                    startedAt,
                    completedAt));

                if (result.Status == ProviderResultStatus.Success || result.Status == ProviderResultStatus.BusinessError)
                {
                    return new PaymentProviderExecutionResult(result, attempts);
                }

                _logger.LogWarning("Provider {ProviderName} failed with status {Status}. Trying next provider.", provider.Name, result.Status);
            }

            return new PaymentProviderExecutionResult(
                new ProviderInquiryResult(
                    ProviderResultStatus.TechnicalError,
                    null,
                    "All payment providers are unavailable."),
                attempts);
        }

        private static async Task<ProviderInquiryResult> ExecuteProviderAsync(
            IPaymentProvider provider,
            PaymentInquiryRequest request,
            int timeoutSeconds,
            CancellationToken cancellationToken)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            try
            {
                return await provider.InquiryAsync(request, timeoutCts.Token);
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                return new ProviderInquiryResult(
                    ProviderResultStatus.Timeout,
                    null,
                    $"Provider '{provider.Name}' timed out.");
            }
            catch (Exception ex)
            {
                return new ProviderInquiryResult(
                    ProviderResultStatus.TechnicalError,
                    null,
                    ex.Message);
            }
        }
    }
}
