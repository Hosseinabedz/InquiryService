using InquiryService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Application.Providers.Models
{
    public record PaymentProviderExecutionResult(
        ProviderInquiryResult FinalResult,
        IReadOnlyCollection<ProviderAttemptResult> Attempts);

    public record ProviderAttemptResult(
        string ProviderName,
        ProviderResultStatus Status,
        string? ErrorMessage,
        DateTime StartedAt,
        DateTime CompletedAt);
}
