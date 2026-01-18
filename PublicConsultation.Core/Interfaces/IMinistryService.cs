using PublicConsultation.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PublicConsultation.Core.Interfaces;

public interface IMinistryService
{
    Task<List<Ministry>> GetAllMinistriesAsync();
    Task<Ministry> GetMinistryByIdAsync(Guid id);
    Task CreateMinistryAsync(Ministry ministry);
    Task UpdateMinistryAsync(Ministry ministry);
    Task DeleteMinistryAsync(Guid id);
}
