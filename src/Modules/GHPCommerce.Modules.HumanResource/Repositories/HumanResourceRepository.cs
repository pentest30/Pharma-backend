using System;
using GHPCommerce.CrossCuttingConcerns.OS;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Modules.HumanResource.Entities;

namespace GHPCommerce.Modules.HumanResource.Repositories
{
    public class HumanResourceRepository: Repository<Employee, Guid>, IHumanResourceRepository
    {
        public HumanResourceRepository(HumanResourceDbContext dbContext, IDateTimeProvider dateTimeProvider,ICurrentUser currentUser) : base(
            dbContext, dateTimeProvider,currentUser)
        {
        }

    }
}