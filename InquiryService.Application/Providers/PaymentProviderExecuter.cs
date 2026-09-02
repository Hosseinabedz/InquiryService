using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Application.Providers
{
    public class PaymentProviderExecuter(
        IEnumerable<IPaymentProvider> providers,
        PaymentProviderOptions options,
        ILogger<PaymentProviderExecuter> logger) : IPaymentProviderExecuter
    {
        private readonly IEnumerable<IPaymentProvider> _providers = providers;
        private readonly ILogger<PaymentProviderExecuter> _logger = logger;
        private readonly PaymentProviderOptions _options = options;

        public async Task<ProviderInquiryResult> ExecuteAsync(PaymentInquiryRequest request, CancellationToken cancellationToken)
        {
            var providerSettings = _options.Providers
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

                var result = await ExecuteProviderAsync(provider, request, setting.TimeoutSeconds, cancellationToken);

                if (result.Status == ProviderResultStatus.Success || result.Status == ProviderResultStatus.BusinessError)
                {
                    return result;
                }

                _logger.LogWarning("Provider {ProviderName} failed with status {Status}. Trying next provider.", provider.Name, result.Status);
            }

            return new ProviderInquiryResult(
            ProviderResultStatus.TechnicalError,
            null,
            "All payment providers are unavailable.");
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
