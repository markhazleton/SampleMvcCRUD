namespace SampleCRUD.Controllers
{
    using Mwh.SampleCRUD.BL.Models;
    using System.Web.Mvc;

    public class EmployeeController : BaseController
    {
        public JsonResult Add(Employee emp)
        {
            emp.EmployeeID = 0;
            return Json(empDB.Update(emp), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            var myResult = empDB.Delete(id);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [Route("/home/EmpSinglePage/")]
        public ActionResult EmpSinglePage()
        {
            return View();
        }

        public JsonResult GetbyID(int id)
        {
            var Employee = empDB.Get(id);
            return Json(Employee, JsonRequestBehavior.AllowGet);
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
            return View();
        }

        public JsonResult List()
        {
            return Json(empDB.ListAll(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Save(Employee postEmployee)
        {
            var myResult = empDB.Update(postEmployee);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Update(Employee emp)
        {
            return Json(empDB.Update(emp), JsonRequestBehavior.AllowGet);
        }
    }
}