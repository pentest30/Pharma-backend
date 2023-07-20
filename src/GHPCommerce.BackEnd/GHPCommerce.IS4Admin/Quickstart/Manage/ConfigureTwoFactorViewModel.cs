using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GHPCommerce.IS4Admin.Quickstart.Manage
{
    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }

        public ICollection<SelectListItem> Providers { get; set; }
    }
}
