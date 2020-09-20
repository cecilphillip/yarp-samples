using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ItemsApi.Controllers
{
    [Route("/api/items")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetItems()
        {
            var result=  await DataService.GetItems();
            if (result == null || !result.Any()) return NotFound();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItemByID(int id)
        {
            var item = await DataService.GetItemByID(id);
            if (item == null) return NotFound();
            return item;
        }
    }
}
