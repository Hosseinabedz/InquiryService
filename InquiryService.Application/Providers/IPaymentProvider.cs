namespace InquiryService.Application.Providers
{
    public interface IPaymentProvider
    {
        string Name { get; }

        Task<ProviderInquiryResult> InquiryAsync(PaymentInquiryRequest request, CancellationToken cancellationToken);
    }
}
