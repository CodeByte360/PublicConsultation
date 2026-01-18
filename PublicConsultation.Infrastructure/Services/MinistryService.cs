using Microsoft.EntityFrameworkCore;
using PublicConsultation.Core.Entities;
using PublicConsultation.Core.Interfaces;
using PublicConsultation.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PublicConsultation.Infrastructure.Services;

public class MinistryService : IMinistryService
{
    private readonly ApplicationDbContext _context;

    public MinistryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Ministry>> GetAllMinistriesAsync()
    {
        return await _context.Ministries.ToListAsync();
    }

    public async Task<Ministry> GetMinistryByIdAsync(Guid id)
    {
        return await _context.Ministries.FindAsync(id);
    }

    public async Task CreateMinistryAsync(Ministry ministry)
    {
        _context.Ministries.Add(ministry);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateMinistryAsync(Ministry ministry)
    {
        ministry.ModifiedDate = DateTime.UtcNow;
        _context.Entry(ministry).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteMinistryAsync(Guid id)
    {
        var ministry = await _context.Ministries.FindAsync(id);
        if (ministry != null)
        {
            _context.Ministries.Remove(ministry);
            await _context.SaveChangesAsync();
        }
    }
}
