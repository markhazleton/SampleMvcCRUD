// <copyright file="JobAssignmentTest.cs">Copyright ©  2019</copyright>
using System;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.SampleCRUD.BL.Models;

namespace Mwh.SampleCRUD.BL.Tests.Models
{
    /// <summary>This class contains parameterized unit tests for JobAssignment</summary>
    [PexClass(typeof(JobAssignment))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [TestClass]
    public partial class JobAssignmentTest
    {
    }
}
