using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Application.Inquiries.Models
{
    public sealed record ProcessInquiryRequest(string BillId, bool IgnoreCache);
}
