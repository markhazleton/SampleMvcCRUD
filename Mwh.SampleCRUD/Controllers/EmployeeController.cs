namespace SampleCRUD.Controllers
{
    using SampleCRUD.Models;
    using System;
    using System.Linq;
    using System.Web.Mvc;

    public class EmployeeController : BaseController
    {
        [HttpPost]
        public JsonResult Delete(int id)
        {
            var myResult = empDB.Delete(id);
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetEmployeeDelete(int id = 0)
        {
            return PartialView("_EmployeeDelete", empDB.Get(id));
        }

        public ActionResult GetEmployeeEdit(int id = 0)
        {
            return PartialView("_EmployeeEdit", empDB.Get(id));
        }

        public ActionResult GetEmployeeList()
        {
            return PartialView("_EmployeeList", empDB.ListAll());
        }

        // GET: Employee
        public ActionResult Index()
        {
            return View(empDB.ListAll());
        }

        [HttpPost]
        public JsonResult Save(Employee postEmployee)
        {
            var myResult = empDB.Update(postEmployee);
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}