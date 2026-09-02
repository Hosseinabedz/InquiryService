using InquiryService.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Application.Abstractions
{
    public class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
    {
        private readonly AppDbContext _context = dbContext;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
