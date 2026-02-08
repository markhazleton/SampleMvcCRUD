global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Logging.Abstractions;
global using Microsoft.VisualStudio.TestTools.UnitTesting;
global using UISampleSpark.Core.Models;
global using UISampleSpark.Data.Models;
global using UISampleSpark.Data.Services;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]



