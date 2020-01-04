using Mwh.SampleCRUD.BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mwh.Sample.WebApi.Controllers
{
    public class MvcEmployeeController : BaseController
    {
        // GET: MvcEmployee
        [HttpGet]
        public ActionResult Index()
        {
            return View(EmpDB.ListAll());
        }

        // GET: MvcEmployee/Details/5
        [HttpGet]
        public ActionResult Details(int id)
        {
            return View(EmpDB.Get(id));
        }

        // GET: MvcEmployee/Create
        [HttpGet]
        public ActionResult Create()
        {
            return View(new EmployeeModel());
        }

        // POST: MvcEmployee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EmployeeModel employee)
        {
            try
            {
                if (employee != null)
                {
                    employee.EmployeeID = EmpDB.Update(employee);
                }
                return RedirectToAction("Index");
            }
            catch
            {
                if (employee != null)
                {
                    return View(employee);
                }
                return View(new EmployeeModel());
            }
        }

        // GET: MvcEmployee/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            return View(EmpDB.Get(id));
        }

        // POST: MvcEmployee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, EmployeeModel employee)
        {
            try
            {
                if (employee != null)
                {
                    if (employee.EmployeeID == id)
                        EmpDB.Update(employee);
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View(EmpDB.Get(id));
            }
        }

        // GET: MvcEmployee/Delete/5
        [HttpGet]
        public ActionResult Delete(int id)
        {
            return View(EmpDB.Get(id));
        }

        // POST: MvcEmployee/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, EmployeeModel employee)
        {
            try
            {
                if (employee != null)
                {
                    if (employee.EmployeeID == id)
                        _ = EmpDB.Delete(employee.EmployeeID);
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View(EmpDB.Get(id));
            }
        }
    }
}
