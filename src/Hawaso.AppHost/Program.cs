var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Hawaso_ApiService>("apiservice");

builder.AddProject<Projects.Hawaso_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
