using Abp.Application.Services;
using Abp.Entities.Geo.Dto;
using System;

namespace Abp.Entities.Geo
{
    public interface IProvinceAppService : IAsyncCrudAppService<ProvinceDto, Guid>
    {
    }
}
