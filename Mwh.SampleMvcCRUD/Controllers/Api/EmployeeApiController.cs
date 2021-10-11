using Microsoft.AspNetCore.Mvc;
using Mwh.Sample.Common.Interfaces;
using Mwh.Sample.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Core.WebApi.Controllers
{


    /// <summary>
    /// EmployeeApiController
    /// </summary>
    [Route("/api/employee")]
    public class EmployeeApiController : BaseApiController
    {
        private readonly IEmployeeService _employeeService;

        /// <summary>
        /// EmployeeApiController
        /// </summary>
        public EmployeeApiController(IEmployeeService employeeService) : base() { _employeeService = employeeService; }

        /// <summary>
        /// Deletes a given employee according to an identifier.
        /// </summary>
        /// /// <remarks>
        /// Id must be a valid employee
        /// </remarks>
        /// <param name="id">Employee identifier.</param>
        /// <returns>Response for the request.</returns>
        [HttpDelete("{id}", Name = "DeleteEmployee")]
        [ProducesResponseType(typeof(EmployeeModel), 200)]
        [ProducesResponseType(typeof(ErrorResource), 400)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            CancellationTokenSource cts = new();
            var result = await _employeeService.DeleteAsync(id, cts.Token).ConfigureAwait(false);

            if (!result.Success)
            {
                return BadRequest(new ErrorResource(result.Message));
            }
            return Ok(result);
        }

        /// <summary>
        /// Returns Single Employee according to an identifier.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EmployeeModel), 200)]
        [ProducesResponseType(typeof(ErrorResource), 400)]
        public async Task<IActionResult> FindByIdAsync(int id)
        {
            CancellationTokenSource cts = new();
            var result = await _employeeService.FindByIdAsync(id, cts.Token).ConfigureAwait(false);

            if (result.id != id)
            {
                return BadRequest(new ErrorResource("Employee Not Found"));
            }
            return Ok(result);
        }

        /// <summary>
        /// Returns collection of all employees
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EmployeeModel>), 200)]
        public async Task<IEnumerable<EmployeeModel>> ListAsync()
        {
            CancellationTokenSource cts = new();
            var employees = await _employeeService.GetAsync(cts.Token).ConfigureAwait(false);
            return employees;
        }

        /// <summary>
        /// Creates a new employee.
        /// </summary>
        /// <param name="employee">Employee data.</param>
        /// <returns>Response for the request.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(EmployeeModel), 201)]
        [ProducesResponseType(typeof(ErrorResource), 400)]
        public async Task<IActionResult> PostAsync([FromBody] EmployeeModel employee)
        {
            if (employee == null) return BadRequest("Employee was null");

            CancellationTokenSource cts = new();
            var result = await _employeeService.SaveAsync(employee, cts.Token).ConfigureAwait(false);

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
            CancellationTokenSource cts = new();
            var result = await _employeeService.UpdateAsync(id, employee, cts.Token).ConfigureAwait(false);
            if (!result.Success)
            {
                return BadRequest(new ErrorResource(result.Message));
            }
            return Ok(result);
        }
    }
}