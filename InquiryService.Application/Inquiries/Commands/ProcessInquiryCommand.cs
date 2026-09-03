using InquiryService.Application.Inquiries.Models;
using MediatR;

namespace InquiryService.Application.Inquiries.Commands
{
    public sealed record ProcessInquiryCommand(string BillId, bool IgnoreCache) : IRequest<ProcessInquiryResult>;
}
