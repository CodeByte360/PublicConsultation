#nullable enable
using PublicConsultation.Core.Entities;
using System;
using System.Threading.Tasks;

namespace PublicConsultation.Core.Interfaces;

public interface IBiometricService
{
    Task<Biometric?> GetBiometricByUserIdAsync(Guid userId);
    Task<bool> SaveBiometricAsync(Biometric biometric);
    Task<bool> UpdateBiometricAsync(Biometric biometric);
}
