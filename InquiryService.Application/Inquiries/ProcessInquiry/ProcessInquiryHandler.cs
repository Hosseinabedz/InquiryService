namespace InquiryService.Application.Inquiries.ProcessInquiry
{
    public class ProcessInquiryHandler
    {
        public async Task<ProcessInquiryResult> Handle(ProcessInquiryCommand command, CancellationToken cancellationToken)
        {
            // Create inquiry
            // Select Provider
            // Call Provider
            // Handle failover
            // Complete/Fail inquiry
            // Return result
        }
    }
}
