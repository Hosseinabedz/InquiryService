using InquiryService.Application.Providers.Abstractions;
using InquiryService.Application.Providers.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Infrastructure.Providers.Mellat
{
    public class MellatPaymentProvider : IPaymentProvider
    {
        public string Name => "Mellat";

        public async Task<ProviderInquiryResult> InquiryAsync(PaymentInquiryRequest request, CancellationToken cancellationToken)
        {
            // Call external provider
            await Task.Delay(100, cancellationToken);

            return new ProviderInquiryResult(
                Status: ProviderResultStatus.Success,
                Amount: 120000,
                ErrorMessage: null
                );
        }
    }
}
