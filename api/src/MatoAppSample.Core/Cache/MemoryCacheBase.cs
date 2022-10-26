using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.Data;
using Abp.Runtime.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Abp.Threading.Extensions;

namespace MatoAppSample.Cache
{
    public class MemoryCacheBase<T> : AbpCacheBase<string, T>
    {
        private MemoryCache _memoryCache;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Unique name of the cache</param>
        public MemoryCacheBase(string name)
            : base(name)
        {
            _memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
        }

        public override bool TryGetValue(string key, out T value)
        {
            return _memoryCache.TryGetValue(key, out value);
        }

        public override void Set(string key, T value, TimeSpan? slidingExpireTime = null, DateTimeOffset? absoluteExpireTime = null)
        {
            if (value == null)
            {
                throw new AbpException("Can not insert null values to the cache!");
            }

            if (absoluteExpireTime.HasValue)
            {
                _memoryCache.Set(key, value, absoluteExpireTime.Value);
            }
            else if (slidingExpireTime.HasValue)
            {
                _memoryCache.Set(key, value, slidingExpireTime.Value);
            }
            else if (DefaultAbsoluteExpireTime.HasValue)
            {
                _memoryCache.Set(key, value, DefaultAbsoluteExpireTime.Value);
            }
            else
            {
                _memoryCache.Set(key, value, DefaultSlidingExpireTime);
            }
        }

        public override void Remove(string key)
        {
            _memoryCache.Remove(key);
        }

        public override void Clear()
        {
            _memoryCache.Dispose();
            _memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
        }

        public override void Dispose()
        {
            _memoryCache.Dispose();
            base.Dispose();
        }


        public virtual async Task<T> GetAsync(string key, Func<string, T> factory, TimeSpan? slidingExpireTime = null, DateTimeOffset? absoluteExpireTime = null)
        {
            ConditionalValue<T> result = default;

            try
            {
                result = await TryGetValueAsync(key);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString(), ex);
            }

            if (result.HasValue)
            {
                return result.Value;
            }

            using (await SemaphoreSlim.LockAsync())
            {
                try
                {
                    result = await TryGetValueAsync(key);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString(), ex);
                }

                if (result.HasValue)
                {
                    return result.Value;
                }
                if (factory == null)
                {
                    return default;
                }
                var generatedValue = factory.Invoke(key);
                if (IsDefaultValue(generatedValue))
                {
                    return generatedValue;
                }

                try
                {
                    await SetAsync(key, generatedValue, slidingExpireTime, absoluteExpireTime);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString(), ex);
                }

                return generatedValue;
            }
        }



    }
}
