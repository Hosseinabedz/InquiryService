using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Domain.Enums
{
    public enum ProviderAttemptStatus
    {
        Processing = 0,
        Success = 1,
        BusinessError = 2,
        Timeout = 3,
        TechnicalError = 4,
    }
}
