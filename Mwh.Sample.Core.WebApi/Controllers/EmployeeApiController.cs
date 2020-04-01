using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Mwh.Sample.Common.Models;
using System;
using System.Collections.Generic;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    /// <summary>
    /// EmployeeApiController
    /// </summary>
    [Route("[controller]")]
    public class EmployeeApiController : BaseApiController
    {
        /// <summary>
        /// EmployeeApiController
        /// </summary>
        public EmployeeApiController(IMemoryCache memoryCache) : base(memoryCache)
        {
            
        }

        /// <summary>
        /// Employee List of Employees
        /// </summary>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public IEnumerable<EmployeeModel> Get()
        {
            return EmpDB.EmployeeCollection();
        }


        /// <summary>
        /// Employee by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id}")]
        [HttpGet]
        public EmployeeModel Get(int id)
        {
            return EmpDB.Employee(id);
        }

        /// <summary>
        /// Add / Update Employee
        /// </summary>
        /// <param name="employee"></param>
        /// <returns>Employee</returns>
        [Route("")]
        [HttpPost]
        public EmployeeModel Post(EmployeeModel employee)
        {
            var updateID = EmpDB.Update(employee);
            return EmpDB.Employee(updateID);
        }

        /// <summary>
        /// Delete Employee by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id}")]
        [HttpDelete]
        public int Delete(int id)
        {
            return EmpDB.Delete(id);
        }
    }
}
