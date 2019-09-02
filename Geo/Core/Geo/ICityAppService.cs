using Abp.Application.Services;
using Malekino.Geo.Dto;
using System;

namespace Abp.Entities.Geo
{
    public interface ICityAppService : IAsyncCrudAppService<CityDto, Guid>
    {
    }
}
