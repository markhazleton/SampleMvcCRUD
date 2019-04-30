using SampleCRUD.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SampleCRUD.Controllers
{
    public class EmployeeController : BaseController
    {
        public ActionResult Create()
        {
            return View("Edit", new Employee());
        }
        public ActionResult Delete(int id)
        {
            return View(empDB.Get(id));
        }
        public ActionResult Details(int id)
        {
            return View(empDB.Get(id));
        }
        public ActionResult Edit(int id = 0)
        {
            return View(empDB.Get(id));
        }

        public ActionResult GetEmployeeEdit(int id = 0)
        {
            return PartialView("_EmployeeEdit", empDB.Get(id));
        }

        public ActionResult GetEmployeeList()
        {
            return PartialView("_EmployeeList", empDB.ListAll());
        }

        [HttpPost]
        public JsonResult Save(Employee postEmployee)
        {
            var myResult = empDB.Update(postEmployee);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        // GET: Employee
        public ActionResult Index()
        {
            return View(empDB.ListAll());
        }
    }
}