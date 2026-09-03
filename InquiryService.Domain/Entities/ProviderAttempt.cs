using InquiryService.Domain.Enums;

namespace InquiryService.Domain.Entities
{
    public class ProviderAttempt
    {
        public Guid Id { get; private set; }
        public Guid InquiryId { get; private set; }
        public ProviderAttemptStatus Status { get; private set; }
        public string ProviderName { get; private set; }
        public string? ErrorMessage { get; private set; }
        public DateTime StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        private ProviderAttempt() { }

        private ProviderAttempt(Guid inquiryId, string providerName, ProviderAttemptStatus status, DateTime startedAt, DateTime completedAt, string? errorMessage)
        {
            Id = Guid.NewGuid();
            InquiryId = inquiryId;
            Status = status;
            ProviderName = providerName;
            StartedAt = startedAt;
            CompletedAt = completedAt;
            ErrorMessage = errorMessage;
        }

        public static ProviderAttempt Create(Guid inquiryId, string providerName, ProviderAttemptStatus status, DateTime startedAt, DateTime completedAt, string? errorMessage)
        {
            if (inquiryId == Guid.Empty)
                throw new ArgumentException("Inquiry id cannot be empty.", nameof(inquiryId));

            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("Provider name cannot be empty.", nameof(providerName));

            return new ProviderAttempt(inquiryId, providerName, status, startedAt, completedAt, errorMessage);
        }
    }
}
