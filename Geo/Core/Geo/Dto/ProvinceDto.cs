using Abp.Application.Services.Dto;
using System;

namespace Abp.Entities.Geo.Dto
{
    public class ProvinceDto: EntityDto<Guid>
    {
        public string Name { get; set; }
    }
}
