using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Application.Inquiries
{
    public class InquiryProcessingLock
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public SemaphoreSlim Get(string key)
        {
            return _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        }
    }
}
