using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Application.Providers
{
    public interface IPaymentProviderExecutor
    {
        Task<ProviderInquiryResult> ExecuteAsync(PaymentInquiryRequest request, CancellationToken cancellationToken);
    }
}
