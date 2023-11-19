using Bogus;

namespace DemoAddressApi
{
    public static class DataService
    {
        private static List<Address> Addresses { get; set; }

        static DataService()
        {
            Addresses = new Faker<Address>()
                .CustomInstantiator(f =>
                    new Address(f.IndexVariable++,
                        f.Address.StreetAddress(),
                        f.Address.City(),
                        f.Address.Country()))
                .Generate(50);
        }

        public static ValueTask<IEnumerable<Address>> GetAddresses()
        {
            return new ValueTask<IEnumerable<Address>>(Addresses);
        }

        public static ValueTask<Address?> GetAddressById(int id)
        {
            var item = Addresses.SingleOrDefault(i => i.Id == id);
            return new ValueTask<Address?>(item);
        }
    }

    public record Address(int Id,string Street,string City,  string Country);
}
