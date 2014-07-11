using System.ComponentModel.DataAnnotations;

namespace KwasantWeb.ViewModels
{
    public class MyAccountModel
    {
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
