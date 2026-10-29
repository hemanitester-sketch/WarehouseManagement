using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace WarehouseManagement.Faculty;

public interface IFacultyMemberAppService :
    ICrudAppService<
        FacultyMemberDto,
        Guid,
        GetFacultyMemberListDto,
        CreateUpdateFacultyMemberDto,
        CreateUpdateFacultyMemberDto>
{
    Task<ListResultDto<FacultyMemberLookupDto>> GetLookupAsync(Guid? warehouseId = null);

    Task<FacultyMemberDto?> FindByCodeAsync(string employeeCode);
}

