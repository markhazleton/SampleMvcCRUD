using Mwh.SampleCRUD.BL.Models;
using Mwh.SampleCRUD.BL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web.Http;

namespace Mwh.Sample.WebApi.Controllers
{

    [RoutePrefix("api/employee")]
    public class EmployeeApiController : BaseApiController
    {
        public EmployeeApiController()
        {

        }

        /// <summary>
        /// Get List of Employees
        /// </summary>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public IEnumerable<EmployeeModel> Get()
        {
            return EmpDB.ListAll();
        }


        /// <summary>
        /// Get Employee by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id}")]
        [HttpGet]
        public EmployeeModel Get(int id)
        {
            return EmpDB.Get(id);
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
            return EmpDB.Get(updateID);
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
