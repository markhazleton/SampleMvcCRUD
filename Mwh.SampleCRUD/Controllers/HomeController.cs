using SampleCRUD.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SampleCRUD.Controllers
{
    public class HomeController : BaseController
    {
        public JsonResult Add(Employee emp)
        {
            emp.EmployeeID = 0;
            return Json(empDB.Update(emp), JsonRequestBehavior.AllowGet);
        }
        public JsonResult Delete(int ID)
        {
            return Json(empDB.Delete(ID), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetbyID(int id)
        {
            var Employee = empDB.Get(id);
            return Json(Employee, JsonRequestBehavior.AllowGet);
        }
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult List()
        {
            return Json(empDB.ListAll(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult Update(Employee emp)
        {
            return Json(empDB.Update(emp), JsonRequestBehavior.AllowGet);
        }
    }
}