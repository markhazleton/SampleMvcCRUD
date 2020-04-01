﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Mwh.Sample.Common.Models;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    /// <summary>
    /// MvcEmployeeController
    /// </summary>
    public class MvcEmployeeController : BaseController
    {
        public MvcEmployeeController(ILogger<HomeController> logger, IMemoryCache memoryCache) : base(logger, memoryCache)
        {
        }
        /// <summary>
        /// Default Page for MvcEmployeeController
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            return View(EmpDB.EmployeeCollection());
        }


        /// <summary>
        /// View Employee Details
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Details(int id)
        {
            return View(EmpDB.Employee(id));
        }


        /// <summary>
        /// Load Page to Create A New Employee
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Create()
        {
            return View(new EmployeeModel());
        }


        /// <summary>
        /// Save New Employee
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EmployeeModel employee)
        {
            if (employee != null)
            {
                employee.EmployeeID = EmpDB.Update(employee);
            }
            return RedirectToAction("Index");

        }


        /// <summary>
        /// Edit an employee by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit(int id)
        {
            return View(EmpDB.Employee(id));
        }


        /// <summary>
        /// Save Employee 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, EmployeeModel employee)
        {
            if (employee != null)
            {
                if (employee.EmployeeID == id)
                    EmpDB.Update(employee);
            }
            return RedirectToAction("Index");

        }


        /// <summary>
        /// Select an Employee to delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Delete(int id)
        {
            return View(EmpDB.Employee(id));
        }


        /// <summary>
        /// Delete Employee
        /// </summary>
        /// <param name="id"></param>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, EmployeeModel employee)
        {
            if (employee != null)
            {
                if (employee.EmployeeID == id)
                    _ = EmpDB.Delete(employee.EmployeeID);
            }
            return RedirectToAction("Index");
        }
    }
}