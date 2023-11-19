using Microsoft.AspNetCore.Http.HttpResults;

namespace DemoAddressApi;

public static class AddressEndpointsExtensions
{
    public static RouteGroupBuilder MapAddressApis(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAddresses).WithName(nameof(GetAddresses));
        group.MapGet("/{id}", GetAddressById).WithName(nameof(GetAddressById));
        return group;
    }

    private static async Task<Results<Ok<IEnumerable<Address>>, NotFound>> GetAddresses()
    {
        var result = await DataService.GetAddresses();
        if (!result.Any()) return TypedResults.NotFound();
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<Address>, NotFound>> GetAddressById(int id)
    {
        var item = await DataService.GetAddressById(id);
        if (item == null) return TypedResults.NotFound();
        return TypedResults.Ok(item);
    }
}
