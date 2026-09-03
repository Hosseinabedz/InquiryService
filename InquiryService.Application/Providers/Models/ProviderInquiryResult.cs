namespace InquiryService.Application.Providers.Models
{
    public sealed record ProviderInquiryResult(
        ProviderResultStatus Status,
        decimal? Amount,
        string? ErrorMessage);
}
