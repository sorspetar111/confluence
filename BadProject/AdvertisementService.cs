using System;
using System.Threading;
using ThirdParty;
using Microsoft.Extensions.Configuration;

namespace Adv
{
    public interface ICacheProvider
    {
        Advertisement Get(string key);
        void Set(string key, Advertisement value, DateTimeOffset absoluteExpiration);
    }

    public interface IDataProvider
    {
        Advertisement GetAdvertisement(string id);
    }

    public class AdvertisementService : IDataProvider
    {
        private readonly ICacheProvider _cache;
        private readonly IConfiguration _configuration;
        private readonly NoSqlAdvProvider _mainProvider;
        public AdvertisementService(ICacheProvider cache, NoSqlAdvProvider mainProvider, IConfiguration configuration)
        {
            _cache = cache;
            _mainProvider = mainProvider;
            _configuration = configuration;
        }

        public Advertisement GetAdvertisement(string id)
        {
            Advertisement adv = GetAdvertisementFromCache(id);

            if (adv == null)
            {
                adv = GetAdvertisementFromMainProvider(id);
            }

            if (adv == null)
            {
                adv = GetAdvertisementFromBackupProvider(id);
            }

            return adv;
        }

        private Advertisement GetAdvertisementFromBackupProvider(string id)
        {
            Advertisement adv = SQLAdvProvider.GetAdv(id);
            if (adv != null)
            {
                _cache.Set($"AdvKey_{id}", adv, DateTimeOffset.Now.AddMinutes(5));
            }
            return adv;
        }

        private Advertisement GetAdvertisementFromCache(string id)
        {
            return _cache.Get($"AdvKey_{id}");
        }

        private Advertisement GetAdvertisementFromMainProvider(string id)
        {
            int retryCount = int.Parse(_configuration["RetryCount"]);

            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    Advertisement adv = _mainProvider.GetAdv(id);
                    if (adv != null)
                    {
                        _cache.Set($"AdvKey_{id}", adv, DateTimeOffset.Now.AddMinutes(5));
                        return adv;
                    }
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }

            return null;
        }
    }
}
