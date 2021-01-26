using Mwh.Sample.Common.Models;
using System.Collections.Generic;
using System.Web.Http;

namespace Mwh.Sample.WebApi.Controllers
{

    /// <summary>
    /// EmployeeApiController
    /// </summary>
    [RoutePrefix("api/employee")]
    public class EmployeeApiController : BaseApiController
    {
        /// <summary>
        /// EmployeeApiController
        /// </summary>
        public EmployeeApiController()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("Simple/Test")]
        [HttpGet]
        public IHttpActionResult TestResult()
        {
            return LogRequest();

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
        /// Get Employee by Id
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
        [HttpPut]
        public EmployeeModel Post(EmployeeModel employee)
        {
            return EmpDB.Update(employee);
        }

        /// <summary>
        /// Delete Employee by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id}")]
        [HttpDelete]
        public bool Delete(int id)
        {
            return EmpDB.Delete(id);
        }
    }
}
