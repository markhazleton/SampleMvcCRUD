var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Mwh_Sample_Web>("webfrontend");

builder.Build().Run();
