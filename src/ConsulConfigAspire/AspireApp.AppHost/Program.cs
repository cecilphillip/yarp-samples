using AspireApp.AppHost.Resources;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddConsulContainer("consul", 8500);

builder.AddProject<Projects.ItemsApi>("itemsApi")
    .WithReplicas(3);

builder.AddProject<Projects.ConsulConfigProxy>("yarpproxy");

builder.Build().Run();
