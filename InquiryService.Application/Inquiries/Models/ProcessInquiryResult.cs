using InquiryService.Domain.Enums;

namespace InquiryService.Application.Inquiries.Models
{
    public sealed record ProcessInquiryResult(
        Guid InquiryId,
        InquiryStatus Status,
        decimal? Amount,
        string? Result);
}
