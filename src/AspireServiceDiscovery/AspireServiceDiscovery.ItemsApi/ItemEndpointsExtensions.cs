using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AspireServiceDiscovery.ItemsApi;

public static class ItemEndpointsExtensions
{
    public static RouteGroupBuilder MapItemApis(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetItems).WithName(nameof(GetItems)) ;
        group.MapGet("/{id}", GetItemById).WithName(nameof(GetItemById));
        return group;
    }

    private static async Task<Results<Ok<Item[]>, NotFound>> GetItems()
    {
        var result = await DataService.GetItems();
        var enumerable = result as Item[] ?? result.ToArray();
        if (!enumerable.Any()) return TypedResults.NotFound();
        return TypedResults.Ok(enumerable);
    }

    private static async Task<Results<Ok<Item>, NotFound>> GetItemById([FromRoute] int id)
    {
        var item = await DataService.GetItemById(id);
        if (item == null) return TypedResults.NotFound();
        return TypedResults.Ok(item);
    }
}
