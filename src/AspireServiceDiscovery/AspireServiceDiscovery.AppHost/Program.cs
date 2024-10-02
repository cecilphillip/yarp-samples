using AspireServiceDiscovery.AppHost.Resources;

var builder = DistributedApplication.CreateBuilder(args);

//TODO: Read consul json config file?
var consulServer = builder.AddConsul("consul-server", dataCenter:"yarp-aspire-sd");

builder.AddProject<Projects.AspireServiceDiscovery_ItemsApi>("itemsApi")
    .WithReference(consulServer)
    .WithReplicas(3);

builder.AddProject<Projects.AspireServiceDiscovery_Proxy>("yarp-proxy")
    .WithReference(consulServer);

builder.AddProject<Projects.AspireServiceDiscovery_Console>("console")
    .WithReference(consulServer);

builder.Build().Run();