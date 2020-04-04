using Mwh.Sample.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Common.Repositories
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public Task<IEnumerable<EmployeeModel>> ListAsync(CancellationToken token)
        {
            return _employeeRepository.ListAsync(token);
        }

        public Task<EmployeeModel> FindByIDAsync(int id, CancellationToken token)
        {
            return _employeeRepository.FindByIdAsync(id,token);
        }

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

        public async Task<EmployeeResponse> UpdateAsync(int id, EmployeeModel employee, CancellationToken token)
        {
            var existingEmployee = await _employeeRepository.FindByIdAsync(id, token).ConfigureAwait(true);

            if (existingEmployee == null)
                return new EmployeeResponse("Employee not found.");

            existingEmployee.Name = employee.Name;

            try
            {
                _employeeRepository.Update(existingEmployee);

                return new EmployeeResponse(existingEmployee);
            }
            catch (Exception ex)
            {
                // Do some logging stuff
                return new EmployeeResponse($"An error occurred when updating the employee: {ex.Message}");
            }
        }

        public async Task<EmployeeResponse> DeleteAsync(int id, CancellationToken token)
        {
            var existingEmployee = await _employeeRepository.FindByIdAsync(id, token).ConfigureAwait(true);

            if (existingEmployee == null)
                return new EmployeeResponse("Employee not found.");

            try
            {
                _employeeRepository.Remove(existingEmployee);

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
