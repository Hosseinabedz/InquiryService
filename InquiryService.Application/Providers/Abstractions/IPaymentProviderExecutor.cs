using InquiryService.Application.Providers.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Application.Providers.Abstractions
{
    public interface IPaymentProviderExecutor
    {
        Task<PaymentProviderExecutionResult> ExecuteAsync(PaymentInquiryRequest request, CancellationToken cancellationToken);
    }
}
