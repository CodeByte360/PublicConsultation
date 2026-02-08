using Microsoft.Extensions.Caching.Memory;
using PublicConsultation.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace PublicConsultation.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly IMemoryCache _memoryCache;

    public OtpService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<string> GenerateOtpAsync(string key)
    {
        var otp = new Random().Next(100000, 999999).ToString();
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

        _memoryCache.Set(key, otp, cacheOptions);

        return Task.FromResult(otp);
    }

    public Task<bool> ValidateOtpAsync(string key, string otp)
    {
        if (_memoryCache.TryGetValue(key, out string? cachedOtp))
        {
            if (cachedOtp == otp)
            {
                _memoryCache.Remove(key); // OTP key is one-time use
                return Task.FromResult(true);
            }
        }
        return Task.FromResult(false);
    }
}
