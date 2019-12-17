using Mwh.SampleCRUD.BL.Models;
using System.Web.Mvc;

namespace Mwh.Sample.WebApi.Controllers
{

    public class EmployeeController : BaseController
    {
        public JsonResult Add(EmployeeModel emp)
        {
            if (emp == null)
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
            emp.EmployeeID = 0;
            return Json(EmpDB.Update(emp), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            var myResult = EmpDB.Delete(id);
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("/home/EmpSinglePage/")]
        public ActionResult EmpSinglePage()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetbyID(int id)
        {
            var Employee = EmpDB.Get(id);
            return Json(Employee, JsonRequestBehavior.AllowGet);
        }

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

        [HttpGet]
        public JsonResult List()
        {
            return Json(EmpDB.ListAll(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Save(EmployeeModel postEmployee)
        {
            var myResult = EmpDB.Update(postEmployee);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Update(EmployeeModel emp)
        {
            return Json(EmpDB.Update(emp), JsonRequestBehavior.AllowGet);
        }
    }
}