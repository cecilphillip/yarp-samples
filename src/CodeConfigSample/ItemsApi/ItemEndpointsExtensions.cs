using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ItemsApi;

public static class ItemEndpointsExtensions
{
    public static RouteGroupBuilder MapItemApis(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetItems).WithName(nameof(GetItems)) ;
        group.MapGet("/{id}", GetItemById).WithName(nameof(GetItemById));
        return group;
    }

    private static async Task<Results<Ok<IEnumerable<Item>>, NotFound>> GetItems(HttpContext ctx)
    {           
        var result = await DataService.GetItems();
        if (!result.Any()) return TypedResults.NotFound();
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<Item>, NotFound>> GetItemById(HttpContext ctx, [FromRoute] int id)
    {
        var item = await DataService.GetItemById(id);
        if (item == null) return TypedResults.NotFound();
        return TypedResults.Ok(item);
    }
}
