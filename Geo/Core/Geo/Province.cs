using Abp.Domain.Entities;
using System;

namespace Abp.Entities.Geo
{
    public class Province : Entity<Guid>
    {
        public string Name { get; set; }

    }
}
