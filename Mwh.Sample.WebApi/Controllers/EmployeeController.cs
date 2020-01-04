using Mwh.SampleCRUD.BL.Models;
using System.Web.Mvc;

namespace Mwh.Sample.WebApi.Controllers
{

    public class EmployeeController : BaseController
    {

        [HttpGet]
        public ActionResult GetEmployeeDelete(int id = 0)
        {
            return PartialView("_EmployeeDelete", EmpDB.Get(id));
        }

        [HttpGet]
        public ActionResult GetEmployeeEdit(int id = 0)
        {
            return PartialView("_EmployeeEdit", EmpDB.Get(id));
        }

        [HttpGet]
        public ActionResult GetEmployeeList()
        {
            return PartialView("_EmployeeList", EmpDB.ListAll());
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}