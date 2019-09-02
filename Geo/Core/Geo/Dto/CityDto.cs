using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abp.Entities.Geo.Dto
{
    public class CityDto: EntityDto<Guid>
    {
        public string Name { get; set; }
        public Guid ProvinceId { get; set; }
        public ProvinceDto Province { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
    }
}
