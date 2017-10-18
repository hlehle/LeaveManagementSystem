using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;
using System.Web.Mvc;
using LeaveApp.Models;
using LeaveApp.ViewModels;
using System.Net.Mail;
using System.Data.SqlClient;

namespace LeaveApp.Controllers
{
    public class AppController : Controller
    {
        // GET: App
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult loginPage()
        {

            return View();
        }
        [HttpPost]
        public ActionResult loginPage(Employee objEmp)
        {
            if (ModelState.IsValid)
            {
                using (Intern_LeaveDBEntities db = new Intern_LeaveDBEntities())
                {
                    var emp = db.Employees.Where(a => a.Username.Equals(objEmp.Username)
                    && a.Emp_Password.Equals(objEmp.Emp_Password)).FirstOrDefault();


                    if (emp != null)
                    {
                        Session["userid"] = emp.Emp_ID.ToString();
                        Session["username"] = emp.Emp_Name.ToString();

                        if (emp.Emp_Division == "Manager")
                        {
                            return RedirectToAction("ManagerPage");
                        }
                        var leave = db.Leave_Days.Where(t => t.Emp_ID.Equals(emp.Emp_ID)).FirstOrDefault();

                        var viewModel = new ViewModels.AppFormViewModel();
                        viewModel.Emp_ID = emp.Emp_ID;

                        return RedirectToAction("appForm", new { empId = emp.Emp_ID });
                    }
                    else
                    {
                        Response.Write("<script LANGUAGE='JavaScript' >alert('Incorrect Login Credentials')</script>");
                    }

                }
            }
            return View(objEmp);
        }
        public ActionResult LogOut() {

            Session.Clear();
            Session.RemoveAll();
            return RedirectToAction("loginPage");
        }
        public ActionResult ManagerPage()
        {
            Intern_LeaveDBEntities db = new Intern_LeaveDBEntities();
            return View(from leave in db.Applications select leave);
        }
        [HttpPost]
        public ActionResult ManagerPage(FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var managerForm = new List<Application>();


                string[] app_Ids = form["ID"].Split(new char[] { ',' });
                string[] values = form["App_Status"].Split(new char[] { ',' });
                string[] emp_Ids = form["Emp_ID"].Split(new char[] { ',' });
                //string[] emp_Names = form["Emp_Name"].Split(new char[] { ',' });
                string[] leave_Days = form["leave_Days"].Split(new char[] { ',' });
                string[] leave_Types = form["leave_Type"].Split(new char[] { ',' });

                using (Intern_LeaveDBEntities db = new Intern_LeaveDBEntities())
                {
                    Application app = new Application();

                    for (int i = 0; i < app_Ids.Length; i++)
                    {
                        int id = int.Parse(app_Ids[i]);
                        int emp_Id = int.Parse(emp_Ids[i]);
                        //Session["peruser"] = emp_Names[i];

                        app = db.Applications.Single(u => u.App_ID == id);
                        var emp = db.Employees.Single(e => e.Emp_ID == emp_Id);

                        app.App_Status = values[i];

                        db.SaveChanges();

                        if (!app.App_Status.Equals("Pending"))
                        {
                            //Set email user name - Change this as per your settings
                            const string username = "internleavesystem@gmail.com";
                            //Set the email password - Change this as per your settings
                            const string password = "Leave@2018";
                            SmtpClient smtpclient = new SmtpClient();
                            System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                            //Set the email from address of mail message -  Change this as per your settings
                            MailAddress fromaddress = new MailAddress("internleavesystem@gmail.com");
                            //Set the email smtp host -  Change this as per your settings
                            smtpclient.Host = "53.151.100.102";
                            //Set the email client port -  Change this as per your settings
                            smtpclient.Port = 25;
                            mail.From = fromaddress;
                            //Adding email id of receiver link
                            mail.To.Add(emp.Emp_emailAddr);
                            //Set the email subject
                            mail.Subject = ("Leave Notification");
                            mail.IsBodyHtml = false;
                            //Set the email body - Change this as per your logic
                            mail.Body = "Your Leave has been" + " " + app.App_Status + "." + " " + "From " + " " + app.First_Day + " " + "to" + " " + app.Last_Day;
                            smtpclient.DeliveryMethod = SmtpDeliveryMethod.Network;
                            smtpclient.Credentials = new System.Net.NetworkCredential(username, password);
                            {
                                Response.Write("<script LANGUAGE='JavaScript' >alert('Notification Has been sent')</script>");
                            }


                            try
                            {
                                //Sending Email
                                smtpclient.Send(mail);

                            }
                            catch (Exception ex)
                            {
                                //Catch if any exception occurs
                                Response.Write(ex.Message);
                                Response.Write("<script LANGUAGE='JavaScript' >alert('Notification Has been sent')</script>");
                            }
                        }


                        int leave_Day = int.Parse(leave_Days[i]);
                        string leave_Type = leave_Types[i];

                        if (app.App_Status.Equals("Approved"))
                        {

                            var leaveDb = db.Leave_Days.Single(a => a.Emp_ID == app.Emp_ID);

                            if(leave_Type.Contains("Annual"))
                            {
                                leaveDb.Annual_Leave = leaveDb.Annual_Leave - leave_Day; 
                            }

                            else if (leave_Type.Contains("Sick"))
                            {
                                leaveDb.Sick_Leave = leaveDb.Sick_Leave - leave_Day;
                            }

                            else if (leave_Type.Contains("fam"))
                            {
                                leaveDb.Fam_Leave = leaveDb.Fam_Leave - leave_Day;
                            }

                            db.SaveChanges();
                            
                        }

                    }
                }
            }


