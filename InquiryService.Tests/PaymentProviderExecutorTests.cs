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

            _mellatProvider
                .SetupGet(x => x.Name)
                .Returns("Mellat");

            _samanProvider
                .SetupGet(x => x.Name)
                .Returns("Saman");

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

            _sut = new PaymentProviderExecutor(
                providers,
                options,
                logger);
        }

        [Fact]
        public async Task ExecuteAsync_WhenFirstProviderSucceeds_ShouldReturnSuccess()
        {
            var request = new PaymentInquiryRequest("123456");

            _mellatProvider
                .Setup(x => x.InquiryAsync(
                    request,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new ProviderInquiryResult(
                        ProviderResultStatus.Success,
                        125_000,
                        null));

            var result = await _sut.ExecuteAsync(
                request,
                CancellationToken.None);

            result.FinalResult.Status.Should().Be(ProviderResultStatus.Success);
            result.FinalResult.Amount.Should().Be(125_000);

            _mellatProvider.Verify(
                x => x.InquiryAsync(
                    request,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _samanProvider.Verify(
                x => x.InquiryAsync(
                    It.IsAny<PaymentInquiryRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WhenFirstProviderReturnsBusinessError_ShouldNotFailover()
        {
            var request = new PaymentInquiryRequest("123456");

            _mellatProvider
                .Setup(x => x.InquiryAsync(
                    request,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new ProviderInquiryResult(
                        ProviderResultStatus.BusinessError,
                        null,
                        "Invalid bill id."));

            var result = await _sut.ExecuteAsync(
                request,
                CancellationToken.None);

            result.FinalResult.Status.Should().Be(ProviderResultStatus.BusinessError);
            result.FinalResult.ErrorMessage.Should().Be("Invalid bill id.");

            _mellatProvider.Verify(
                x => x.InquiryAsync(
                    request,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _samanProvider.Verify(
                x => x.InquiryAsync(
                    It.IsAny<PaymentInquiryRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
