using FluentAssertions;
using InquiryService.Application.Abstractions;
using InquiryService.Application.Inquiries;
using InquiryService.Application.Inquiries.Commands;
using InquiryService.Application.Inquiries.Models;
using InquiryService.Application.Providers.Abstractions;
using InquiryService.Application.Providers.Models;
using InquiryService.Domain.Entities;
using InquiryService.Domain.Enums;
using InquiryService.Domain.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Tests
{
    public sealed class ProcessInquiryCommandHandlerTests : IDisposable
    {
        private readonly Mock<IPaymentProviderExecutor> _providerExecutor = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IInquiryRepository> _inquiryRepository = new();
        private readonly MemoryCache _cache = new(new MemoryCacheOptions());
        private readonly InquiryProcessingLock _processingLock = new();
        private readonly ProcessInquiryCommandHandler _sut;

        public ProcessInquiryCommandHandlerTests()
        {
            _inquiryRepository
                .Setup(x => x.AddAsync(It.IsAny<Inquiry>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _providerExecutor
                .Setup(x => x.ExecuteAsync(It.IsAny<PaymentInquiryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateSuccessExecutionResult());

            _sut = new ProcessInquiryCommandHandler(
                _providerExecutor.Object,
                _cache,
                _processingLock,
                _unitOfWork.Object,
                _inquiryRepository.Object);
        }

        [Fact]
        public async Task Handle_WhenResultIsCached_ShouldReturnCachedResultWithoutCallingProvider()
        {
            var billId = "123456";
            var cachedResult = new ProcessInquiryResult(
                Guid.NewGuid(),
                InquiryStatus.Completed,
                125_000,
                "Cached result");

            _cache.Set($"inquiry:{billId}", cachedResult, TimeSpan.FromMinutes(5));

            var result = await _sut.Handle(
                new ProcessInquiryCommand(billId, IgnoreCache: false),
                CancellationToken.None);

            result.Should().Be(cachedResult);

            _providerExecutor.Verify(
                x => x.ExecuteAsync(It.IsAny<PaymentInquiryRequest>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _inquiryRepository.Verify(
                x => x.AddAsync(It.IsAny<Inquiry>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenIgnoreCacheIsTrue_ShouldBypassCacheAndProcessAgain()
        {
            var billId = "123456";
            var cachedResult = new ProcessInquiryResult(
                Guid.NewGuid(),
                InquiryStatus.Completed,
                100_000,
                "Old result");

            _cache.Set($"inquiry:{billId}", cachedResult, TimeSpan.FromMinutes(5));

            var result = await _sut.Handle(
                new ProcessInquiryCommand(billId, IgnoreCache: true),
                CancellationToken.None);

            result.Status.Should().Be(InquiryStatus.Completed);
            result.Amount.Should().Be(125_000);
            result.Should().NotBe(cachedResult);

            _providerExecutor.Verify(
                x => x.ExecuteAsync(
                    It.Is<PaymentInquiryRequest>(r => r.BillId == billId),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _inquiryRepository.Verify(
                x => x.AddAsync(It.IsAny<Inquiry>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenIdenticalRequestsArriveConcurrently_ShouldProcessProviderOnlyOnce()
        {
            var billId = "123456";
            var providerStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseProvider = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            _providerExecutor
                .Setup(x => x.ExecuteAsync(
                    It.Is<PaymentInquiryRequest>(r => r.BillId == billId),
                    It.IsAny<CancellationToken>()))
                .Returns((PaymentInquiryRequest _, CancellationToken _) =>
                    ProcessProviderAsync());

            async Task<PaymentProviderExecutionResult> ProcessProviderAsync()
            {
                providerStarted.SetResult(true);
                await releaseProvider.Task;
                return CreateSuccessExecutionResult();
            }

            var command = new ProcessInquiryCommand(billId, IgnoreCache: false);

            var firstTask = _sut.Handle(command, CancellationToken.None);

            await providerStarted.Task;

            var secondTask = _sut.Handle(command, CancellationToken.None);

            await Task.Delay(100);

            _providerExecutor.Verify(
                x => x.ExecuteAsync(
                    It.Is<PaymentInquiryRequest>(r => r.BillId == billId),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            secondTask.IsCompleted.Should().BeFalse();

            releaseProvider.SetResult(true);

            var results = await Task.WhenAll(firstTask, secondTask);

            results[0].Status.Should().Be(InquiryStatus.Completed);
            results[1].Should().Be(results[0]);

            _inquiryRepository.Verify(
                x => x.AddAsync(It.IsAny<Inquiry>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private static PaymentProviderExecutionResult CreateSuccessExecutionResult()
        {
            var now = DateTime.UtcNow;

            return new PaymentProviderExecutionResult(
                new ProviderInquiryResult(
                    ProviderResultStatus.Success,
                    125_000,
                    null),
                new[]
                {
                new ProviderAttemptResult(
                    "Mellat",
                    ProviderResultStatus.Success,
                    null,
                    now,
                    now)
                });
        }

        public void Dispose()
        {
            _cache.Dispose();
        }
    }
}
