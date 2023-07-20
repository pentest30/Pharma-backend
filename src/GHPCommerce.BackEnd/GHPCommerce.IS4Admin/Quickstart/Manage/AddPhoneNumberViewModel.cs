using System.ComponentModel.DataAnnotations;

namespace GHPCommerce.IS4Admin.Quickstart.Manage
{
    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }
    }
}
