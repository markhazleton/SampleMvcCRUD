using SampleCRUD.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SampleCRUD.Controllers
{
    public class BaseController : Controller
    {
        public IEmployeeDB empDB
        {
            get
            {
                if (Session == null) return new EmployeeMock();

                if ((EmployeeMock)Session["EmployeeDB"] == null)
                    Session["EmployeeDB"] = new EmployeeMock();

                return ((EmployeeMock)Session["EmployeeDB"]);
            }
        }
    }
}