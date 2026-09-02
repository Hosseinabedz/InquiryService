using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Application.Providers
{
    public interface IPaymentProviderExecutor
    {
        Task<PaymentProviderExecutionResult> ExecuteAsync(PaymentInquiryRequest request, CancellationToken cancellationToken);
    }
}
