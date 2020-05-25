using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;

namespace DemoAddressApi
{
    public static class DataService
    {
        private static List<Address> Addresses { get; set; }

        static DataService()
        {
            Addresses = new Faker<Address>()
                .RuleFor(i => i.ID, f => f.IndexVariable++)
                .RuleFor(i => i.Street, f => f.Address.StreetAddress())
                .RuleFor(i => i.City, f => f.Address.City())
                .RuleFor(i => i.Country, f => f.Address.Country())
                
                .Generate(50);
        }

        public static ValueTask<IEnumerable<Address>> GetAdresses()
        {
            return new ValueTask<IEnumerable<Address>>(Addresses);
        }

        public static ValueTask<Address> GetAddressByID(int id)
        {
            var item = Addresses.SingleOrDefault(i => i.ID == id);
            return item != null ? new ValueTask<Address>(item) : new ValueTask<Address>();
        }
    }
}