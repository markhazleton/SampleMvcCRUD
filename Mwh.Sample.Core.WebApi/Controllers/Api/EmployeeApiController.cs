using Microsoft.AspNetCore.Mvc;
using Mwh.Sample.Common.Models;
using Mwh.Sample.Common.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    /// <summary>
    /// EmployeeApiController
    /// </summary>
    [Route("/api/employees")]
    [Produces("application/json")]
    [ApiController]
    public class EmployeeApiController : Controller
    {
        private readonly IEmployeeService _employeeService;

        /// <summary>
        /// EmployeeApiController
        /// </summary>
        public EmployeeApiController(IEmployeeService employeeService) : base()
        { _employeeService = employeeService; }

        /// <summary>
        /// Deletes a given employee according to an identifier.
        /// </summary>
        /// <param name="id">Employee identifier.</param>
        /// <returns>Response for the request.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(EmployeeModel), 200)]
        [ProducesResponseType(typeof(ErrorResource), 400)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var result = await _employeeService.DeleteAsync(id, cts.Token).ConfigureAwait(true);

            if (!result.Success)
            {
                return BadRequest(new ErrorResource(result.Message));
            }
            return Ok(result);
        }

        ///// <summary>
        ///// Lists all employees.
        ///// </summary>
        ///// <returns>List os employees.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EmployeeModel>), 200)]
        public async Task<IEnumerable<EmployeeModel>> ListAsync()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var employees = await _employeeService.ListAsync(cts.Token).ConfigureAwait(true);
            return employees;
        }
        ///// <summary>
        ///// Get Single Employee.
        ///// </summary>
        ///// <returns>List os employees.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EmployeeModel), 200)]
        [ProducesResponseType(typeof(ErrorResource), 400)]
        public async Task<IActionResult> FindByIdAsync(int id)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var result = await _employeeService.FindByIdAsync(id, cts.Token).ConfigureAwait(true);

            if (result.EmployeeID != id)
            {
                return BadRequest(new ErrorResource("Employee Not Found"));
            }
            return Ok(result);
        }
        /// <summary>
        /// Saves a new employee.
        /// </summary>
        /// <param name="employee">Employee data.</param>
        /// <returns>Response for the request.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(EmployeeModel), 201)]
        [ProducesResponseType(typeof(ErrorResource), 400)]
        public async Task<IActionResult> PostAsync([FromBody] EmployeeModel employee)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var result = await _employeeService.SaveAsync(employee, cts.Token).ConfigureAwait(true);

            if (!result.Success)
            {
                return BadRequest(new ErrorResource(result.Message));
            }
            return Ok(result);
        }

        /// <summary>
        /// Updates an existing employee according to an identifier.
        /// </summary>
        /// <param name="id">Employee identifier.</param>
        /// <param name="employee">Updated employee data.</param>
        /// <returns>Response for the request.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EmployeeModel), 200)]
        [ProducesResponseType(typeof(ErrorResource), 400)]
        public async Task<IActionResult> PutAsync(int id, [FromBody] EmployeeModel employee)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var result = await _employeeService.UpdateAsync(id, employee, cts.Token).ConfigureAwait(true);
            if (!result.Success)
            {
                return BadRequest(new ErrorResource(result.Message));
            }
            return Ok(result);
        }
    }
}
