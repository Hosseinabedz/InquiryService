using InquiryService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Application.Providers.Models
{
    public enum ProviderResultStatus
    {
        Success,
        BusinessError,
        TechnicalError,
        Timeout
    }
}
