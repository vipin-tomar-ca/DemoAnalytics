using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollAnalytics.Api.Models;

namespace PayrollAnalytics.Api.Services.Interfaces
{
    public interface ILookupService
    {
        Task<IEnumerable<Department>> GetDepartmentsAsync();
        Task<IEnumerable<PayGrade>> GetPayGradesAsync();
        Task<IEnumerable<AbsenceType>> GetAbsenceTypesAsync();
        Task<IEnumerable<Location>> GetLocationsAsync();
        Task<Department> GetDepartmentByIdAsync(int id);
        Task<PayGrade> GetPayGradeByIdAsync(int id);
        Task<AbsenceType> GetAbsenceTypeByIdAsync(int id);
        Task<Location> GetLocationByIdAsync(int id);
    }
}
