using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GHPCommerce.IS4Admin.Quickstart.Manage
{
    public class DisplayRecoveryCodesViewModel
    {
        [Required]
        public IEnumerable<string> Codes { get; set; }

    }
}
