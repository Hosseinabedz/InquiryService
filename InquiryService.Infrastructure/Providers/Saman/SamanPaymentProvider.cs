using InquiryService.Application.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Infrastructure.Providers.Saman
{
    public class SamanPaymentProvider : IPaymentProvider
    {
        public string Name => "Saman";

        public async Task<ProviderInquiryResult> InquiryAsync(PaymentInquiryRequest request, CancellationToken cancellationToken)
        {
            // Call external provider
            await Task.Delay(100, cancellationToken);

            return new ProviderInquiryResult(
                Status: ProviderResultStatus.TechnicalError,
                Amount: null,
                ErrorMessage: "Saman provider is temporarily unavailable.");

        }
    }
}
