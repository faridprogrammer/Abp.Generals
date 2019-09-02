using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abp.Entities.Geo
{
    public class City : Entity<Guid>
    {
        public string Name { get; set; }
        public Guid ProvinceId { get; set; }
        public Province Province { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
    }
}
