using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace LeaveApp.ViewModels
{
    public class changePasswordViewModel
    {
        [Required(ErrorMessage = "Old password Required")]
        public string old_Password { get; set; }
        [Required(ErrorMessage = "New password Required")]
        public string new_Password { get; set; }
        [Required(ErrorMessage = "Confirmation password Required")]
        public string confirm_Password { get; set; }
    }
}