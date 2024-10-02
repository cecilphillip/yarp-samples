using System.Text.Json;
using AspireServiceDiscovery.ServiceDefaults.ServiceEndpointProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddConsulDnsSrvServiceEndpointProvider(builder.Configuration.GetValue<string>("services:consul-server:dns-udp:0"), options =>
{
    options.DataCenter = builder.Configuration.GetValue<string>("CONSUL_DATACENTER");
});

builder.Services.AddHttpClient().
    ConfigureHttpClientDefaults(httpBuilder =>
    {
        httpBuilder.AddStandardResilienceHandler();
        httpBuilder.AddServiceDiscovery();
    });

var host = builder.Build();

var httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();
using var httpClient = httpClientFactory.CreateClient();
await Task.Delay(5000);
var response = await httpClient.GetAsync("http://itemsapi/api/items");
var stream = await response.Content.ReadAsStreamAsync();
using var jsonDoc = await JsonDocument.ParseAsync(stream);

var jsonText = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });

Console.WriteLine(jsonText);
Console.Read();

// var dnsQuery = host.Services.GetRequiredService<IDnsQuery>(); 
// var results = await dnsQuery.QueryAsync("itemsapi.service.yarp-aspire-sd.dc.consul", QueryType.SRV);
//
// var serviceRecords = results.Answers.SrvRecords();
//
// foreach (var serviceRecord in serviceRecords)
// {
//     Console.WriteLine($"Service: {serviceRecord.DomainName} - {serviceRecord.Target}:{serviceRecord.Port}");
// }