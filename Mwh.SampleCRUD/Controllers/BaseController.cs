using Mwh.SampleCRUD.BL.Repositories;
using System.Web.Mvc;

namespace SampleCRUD.Controllers
{
    public class BaseController : Controller
    {
        public IEmployeeDB empDB
        {
            get
            {
                if (HttpContext.Application == null)
                    return new EmployeeMock();

                if ((EmployeeMock)HttpContext.Application["EmployeeDB"] == null)
                    HttpContext.Application["EmployeeDB"] = new EmployeeMock();

                return ((EmployeeMock)HttpContext.Application["EmployeeDB"]);
            }
        }
    }
}