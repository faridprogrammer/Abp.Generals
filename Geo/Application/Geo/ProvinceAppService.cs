using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Entities.Authorization;
using Abp.Entities.Geo.Dto;
using System;

namespace Abp.Entities.Geo
{
    public class ProvinceAppService : AsyncCrudAppService<Province, ProvinceDto, Guid>, IProvinceAppService
    {
        public ProvinceAppService(IRepository<Province, Guid> repository) : base(repository)
        {
            CreatePermissionName = PermissionNames.Pages_Provinces;
            UpdatePermissionName = PermissionNames.Pages_Provinces;
            DeletePermissionName = PermissionNames.Pages_Provinces;
        }
    }
}
