using ConsulConfigProxy.Workers;

namespace ConsulConfigProxy;

public static class ServiceCollectionExtensions
{
    public static IReverseProxyBuilder LoadFromConsul(this IReverseProxyBuilder builder)
    {
        builder.LoadFromMemory(default!,default!);
        builder.Services.AddHostedService<ConsulMonitorWorker>();

        return builder;
    }
}
