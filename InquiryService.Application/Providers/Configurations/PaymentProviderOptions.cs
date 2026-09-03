using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Application.Providers.Configurations
{
    public class PaymentProviderOptions
    {
        public List<PaymentProviderSetting> Providers { get; set; } = [];
    }

    public class PaymentProviderSetting
    {
        public string Name { get; set; } = string.Empty;
        public int Priority { get; set; }
        public int TimeoutSeconds { get; set; }
    }
}
