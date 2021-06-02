﻿using Microsoft.AspNetCore.Mvc;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    /// <summary>
    /// EmployeeController
    /// </summary>
    public class EmployeeReactController : BaseController
    {
        public EmployeeReactController() : base()
        {
        }
        /// <summary>
        /// Default Page for Employee React Controller
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index() { return View(); }
    }
}