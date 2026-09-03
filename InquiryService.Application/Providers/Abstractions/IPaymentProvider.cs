using InquiryService.Application.Providers.Models;

namespace InquiryService.Application.Providers.Abstractions
{
    public interface IPaymentProvider
    {
        string Name { get; }

        Task<ProviderInquiryResult> InquiryAsync(PaymentInquiryRequest request, CancellationToken cancellationToken);
    }
}
