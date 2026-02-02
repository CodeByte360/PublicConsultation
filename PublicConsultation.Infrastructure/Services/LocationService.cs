using Microsoft.EntityFrameworkCore;
using PublicConsultation.Core.Entities;
using PublicConsultation.Core.Interfaces;
using PublicConsultation.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PublicConsultation.Infrastructure.Services;

public class LocationService : ILocationService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public LocationService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // Divisions
    public async Task<IEnumerable<Division>> GetDivisionsAsync()
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        return await context.Divisions.AsNoTracking().OrderBy(d => d.Name).ToListAsync();
    }

    public async Task<Division?> GetDivisionByIdAsync(Guid id)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        return await context.Divisions.FindAsync(id);
    }

    public async Task<bool> SaveDivisionAsync(Division division)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        context.Divisions.Add(division);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateDivisionAsync(Division division)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        context.Entry(division).State = EntityState.Modified;
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteDivisionAsync(Guid id)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        var item = await context.Divisions.FindAsync(id);
        if (item == null) return false;
        context.Divisions.Remove(item);
        return await context.SaveChangesAsync() > 0;
    }

    // Districts
    public async Task<IEnumerable<District>> GetDistrictsAsync()
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        return await context.Districts.AsNoTracking().Include(d => d.Division).OrderBy(d => d.Name).ToListAsync();
    }

    public async Task<IEnumerable<District>> GetDistrictsByDivisionIdAsync(Guid divisionId)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        return await context.Districts.AsNoTracking().Where(d => d.DivisionId == divisionId).OrderBy(d => d.Name).ToListAsync();
    }

    public async Task<District?> GetDistrictByIdAsync(Guid id)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        return await context.Districts.AsNoTracking().Include(d => d.Division).FirstOrDefaultAsync(d => d.Oid == id);
    }

    public async Task<bool> SaveDistrictAsync(District district)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        context.Districts.Add(district);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateDistrictAsync(District district)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        context.Entry(district).State = EntityState.Modified;
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteDistrictAsync(Guid id)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        var item = await context.Districts.FindAsync(id);
        if (item == null) return false;
        context.Districts.Remove(item);
        return await context.SaveChangesAsync() > 0;
    }

    // Police Stations
    public async Task<IEnumerable<PoliceStation>> GetPoliceStationsAsync()
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        return await context.PoliceStations.AsNoTracking().Include(p => p.District).OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<IEnumerable<PoliceStation>> GetPoliceStationsByDistrictIdAsync(Guid districtId)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        return await context.PoliceStations.AsNoTracking().Where(p => p.DistrictId == districtId).OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<PoliceStation?> GetPoliceStationByIdAsync(Guid id)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        return await context.PoliceStations.AsNoTracking().Include(p => p.District).FirstOrDefaultAsync(p => p.Oid == id);
    }

    public async Task<bool> SavePoliceStationAsync(PoliceStation policeStation)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        context.PoliceStations.Add(policeStation);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdatePoliceStationAsync(PoliceStation policeStation)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        context.Entry(policeStation).State = EntityState.Modified;
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeletePoliceStationAsync(Guid id)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        var item = await context.PoliceStations.FindAsync(id);
        if (item == null) return false;
        context.PoliceStations.Remove(item);
        return await context.SaveChangesAsync() > 0;
    }
}
