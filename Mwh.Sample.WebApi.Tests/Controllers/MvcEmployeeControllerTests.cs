using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.Common.Models;

namespace Mwh.Sample.WebApi.Controllers
    {
    [TestClass]
    public class MvcEmployeeControllerTests
        {

        private MvcEmployeeController mvcEmployeeController;
        private EmployeeModel employee;
        private int id;
        [TestInitialize]
        public void TestInitialize()
            {
            employee = new EmployeeModel();
            id = 0;
            mvcEmployeeController = new MvcEmployeeController()
                {
                ControllerContext = new MvcFakes.FakeControllerContext(new MvcEmployeeController(), "MarkHazleton"),
                };
            }


        [TestMethod]
        public void Index_StateUnderTest_ExpectedBehavior()
            {
            // Arrange

            // Act
            var result = mvcEmployeeController.Index();

            // Assert
            Assert.IsNotNull(result);
            }

        [TestMethod]
        public void Details_StateUnderTest_ExpectedBehavior()
            {
            // Arrange

            // Act
            var result = mvcEmployeeController.Details(id);

            // Assert
            Assert.IsNotNull(result);
            }

        [TestMethod]
        public void Create_StateUnderTest_ExpectedBehavior()
            {
            // Arrange

            // Act
            var result = mvcEmployeeController.Create();

            // Assert
            Assert.IsNotNull(result);
            }

        [TestMethod]
        public void Create_StateUnderTest_ExpectedBehavior1()
            {
            // Arrange

            // Act
            var result = mvcEmployeeController.Create(employee);

            // Assert
            Assert.IsNotNull(result);
            }

        [TestMethod]
        public void Edit_StateUnderTest_ExpectedBehavior()
            {
            // Arrange

            // Act
            var result = mvcEmployeeController.Edit(id);

            // Assert
            Assert.IsNotNull(result);
            }

        [TestMethod]
        public void Edit_StateUnderTest_ExpectedBehavior1()
            {
            // Arrange

            // Act
            var result = mvcEmployeeController.Edit(id,employee);

            // Assert
            Assert.IsNotNull(result);
            }

        [TestMethod]
        public void Delete_StateUnderTest_ExpectedBehavior()
            {
            // Arrange

            // Act
            var result = mvcEmployeeController.Delete(id);

            // Assert
            Assert.IsNotNull(result);
            }

        [TestMethod]
        public void Delete_StateUnderTest_ExpectedBehavior1()
            {
            // Arrange

            // Act
            var result = mvcEmployeeController.Delete(id,employee);

            // Assert
            Assert.IsNotNull(result);
            }
        }
    }
