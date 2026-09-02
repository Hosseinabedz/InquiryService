using InquiryService.Application.Abstractions;
using InquiryService.Application.Inquiries.Models;
using InquiryService.Application.Providers;
using InquiryService.Domain.Entities;
using InquiryService.Domain.Enums;
using InquiryService.Domain.Repositories;
using MediatR;

namespace InquiryService.Application.Inquiries.Commands
{
    public class ProcessInquiryCommandHandler(
        IPaymentProviderExecutor providerExecutor,
        IUnitOfWork unitOfWork,
        IInquiryRepository inquiryRepository) : IRequestHandler<ProcessInquiryCommand, ProcessInquiryResult>
    {
        private readonly IPaymentProviderExecutor _providerExecutor = providerExecutor;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IInquiryRepository _inquiryRepository = inquiryRepository;

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

            await _inquiryRepository.AddAsync(inquiry, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
