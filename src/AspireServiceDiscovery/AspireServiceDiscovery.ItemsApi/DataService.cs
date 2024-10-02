using Bogus;

namespace AspireServiceDiscovery.ItemsApi;

public static class DataService
{
    private static List<Item> Items { get; }

    static DataService()
    {
        Items = new Faker<Item>()
            .CustomInstantiator( f=>
                new Item(
                    f.IndexVariable++,
                    f.Commerce.ProductName(),
                    f.PickRandom(f.Commerce.Categories(5)),
                    f.Address.Country(),
                    f.Random.Number(10, 100) )
            ).Generate(50);
    }

    public static ValueTask<IEnumerable<Item>> GetItems()
    {
        return new ValueTask<IEnumerable<Item>>(Items);
    }

    public static ValueTask<Item?> GetItemById(int id)
    {
        var item = Items.SingleOrDefault(i => i.Id == id);
        return new ValueTask<Item?>(item);
    }
}

public record Item(int Id, string Name, string Category, string Origin, int Quantity);