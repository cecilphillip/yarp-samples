using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DemoAddressApi.Controllers
{
    [Route("/api/addresses")]
    [ApiController]
    public class AddressesController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Address>>> GetItems()
        {
            var result=  await DataService.GetAdresses();
            if (result == null || !result.Any()) return NotFound();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Address>> GetItemByID(int id)
        {
            var item = await DataService.GetAddressByID(id);
            if (item == null) return NotFound();
            return item;
        }
    }
}