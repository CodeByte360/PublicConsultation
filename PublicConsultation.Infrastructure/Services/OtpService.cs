using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using PublicConsultation.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace PublicConsultation.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _configuration;

    public OtpService(IMemoryCache memoryCache, IConfiguration configuration)
    {
        _memoryCache = memoryCache;
        _configuration = configuration;
    }

    public Task<string> GenerateOtpAsync(string key)
    {
        var otp = new Random().Next(100000, 999999).ToString();
        var expirationMinutes = _configuration.GetValue<int>("OtpSettings:ExpirationMinutes", 5);

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(expirationMinutes));

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
