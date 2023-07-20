using System;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.HumanResource.Entities;

namespace GHPCommerce.Modules.HumanResource.Repositories
{
    public interface IHumanResourceRepository : IRepository<Employee, Guid>
    {
        
    }
}