using FluentAssertions;
using InquiryService.Application.Providers.Abstractions;
using InquiryService.Application.Providers.Configurations;
using InquiryService.Application.Providers.Models;
using InquiryService.Application.Providers.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace InquiryService.Tests
{
    public sealed class PaymentProviderExecutorTests
    {
        private readonly Mock<IPaymentProvider> _mellatProvider;
        private readonly Mock<IPaymentProvider> _samanProvider;
        private readonly PaymentProviderExecutor _sut;

        public PaymentProviderExecutorTests()
        {
            _mellatProvider = new Mock<IPaymentProvider>();
            _samanProvider = new Mock<IPaymentProvider>();

            _mellatProvider.SetupGet(x => x.Name).Returns("Mellat");
            _samanProvider.SetupGet(x => x.Name).Returns("Saman");

            var providers = new[]
            {
            _mellatProvider.Object,
            _samanProvider.Object
        };

            var options = Options.Create(
                new PaymentProviderOptions
                {
                    Providers =
                    [
                        new PaymentProviderSetting
                    {
                        Name = "Mellat",
                        Priority = 1,
                        TimeoutSeconds = 1
                    },
                    new PaymentProviderSetting
                    {
                        Name = "Saman",
                        Priority = 2,
                        TimeoutSeconds = 1
                    }
                    ]
                });

            var logger = Mock.Of<ILogger<PaymentProviderExecutor>>();

            _sut = new PaymentProviderExecutor(providers, options, logger);
        }

        [Fact]
        public async Task ExecuteAsync_WhenFirstProviderSucceeds_ShouldReturnSuccessAndNotFailover()
        {
            var request = new PaymentInquiryRequest("123456");

            _mellatProvider
                .Setup(x => x.InquiryAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProviderInquiryResult(
                    ProviderResultStatus.Success,
                    125_000,
                    null));

            var result = await _sut.ExecuteAsync(request, CancellationToken.None);

            result.FinalResult.Status.Should().Be(ProviderResultStatus.Success);
            result.FinalResult.Amount.Should().Be(125_000);
            result.Attempts.Should().ContainSingle();
            result.Attempts.Single().ProviderName.Should().Be("Mellat");

            _mellatProvider.Verify(
                x => x.InquiryAsync(request, It.IsAny<CancellationToken>()),
                Times.Once);

            _samanProvider.Verify(
                x => x.InquiryAsync(It.IsAny<PaymentInquiryRequest>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WhenFirstProviderReturnsBusinessError_ShouldNotFailover()
        {
            var request = new PaymentInquiryRequest("123456");

            _mellatProvider
                .Setup(x => x.InquiryAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProviderInquiryResult(
                    ProviderResultStatus.BusinessError,
                    null,
                    "Invalid bill id."));

            var result = await _sut.ExecuteAsync(request, CancellationToken.None);

            result.FinalResult.Status.Should().Be(ProviderResultStatus.BusinessError);
            result.FinalResult.ErrorMessage.Should().Be("Invalid bill id.");
            result.Attempts.Should().ContainSingle();

            _samanProvider.Verify(
                x => x.InquiryAsync(It.IsAny<PaymentInquiryRequest>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WhenFirstProviderReturnsTechnicalError_ShouldFailoverToNextProvider()
        {
            var request = new PaymentInquiryRequest("123456");

            _mellatProvider
                .Setup(x => x.InquiryAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProviderInquiryResult(
                    ProviderResultStatus.TechnicalError,
                    null,
                    "Mellat is unavailable."));

            _samanProvider
                .Setup(x => x.InquiryAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProviderInquiryResult(
                    ProviderResultStatus.Success,
                    125_000,
                    null));

            var result = await _sut.ExecuteAsync(request, CancellationToken.None);

            result.FinalResult.Status.Should().Be(ProviderResultStatus.Success);
            result.FinalResult.Amount.Should().Be(125_000);
            result.Attempts.Should().HaveCount(2);
            result.Attempts.Select(x => x.Status)
                .Should()
                .Equal(ProviderResultStatus.TechnicalError, ProviderResultStatus.Success);

            _mellatProvider.Verify(
                x => x.InquiryAsync(request, It.IsAny<CancellationToken>()),
                Times.Once);

            _samanProvider.Verify(
                x => x.InquiryAsync(request, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenFirstProviderThrowsException_ShouldFailoverToNextProvider()
        {
            var request = new PaymentInquiryRequest("123456");

            _mellatProvider
                .Setup(x => x.InquiryAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Mellat connection failed."));

            _samanProvider
                .Setup(x => x.InquiryAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProviderInquiryResult(
                    ProviderResultStatus.Success,
                    125_000,
                    null));

            var result = await _sut.ExecuteAsync(request, CancellationToken.None);

            result.FinalResult.Status.Should().Be(ProviderResultStatus.Success);
            result.Attempts.Should().HaveCount(2);
            result.Attempts.First().Status.Should().Be(ProviderResultStatus.TechnicalError);
            result.Attempts.First().ErrorMessage.Should().Be("Mellat connection failed.");
            result.Attempts.Last().Status.Should().Be(ProviderResultStatus.Success);

            _samanProvider.Verify(
                x => x.InquiryAsync(request, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenFirstProviderTimesOut_ShouldFailoverToNextProvider()
        {
            var request = new PaymentInquiryRequest("123456");

            _mellatProvider
                 .Setup(x => x.InquiryAsync(
                     request,
                     It.IsAny<CancellationToken>()))
                 .Returns((PaymentInquiryRequest _, CancellationToken cancellationToken) =>
                 {
                     var tcs = new TaskCompletionSource<ProviderInquiryResult>();

                     cancellationToken.Register(() =>
                         tcs.TrySetCanceled(cancellationToken));

                     return tcs.Task;
                 });

            _samanProvider
                .Setup(x => x.InquiryAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProviderInquiryResult(
                    ProviderResultStatus.Success,
                    125_000,
                    null));

            var result = await _sut.ExecuteAsync(request, CancellationToken.None);

            result.FinalResult.Status.Should().Be(ProviderResultStatus.Success);
            result.Attempts.Should().HaveCount(2);
            result.Attempts.First().Status.Should().Be(ProviderResultStatus.Timeout);
            result.Attempts.Last().Status.Should().Be(ProviderResultStatus.Success);

            _mellatProvider.Verify(
                x => x.InquiryAsync(request, It.IsAny<CancellationToken>()),
                Times.Once);

            _samanProvider.Verify(
                x => x.InquiryAsync(request, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenAllProvidersFail_ShouldReturnTechnicalError()
        {
            var request = new PaymentInquiryRequest("123456");

            _mellatProvider
                .Setup(x => x.InquiryAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProviderInquiryResult(
                    ProviderResultStatus.TechnicalError,
                    null,
                    "Mellat is unavailable."));

            _samanProvider
                .Setup(x => x.InquiryAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProviderInquiryResult(
                    ProviderResultStatus.TechnicalError,
                    null,
                    "Saman is unavailable."));

            var result = await _sut.ExecuteAsync(request, CancellationToken.None);

            result.FinalResult.Status.Should().Be(ProviderResultStatus.TechnicalError);
            result.FinalResult.ErrorMessage.Should().Be("All payment providers are unavailable.");
            result.Attempts.Should().HaveCount(2);

            _mellatProvider.Verify(
                x => x.InquiryAsync(request, It.IsAny<CancellationToken>()),
                Times.Once);

            _samanProvider.Verify(
                x => x.InquiryAsync(request, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenProvidersAreRegisteredInDifferentOrder_ShouldUseConfiguredPriority()
        {
            var request = new PaymentInquiryRequest("123456");

            var options = Options.Create(
                new PaymentProviderOptions
                {
                    Providers =
                    [
                        new PaymentProviderSetting
                    {
                        Name = "Saman",
                        Priority = 1,
                        TimeoutSeconds = 1
                    },
                    new PaymentProviderSetting
                    {
                        Name = "Mellat",
                        Priority = 2,
                        TimeoutSeconds = 1
                    }
                    ]
                });

            var executor = new PaymentProviderExecutor(
                new[] { _mellatProvider.Object, _samanProvider.Object },
                options,
                Mock.Of<ILogger<PaymentProviderExecutor>>());

            _samanProvider
                .Setup(x => x.InquiryAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProviderInquiryResult(
                    ProviderResultStatus.Success,
                    100_000,
                    null));

            await executor.ExecuteAsync(request, CancellationToken.None);

            _samanProvider.Verify(
                x => x.InquiryAsync(request, It.IsAny<CancellationToken>()),
                Times.Once);

            _mellatProvider.Verify(
                x => x.InquiryAsync(request, It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
