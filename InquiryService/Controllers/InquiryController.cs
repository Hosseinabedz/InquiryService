using InquiryService.Application.Inquiries.Commands;
using InquiryService.Application.Inquiries.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InquiryService.API.Controllers
{
    [ApiController]
    [Route("api/inquiries")]
    public class InquiryController(
        IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Inquiries([FromBody] ProcessInquiryRequest request, CancellationToken cancellationToken)
        {
            var command = new ProcessInquiryCommand(request.BillId, request.IgnoreCache);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
    }
}
