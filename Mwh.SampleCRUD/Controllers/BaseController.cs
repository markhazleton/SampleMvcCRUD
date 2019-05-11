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
                if(HttpContext.Application == null)
                    return new EmployeeMock();

                if((EmployeeMock)HttpContext.Application["EmployeeDB"] == null)
                    HttpContext.Application["EmployeeDB"] = new EmployeeMock();

                return ((EmployeeMock)HttpContext.Application["EmployeeDB"]);
            }
        }
    }
}