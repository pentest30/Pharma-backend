using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Organizations.Commands
{
    public class ActivateOrganizationCommand :ICommand
    {
        public Guid Id { get; set; }
    }
}
