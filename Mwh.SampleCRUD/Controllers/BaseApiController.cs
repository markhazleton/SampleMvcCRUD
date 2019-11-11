using Mwh.SampleCRUD.BL.Repositories;
using System;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Mwh.SampleCrud.Controllers
{
    public abstract class BaseApiController : ApiController
    {
        protected BaseApiController() { }

        public IEmployeeDB empDB
        {
            get
            {
                if (HttpContext.Current.Application == null)
                    return new EmployeeMock();

                if ((EmployeeMock)HttpContext.Current.Application["EmployeeDB"] == null)
                    HttpContext.Current.Application["EmployeeDB"] = new EmployeeMock();

                return ((EmployeeMock)HttpContext.Current.Application["EmployeeDB"]);
            }
        }
    }
}
