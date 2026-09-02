namespace InquiryService.Application.Providers
{
    public sealed record ProviderInquiryResult(
        ProviderResultStatus Status,
        decimal? Amount,
        string? ErrorMessage);
}