            return RedirectToAction("managerPage");
        }
        public ActionResult appForm(int? empId)
        {

            if (empId.HasValue && Session["userid"] != null)
            {

                using (Intern_LeaveDBEntities db = new Intern_LeaveDBEntities())
                {
                    var emp = db.Employees.Where(a => a.Emp_ID.Equals(empId.Value)).FirstOrDefault();
                    if (emp != null)
                    {

                        var leave = db.Leave_Days.Where(t => t.Emp_ID.Equals(emp.Emp_ID)).FirstOrDefault();

                        var empList = db.Employees.Where(b => b.Emp_ID != empId && b.Emp_Division != "Manager").Select(b => b.Emp_Name + " " + b.Emp_Surname).ToList();
                        var viewModel = new ViewModels.AppFormViewModel();

                        viewModel.Names = empList;

                        Session["Annual_Leave"] = leave.Annual_Leave;
                        Session["Sick_Leave"] = leave.Sick_Leave;
                        Session["Fam_Leave"] = leave.Fam_Leave;



                        return View(viewModel);
                    }
                }
                return RedirectToAction("loginPage");
            }
            else
            {
                return RedirectToAction("loginPage");
            }

        }
        [HttpPost]
        public ActionResult appForm(FormCollection form)
        {
            var appForm = new AppFormViewModel();
            TryUpdateModel<AppFormViewModel>(appForm, form);
            using (Intern_LeaveDBEntities db = new Intern_LeaveDBEntities())
            {  
                if (ModelState.IsValid)
                {
                    Application myApp = new Application();
                    myApp.Date = DateTime.Now;
                    myApp.Emp_Name = Session["username"].ToString();
                    myApp.Emp_ID = int.Parse(Session["userid"].ToString());
                    myApp.First_Day = appForm.First_Day;
                    myApp.Last_Day = appForm.Last_Day;
                    myApp.Leave_Days = numberOfLeaveDays(appForm.First_Day, appForm.Last_Day);
                    myApp.Reason_Leave = appForm.Reason_Leave;
                    myApp.App_Status = "Pending";
                    myApp.Type_Leave = appForm.Type_Leave;
                    myApp.standIn = appForm.StandIn;

                    var overlappingApps = db.Applications.Where(a => a.App_Status == "Approved" && (a.Last_Day >= myApp.First_Day || a.First_Day <= myApp.Last_Day)).Select(a => a.Emp_ID).ToList();

                    string user = Session["username"].ToString();
                    var availEmpList = db.Employees.Where(b => b.Emp_Name != user && b.Emp_Division != "Manager" && overlappingApps.Contains(b.Emp_ID) == false).Select(b => b.Emp_Name + " " + b.Emp_Surname).ToList();
                    appForm.Names = availEmpList;

                    
                    appForm.File = Request.Files[0];
                    if (appForm.File != null)
                    {
                        appForm.Emp_SickNote = new byte[appForm.File.ContentLength];
                        appForm.File.InputStream.Read(appForm.Emp_SickNote, 0, appForm.File.ContentLength);
                        myApp.Emp_SickNote = appForm.Emp_SickNote;
                    }


                    
                    if (myApp.Leave_Days == 0)
                    {
                        Response.Write("<script LANGUAGE='JavaScript' >alert('Your Leave must be on a working day')</script>");
                        return View(appForm);
                    }


                   if (!availEmpList.Contains(myApp.standIn))
                    {
                        Response.Write("<script LANGUAGE='JavaScript' >alert('User currently on leave please select another stand in.')</script>");
                        return View(appForm);
                    }

                    if (appForm.First_Day > appForm.Last_Day)
                    {
                        Response.Write("<script LANGUAGE='JavaScript' >alert('Your last day of leave cannot occur before your start day.')</script>");
                        return View(appForm);
                    }
                    //Validate leave limit
                    if (myApp.Leave_Days >= 10)
                    {
                        Response.Write("<script LANGUAGE='JavaScript' >alert('You can not take more than 10 leave days at once')</script>");
                        return View(appForm);
                    }
                    //defining leave days Database.
                   var leaveDb = db.Leave_Days.Single(t => t.Emp_ID == myApp.Emp_ID);
                    //validate if Radiolist is checked.
                    if (appForm.Type_Leave == null)
                    {
                        Response.Write("<script LANGUAGE='JavaScript' >alert('You can not take more than 10 leave days at once')</script>");
                        return View(appForm);
                    }
                    //validating leave limit
                    switch (myApp.Type_Leave)
                    {
                        case "Annual Leave":
                            if ((leaveDb.Annual_Leave == 0 && myApp.Type_Leave.Contains("Annual")) || (leaveDb.Annual_Leave < myApp.Leave_Days))
                            {
                                Response.Write("<script LANGUAGE='JavaScript' >alert('You have exhausted your Annual leave days')</script>");
                                return View(appForm);
                            }
                            break;

                        case "Sick Leave":
                            if (leaveDb.Sick_Leave == 0 && myApp.Type_Leave.Contains("Sick") || (leaveDb.Sick_Leave < myApp.Leave_Days))
                            {
                                Response.Write("<script LANGUAGE='JavaScript' >alert('You have exhausted your Sick leave days')</script>");
                                return View(appForm);
                            }
                            break;

                        case "Compassionate/family Leave":
                            if (leaveDb.Fam_Leave == 0 && myApp.Type_Leave.Contains("fam") || (leaveDb.Fam_Leave < myApp.Leave_Days))
                            {
                                Response.Write("<script LANGUAGE='JavaScript' >alert('You have exhausted your Family Responsebility leave days')</script>");
                                return View(appForm);
                            }
                            break;
                    }
                    db.Applications.Add(myApp);
                    db.SaveChanges();
                  
                    
                    Response.Write("<script LANGUAGE='JavaScript' >alert('Leave booked awaiting for manager approval')</script>");
                }

                else // (!ModelState.IsValid)
                {

                    return RedirectToAction("ErrorPage");
                    //return Content(";,l");
                }

            }
            return View(appForm);

        }
        public ActionResult GetSummary(string username)
        {
            var db = new Intern_LeaveDBEntities();
            //string user = Session["username"].ToString();
            var leaveDetails = db.Applications.Where(e => e.Emp_Name == username).ToList();
            return View(leaveDetails);
        }
        private static int numberOfLeaveDays(DateTime start, DateTime stop)
        {
            var Holidays = new List<DateTime>();

            Holidays.Add(new DateTime(DateTime.Now.Year, 1, 1));
            Holidays.Add(new DateTime(DateTime.Now.Year, 1, 2));
            Holidays.Add(new DateTime(DateTime.Now.Year, 3, 21));
            Holidays.Add(new DateTime(DateTime.Now.Year, 4, 27));
            Holidays.Add(new DateTime(DateTime.Now.Year, 6, 16));
            Holidays.Add(new DateTime(DateTime.Now.Year, 3, 10));
            Holidays.Add(new DateTime(DateTime.Now.Year, 9, 24));
            Holidays.Add(new DateTime(DateTime.Now.Year, 12, 16));
            Holidays.Add(new DateTime(DateTime.Now.Year, 12, 25));
            Holidays.Add(new DateTime(DateTime.Now.Year, 12, 26));

            int days = 0;
            
            while (start <= stop)
            {
                if (start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday || Holidays.Any(a => a == start))
                { }
                else
                {
                    ++days;
                }

                start = start.AddDays(1);
            }
                return days;
        }

