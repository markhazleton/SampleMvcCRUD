using Mwh.Sample.Common.Interfaces;
using Mwh.Sample.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Common.Repositories
{
    /// <summary>
    /// Class EmployeeService.
    /// Implements the <see cref="Mwh.Sample.Common.Repositories.IEmployeeService" />
    /// </summary>
    /// <seealso cref="Mwh.Sample.Common.Repositories.IEmployeeService" />
    public class EmployeeService : IEmployeeService
    {
        /// <summary>
        /// The employee repository
        /// </summary>
        private readonly IEmployeeRepository _employeeRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeService"/> class.
        /// </summary>
        /// <param name="employeeRepository">The employee repository.</param>
        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        /// <summary>
        /// Lists the asynchronous.
        /// </summary>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;IEnumerable&lt;EmployeeModel&gt;&gt;.</returns>
        public Task<IEnumerable<EmployeeModel>> ListAsync(CancellationToken token)
        {
            return _employeeRepository.ListAsync(token);
        }

        /// <summary>
        /// Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;EmployeeModel&gt;.</returns>
        public Task<EmployeeModel> FindByIdAsync(int id, CancellationToken token)
        {
            return _employeeRepository.FindByIdAsync(id,token);
        }

        /// <summary>
        /// save as an asynchronous operation.
        /// </summary>
        /// <param name="employee">The employee.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>EmployeeResponse.</returns>
        public async Task<EmployeeResponse> SaveAsync(EmployeeModel employee, CancellationToken token)
        {
            try
            {
                await _employeeRepository.AddAsync(employee, token).ConfigureAwait(true);

                return new EmployeeResponse(employee);
            }
            catch (Exception ex)
            {
                // Do some logging stuff
                return new EmployeeResponse($"An error occurred when saving the employee: {ex.Message}");
            }
        }

        /// <summary>
        /// update as an asynchronous operation.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="employee">The employee.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>EmployeeResponse.</returns>
        public async Task<EmployeeResponse> UpdateAsync(int id, EmployeeModel employee, CancellationToken token)
        {
            if (employee == null)
                return new EmployeeResponse("Employee is null.");

            if (employee.EmployeeID != id)
                return new EmployeeResponse($"Mismatch in id({id}) && employee_id({employee.EmployeeID}).");

            var existingEmployee = await _employeeRepository.FindByIdAsync(id, token).ConfigureAwait(true);
            if (existingEmployee == null)
                return new EmployeeResponse("Employee not found.");

            try
            {
                var response = await _employeeRepository.Update(employee,token).ConfigureAwait(true);
                return new EmployeeResponse(response);
            }
            catch (Exception ex)
            {
                // Do some logging stuff
                return new EmployeeResponse($"An error occurred when updating the employee: {ex.Message}");
            }
        }

        /// <summary>
        /// delete as an asynchronous operation.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>EmployeeResponse.</returns>
        public async Task<EmployeeResponse> DeleteAsync(int id, CancellationToken token)
        {
            var existingEmployee = await _employeeRepository.FindByIdAsync(id, token).ConfigureAwait(true);

            if (existingEmployee == null)
                return new EmployeeResponse("Employee not found.");

            if (!existingEmployee.IsValid)
                return new EmployeeResponse("Employee not found.");

            try
            {
                var response = await _employeeRepository.Remove(existingEmployee,token).ConfigureAwait(true);

                if (response)
                    existingEmployee.EmployeeID = 0;

                return new EmployeeResponse(existingEmployee);
            }
            catch (Exception ex)
            {
                // Do some logging stuff
                return new EmployeeResponse($"An error occurred when deleting the employee: {ex.Message}");
            }
        }
    }
}
