using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LeaveApp.ViewModels
{
    public class AppFormViewModel
    {
        public string Emp_Name { get; set; }
        public int Leave_Days { get; set; }

        [Required(ErrorMessage ="Please chose Type of leave")]
        public string Type_Leave { get; set; }
        public System.DateTime First_Day { get; set; }
        public System.DateTime Last_Day { get; set; }
        public string Reason_Leave { get; set; }
        public string StandIn { get; set; }
        public int Emp_ID { get; set; }
        public string Emp_Division { get; set; }
        public string App_Status { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public int Annual_Leave { get; set; }
        public int Sick_Leave { get; set; }
        public int Fam_Leave { get; set; }
        [DataType(DataType.Upload)]
        public HttpPostedFileBase Emp_SickNote { get; set; }
        public List<string> Names { get; set; }
    }
}