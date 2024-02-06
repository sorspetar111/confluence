using System.Runtime.Caching;

namespace Units.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }

        [Test]
        public void GetAdvertisement_ReturnsAdvertisement_WhenCacheIsNotEmpty()
        {
            // Arrange
            var cacheMock = new Mock<MemoryCache>("", new MemoryCacheOptions());
            var mainProviderMock = new Mock<IAdvProvider>();
            var backupProviderMock = new Mock<IAdvProvider>();
            var configurationMock = new Mock<IConfiguration>();

            var advertisement = new Advertisement { Id = "1", Content = "Test Advertisement" };
            cacheMock.Setup(c => c.Get("AdvKey_1")).Returns(advertisement);

            var service = new AdvertisementService(mainProviderMock.Object, backupProviderMock.Object, configurationMock.Object);

            // Act
            var result = service.GetAdvertisement("1");

            // Assert
            Assert.Equal(advertisement, result);
        }


    }
}