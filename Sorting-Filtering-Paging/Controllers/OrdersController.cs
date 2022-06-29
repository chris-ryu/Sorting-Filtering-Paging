using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sorting_Filtering_Paging.Models;

namespace Sorting_Filtering_Paging.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AdventureWorksContext _context;

        public OrdersController(AdventureWorksContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery]string searchKey, [FromQuery]int page)
        {
            int pageSize = 10;
            var baseQuery = _context.SalesOrderHeaders
                .Where(x => x.Customer.Title.Contains(searchKey));

            var totalCount = baseQuery.Count();

            var paged = await baseQuery.Skip((page-1) * pageSize)
                .Take(pageSize)
                .Select(x => new OrderItemDTO { 
                    Id = x.SalesOrderId,
                    CustomerName = x.Customer.FirstName,
                    TotalDue = x.TotalDue
                })
                .ToListAsync();

            return Ok(new {List = paged, TotalCount = totalCount});
        }
    }
}
