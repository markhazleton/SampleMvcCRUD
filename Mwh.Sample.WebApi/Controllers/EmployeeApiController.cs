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

        [Route("")]
        [HttpGet]
        public IEnumerable<Employee> Get()
        {
            return EmpDB.ListAll();
        }


        [Route("{id}")]
        [HttpGet]
        public Employee Get(int id)
        {
            return EmpDB.Get(id);
        }

        [Route("")]
        [HttpPost]
        public Employee Post(Employee employee)
        {
            var updateID = EmpDB.Update(employee);
            return EmpDB.Get(updateID);
        }
    }
}