        public ActionResult AboutUs()
        {
            return View();
        }

        public ActionResult ContactUs()
        {
            return View();
        }

        //[HttpPost]
        public ActionResult changePassword(FormCollection form)
        {
            var changePassword = new changePasswordViewModel();
            TryUpdateModel(changePassword);
            var db = new Intern_LeaveDBEntities();
            if (ModelState.IsValid)
            {
                string usr = Session["username"].ToString();
                var emp = db.Employees.Where(q => q.Emp_Name == usr).FirstOrDefault();
                
                if (changePassword.old_Password != emp.Emp_Password)
                {
                    Response.Write("<script LANGUAGE='JavaScript' >alert('Incorrect Old password')</script>");
                    return View(changePassword);
                }
                if (changePassword.new_Password != changePassword.confirm_Password)
                {
                    Response.Write("<script LANGUAGE='JavaScript' >alert('Confirmation Password and New Password do not Match')</script>");
                    return View(changePassword);
                }
                emp.Emp_Password = changePassword.new_Password;

                db.SaveChanges();
                Response.Write("<script LANGUAGE='JavaScript' >alert('Password changed successfully')</script>");
            }
            return View();
        }

        public ActionResult DisplayNote(int id)
        {

            using (Intern_LeaveDBEntities db = new Intern_LeaveDBEntities())
            {

                var stream = (from m in db.Applications where m.App_ID == id select m.Emp_SickNote).FirstOrDefault();

                return File(stream, "application/pdf");
            }

        }

        public ActionResult GetAvailableStandins(DateTime start, DateTime end)
        {
            using (Intern_LeaveDBEntities db = new Intern_LeaveDBEntities())
            {

                var overlappingApps = db.Applications.Where(a => a.Last_Day >= start || a.First_Day <= end).Select(a => a.Emp_ID).ToList();

                string user = Session["username"].ToString();
                var availEmpList = db.Employees.Where(b => b.Emp_Name != user && b.Emp_Division != "Manager" && overlappingApps.Contains(b.Emp_ID) == false).Select(b => b.Emp_Name + " " + b.Emp_Surname).ToList();

                return Json(availEmpList);
            }
        }

        public ActionResult ErrorPage()
        {
            return View();
        }
    }
}