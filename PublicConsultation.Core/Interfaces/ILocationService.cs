using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PublicConsultation.Core.Entities;

namespace PublicConsultation.Core.Interfaces;

public interface ILocationService
{
    // Divisions
    Task<IEnumerable<Division>> GetDivisionsAsync();
    Task<Division?> GetDivisionByIdAsync(Guid id);
    Task<bool> SaveDivisionAsync(Division division);
    Task<bool> UpdateDivisionAsync(Division division);
    Task<bool> DeleteDivisionAsync(Guid id);

    // Districts
    Task<IEnumerable<District>> GetDistrictsAsync();
    Task<IEnumerable<District>> GetDistrictsByDivisionIdAsync(Guid divisionId);
    Task<District?> GetDistrictByIdAsync(Guid id);
    Task<bool> SaveDistrictAsync(District district);
    Task<bool> UpdateDistrictAsync(District district);
    Task<bool> DeleteDistrictAsync(Guid id);

    // Police Stations
    Task<IEnumerable<PoliceStation>> GetPoliceStationsAsync();
    Task<IEnumerable<PoliceStation>> GetPoliceStationsByDistrictIdAsync(Guid districtId);
    Task<PoliceStation?> GetPoliceStationByIdAsync(Guid id);
    Task<bool> SavePoliceStationAsync(PoliceStation policeStation);
    Task<bool> UpdatePoliceStationAsync(PoliceStation policeStation);
    Task<bool> DeletePoliceStationAsync(Guid id);
}
