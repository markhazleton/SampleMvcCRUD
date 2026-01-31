global using Microsoft.EntityFrameworkCore;
global using Microsoft.VisualStudio.TestTools.UnitTesting;
global using Mwh.Sample.Domain.Models;
global using Mwh.Sample.Repository.Models;
global using Mwh.Sample.Repository.Services;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]



