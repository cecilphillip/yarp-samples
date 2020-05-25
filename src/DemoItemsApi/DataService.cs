using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.AspNetCore.Mvc;

namespace DemoItemsApi
{
    public static class DataService
    {
        private static List<Item> Items { get; set; }

        static DataService()
        {
            Items = new Faker<Item>()
                .RuleFor(i => i.ID, f => f.IndexVariable++)
                .RuleFor(i => i.Name, f => f.Commerce.ProductName())
                .RuleFor(i => i.Category, f => f.PickRandom(f.Commerce.Categories(5)))
                .RuleFor(i => i.Origin, f => f.Address.Country())
                .RuleFor(i => i.Quantity, f => f.Random.Number(10, 100))
                .Generate(50);
        }

        public static ValueTask<IEnumerable<Item>> GetItems()
        {
            return new ValueTask<IEnumerable<Item>>(Items);
        }

        public static ValueTask<Item> GetItemByID(int id)
        {
            var item = Items.SingleOrDefault(i => i.ID == id);
            return item != null ? new ValueTask<Item>(item) : new ValueTask<Item>();
        }
    }
}