using AspireApp.AppHost.Resources;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddConsulContainer();

builder.AddProject<Projects.ItemsApi>("itemsApi")
    .WithReplicas(3);

builder.AddProject<Projects.ConsulConfigProxy>("yarpProxy");


builder.Build().Run();
