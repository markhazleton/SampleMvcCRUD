using Mwh.SampleCRUD.BL.Models;
using System.Collections.Generic;
using System.Web.Http;

namespace Mwh.SampleCrud.Controllers
{
    public class EmployeeApiController : BaseApiController
    {
        // GET api/<controller>
        public IEnumerable<Employee> Get()
        {
            return empDB.ListAll();
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}