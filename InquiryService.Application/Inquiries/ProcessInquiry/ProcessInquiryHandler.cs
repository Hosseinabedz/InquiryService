using InquiryService.Application.Providers;

namespace InquiryService.Application.Inquiries.ProcessInquiry
{
    public class ProcessInquiryHandler(
        IEnumerable<IPaymentProvider> providers,
        PaymentProviderOptions options)
    {
        private readonly IEnumerable<IPaymentProvider> _providers = providers;
        private readonly PaymentProviderOptions _options = options;

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
