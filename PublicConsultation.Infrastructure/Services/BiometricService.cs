#nullable enable
using Microsoft.EntityFrameworkCore;
using PublicConsultation.Core.Entities;
using PublicConsultation.Core.Interfaces;
using PublicConsultation.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace PublicConsultation.Infrastructure.Services;

public class BiometricService : IBiometricService
{
    private readonly ApplicationDbContext _context;

    public BiometricService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Biometric?> GetBiometricByUserIdAsync(Guid userId)
    {
        return await _context.Biometrics
            .FirstOrDefaultAsync(b => b.UserAccountId == userId);
    }

    public async Task<bool> SaveBiometricAsync(Biometric biometric)
    {
        try
        {
            _context.Biometrics.Add(biometric);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> UpdateBiometricAsync(Biometric biometric)
    {
        try
        {
            _context.Biometrics.Update(biometric);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
