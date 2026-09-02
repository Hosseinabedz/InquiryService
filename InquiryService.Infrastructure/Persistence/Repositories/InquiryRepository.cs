using InquiryService.Domain.Entities;
using InquiryService.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Infrastructure.Persistence.Repositories
{
    public class InquiryRepository(AppDbContext dbContext) : IInquiryRepository
    {
        private readonly AppDbContext _context = dbContext;
        public async Task AddAsync(Inquiry inquiry, CancellationToken cancellationToken)
        {
            await _context.Inquiries.AddAsync(inquiry, cancellationToken);
        }
    }
}
