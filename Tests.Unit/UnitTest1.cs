using System;
using System.Threading;
using System.Timers;
using Microsoft.Extensions.Configuration;
using Moq;
using ThirdParty;
using Xunit;

namespace Adv.Tests
{
    public class AdvertisementServiceTests
    {
        [Fact]
        public void GetAdvertisement_ReturnsAdvertisementFromCache_WhenCacheIsNotEmpty()
        {
            // Arrange
            var cacheMock = new Mock<ICacheProvider>();
            var mainProviderMock = new Mock<NoSqlAdvProvider>();
            var configurationMock = new Mock<IConfiguration>();

            var advertisementFromCache = new Advertisement { Id = "1", Content = "Advertisement from cache" };
            cacheMock.Setup(c => c.Get("AdvKey_1")).Returns(advertisementFromCache);

            var service = new AdvertisementService(cacheMock.Object, mainProviderMock.Object, configurationMock.Object);

            // Act
            var result = service.GetAdvertisement("1");

            // Assert
            Assert.Equal(advertisementFromCache, result);
        }

        [Fact]
        public void GetAdvertisement_ReturnsAdvertisementFromMainProvider_WhenCacheIsEmpty()
        {
            // Arrange
            var cacheMock = new Mock<ICacheProvider>();
            var mainProviderMock = new Mock<NoSqlAdvProvider>();
            var configurationMock = new Mock<IConfiguration>();

            cacheMock.Setup(c => c.Get("AdvKey_1")).Returns((Advertisement)null);

            var advertisementFromMainProvider = new Advertisement { Id = "1", Content = "Advertisement from main provider" };
            mainProviderMock.Setup(p => p.GetAdv("1")).Returns(advertisementFromMainProvider);

            var service = new AdvertisementService(cacheMock.Object, mainProviderMock.Object, configurationMock.Object);

            // Act
            var result = service.GetAdvertisement("1");

            // Assert
            Assert.Equal(advertisementFromMainProvider, result);
            cacheMock.Verify(c => c.Set("AdvKey_1", advertisementFromMainProvider, It.IsAny<DateTimeOffset>()), Times.Once);
        }

        // Similar tests for other scenarios (e.g., cache is empty and main provider returns null, backup provider is used, error handling, etc.) can be added.
    }
}
