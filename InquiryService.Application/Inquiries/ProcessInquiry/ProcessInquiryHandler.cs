using InquiryService.Application.Providers;
using InquiryService.Domain.Entities;
using InquiryService.Domain.Enums;
using MediatR;

namespace InquiryService.Application.Inquiries.ProcessInquiry
{
    public class ProcessInquiryHandler(IPaymentProviderExecutor providerExecutor) : IRequestHandler<ProcessInquiryCommand, ProcessInquiryResult>
    {
        private readonly IPaymentProviderExecutor _providerExecutor = providerExecutor;

        public async Task<ProcessInquiryResult> Handle(ProcessInquiryCommand request, CancellationToken cancellationToken)
        {
            var inquiry = Inquiry.Create(request.BillId, DateTime.UtcNow);

            inquiry.StartProcessing();

            var providerRequest = new PaymentInquiryRequest(request.BillId);

            var executionResult = await _providerExecutor.ExecuteAsync(providerRequest, cancellationToken);

            foreach (var attempt in executionResult.Attempts)
            {
                inquiry.AddProviderAttempt(
                    attempt.ProviderName,
                    MapAttemptStatus(attempt.Status),
                    attempt.StartedAt,
                    attempt.CompletedAt,
                    attempt.ErrorMessage
                );
            }

            var result = executionResult.FinalResult;

            if (result.Status == ProviderResultStatus.Success)
            {
                inquiry.Complete(
                    result.Amount!.Value,
                    "Inquiry completed successfully!",
                    DateTime.UtcNow);
            }
            else
            {
                inquiry.Fail(
                    "Inquiry failed!",
                    DateTime.UtcNow);
            }

            return new ProcessInquiryResult(
                inquiry.Id,
                inquiry.Status,
                inquiry.Amount,
                inquiry.Result);

        }

        private static ProviderAttemptStatus MapAttemptStatus(
        ProviderResultStatus status)
        {
            return status switch
            {
                ProviderResultStatus.Success =>
                    ProviderAttemptStatus.Success,

                ProviderResultStatus.BusinessError =>
                    ProviderAttemptStatus.BusinessError,

                ProviderResultStatus.TechnicalError =>
                    ProviderAttemptStatus.TechnicalError,

                ProviderResultStatus.Timeout =>
                    ProviderAttemptStatus.Timeout,

                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }
    }
}
