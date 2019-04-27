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
        public ActionResult Edit(int id=0)
        {
            return View(empDB.Get(id));
        }

        [HttpPost]
        public ActionResult Save(Employee postEmployee)
        {
            var myResult = empDB.Update(postEmployee);
            return View("Index", empDB.ListAll());
        }
        // GET: Employee
        public ActionResult Index()
        {
            return View(empDB.ListAll());
        }
    }
}