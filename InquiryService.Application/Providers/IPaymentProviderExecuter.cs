using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Application.Providers
{
    public interface IPaymentProviderExecuter
    {
        Task<ProviderInquiryResult> ExecuteAsync(PaymentInquiryRequest request, CancellationToken cancellationToken);
    }
}
