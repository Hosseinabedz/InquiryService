using MediatR;

namespace InquiryService.Application.Inquiries.ProcessInquiry
{
    public sealed record ProcessInquiryCommand(string BillId) : IRequest<ProcessInquiryResult>;
}
