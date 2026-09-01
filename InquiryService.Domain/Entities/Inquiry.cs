using InquiryService.Domain.Enums;

namespace InquiryService.Domain.Entities
{
    public class Inquiry
    {
        public Guid Id { get; private set; }
        public string BillId { get; private set; }
        public decimal? Amount { get; private set; }
        public InquiryStatus Status { get; private set; }
        public string? Result { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        private readonly List<ProviderAttempt> _providerAttempts = [];
        public IReadOnlyCollection<ProviderAttempt> ProviderAttempts => _providerAttempts.AsReadOnly();


        private Inquiry() { }
        public Inquiry(Guid id, string billId, DateTime createdAt)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Inquiry id cannot be empty.", nameof(id));

            if (string.IsNullOrWhiteSpace(billId))
                throw new ArgumentException("Bill id cannot be empty.", nameof(billId));

            Id = id;
            BillId = billId;
            CreatedAt = createdAt;
            Status = InquiryStatus.Pending;
        }

        public void StartProcessing()
        {
            if (Status != InquiryStatus.Pending)
                throw new InvalidOperationException(
                    "Only pending inquiries can start processing.");

            Status = InquiryStatus.Processing;
        }

        public ProviderAttempt AddProviderAttempt(string providerName, DateTime startedAt)
        {
            if (Status != InquiryStatus.Processing)
                throw new InvalidOperationException(
                    "Provider attempt can only be added while inquiry is processing.");

            var attempt = ProviderAttempt.Create(
                Id,
                providerName,
                startedAt);

            _providerAttempts.Add(attempt);

            return attempt;
        }

        public void Complete(decimal amount, string result, DateTime completedAt)
        {
            if (Status != InquiryStatus.Processing)
                throw new InvalidOperationException("Only processing inquiries can be completed.");

            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            if (string.IsNullOrWhiteSpace(result))
                throw new ArgumentException("Result cannot be empty.", nameof(result));

            Status = InquiryStatus.Completed;
            Amount = amount;
            Result = result;
            CompletedAt = completedAt;
        }

        public void Fail(string result, DateTime completedAt)
        {
            if (Status != InquiryStatus.Processing)
                throw new InvalidOperationException("Only processing inquiries can be failed.");

            if (string.IsNullOrWhiteSpace(result))
                throw new ArgumentException("Result cannot be empty.", nameof(result));

            Status = InquiryStatus.Failed;
            Result = result;
            CompletedAt = completedAt;
        }
    }
}
