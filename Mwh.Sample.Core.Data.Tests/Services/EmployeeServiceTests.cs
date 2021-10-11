using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.Common.Models;
using Mwh.Sample.Core.Data.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Core.Data.Tests.Services
{
    [TestClass]
    public class EmployeeServiceTests
    {
        private EmployeeService service;
        private CancellationToken token;

        [TestMethod]
        public void AddMultiple_StateUnderTest_Null()
        {
            // Arrange
            string[] namelist = null;

            // Act
            var result = service.AddMultipleEmployees(
                namelist);

            // Assert
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void AddMultiple_StateUnderTest_Valid()
        {
            // Arrange
            string[] namelist = new string[] { "TestMultiple1", "TestMultiple2", "TestMultiple3" };

            // Act
            var initResults = service.Get();
            var result = service.AddMultipleEmployees(namelist);
            var afterResults = service.Get();
            var test = afterResults.Where(w => w.Name == "TestMultiple3").FirstOrDefault();

            // Assert
            Assert.IsNotNull(test);
            Assert.AreEqual((initResults.Length + 3), afterResults.Length);
        }

        [TestMethod]
        public async Task DeleteAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            int id = 0;

            // Act
            var result = await service.DeleteAsync(
                id,
                token);

            // Assert
            Assert.AreEqual(result.Success, false);
        }

        [TestMethod]
        public void Dispose_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            // Act
            service.Dispose();

            // Assert
        }

        [TestMethod]
        public async Task FindByIdAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            int id = 0;

            // Act
            var result = await service.FindByIdAsync(id, token);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void Get_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            // Act
            var result = service.Get();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            // Act
            var result = await service.GetAsync(token);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestInitialize]
        public void Initialize()
        {
            service = new EmployeeService();
            token = default;
        }

        [TestMethod]
        public void Save_StateUnderTest_Null()
        {
            // Arrange
            EmployeeModel item = null;

            // Act
            var result = service.Save(
                item);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Save_StateUnderTest_ValidNew()
        {
            // Arrange
            EmployeeModel item = new()
            {
                Name = "Test",
                Age = 25,
                State = "Texas",
                Country = "USA",
                Department = EmployeeDepartment.IT,
            };

            // Act
            var result = service.Save(item);

            var UpdateEmp = await service.FindByIdAsync(result.Resource.id, token).ConfigureAwait(true);

            UpdateEmp.Age = 50;

            var UpdateResult = service.Save(UpdateEmp);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(UpdateEmp);
            Assert.IsNotNull(UpdateResult);
            Assert.IsTrue(UpdateResult.Success);
            Assert.AreEqual(50, UpdateResult.Resource?.Age);
        }

        [TestMethod]
        public async Task SaveAsync_StateUnderTest_Valid()
        {
            // Arrange
            EmployeeModel item = new()
            {
                Name = "Test",
                Age = 25,
                State = "Texas",
                Country = "USA",
                Department = EmployeeDepartment.IT,
            };

            // Act
            var result = await service.SaveAsync(item);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public async Task SaveAsync_StateUnderTest_Null()
        {
            // Arrange
            EmployeeModel item = null;

            // Act
            var result = await service.SaveAsync(
                item,
                token);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public async Task UpdateAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            int id = 0;
            EmployeeModel employee = null;

            // Act
            var result = await service.UpdateAsync(
                id,
                employee,
                token);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
        }
    }
}