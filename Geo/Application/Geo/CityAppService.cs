using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Malekino.Authorization;
using Malekino.Geo.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abp.Entities.Geo
{

    public class CityAppService : AsyncCrudAppService<City, CityDto, Guid>, ICityAppService
    {

        public CityAppService(IRepository<City, Guid> repository) : base(repository)
        {
            CreatePermissionName = PermissionNames.Pages_Cities;
            UpdatePermissionName = PermissionNames.Pages_Cities;
            DeletePermissionName = PermissionNames.Pages_Cities;
        }

    }
}
