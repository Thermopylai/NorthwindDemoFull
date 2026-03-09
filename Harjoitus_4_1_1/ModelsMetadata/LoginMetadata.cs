using Harjoitus_4_1_1.ModelsMetadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.EnterpriseServices.Internal;
using System.Linq;
using System.Web;

namespace Harjoitus_4_1_1.ModelsMetadata
{
    public class LoginMetadata
    {
        [UIHint("Id_number")]
        [Display(Name = "ID")]
        [Key]
        public int LoginID { get; set; }

        [Required(ErrorMessage = "User name is required!")]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

namespace Harjoitus_4_1_1.Models
{
    [MetadataType(typeof(LoginMetadata))]
    public partial class Login
    {
        
    }
}