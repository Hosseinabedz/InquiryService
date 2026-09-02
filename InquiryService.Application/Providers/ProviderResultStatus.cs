using InquiryService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Application.Providers
{
    public enum ProviderResultStatus
    {
        Success,
        BusinessError,
        TechnicalError,
        Timeout
    }
}
