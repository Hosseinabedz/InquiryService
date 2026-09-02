using InquiryService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Domain.Repositories
{
    public interface IInquiryRepository
    {
        Task AddAsync(Inquiry inquiry, CancellationToken cancellationToken);
    }
}
