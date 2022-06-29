using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sorting_Filtering_Paging.Models;

namespace Sorting_Filtering_Paging.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ListBaseController
    {
        private readonly AdventureWorksContext _context;

        public OrdersController(AdventureWorksContext context, IHttpContextAccessor httpContextAccessor): base(httpContextAccessor)
        {
            _context = context;
        }

        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomerList(
                [FromQuery]string Title_like, 
                [FromQuery]bool FirstName_sort, 
                [FromQuery]int page) 
        {
            int pageSize = 10;
            var baseQuery = _context.Customers.AsQueryable();

            var filter = this.GetFilter();

            var deleg = ExpressionBuilder.GetExpression<Customer>(filter).Compile();
            var filteredResultSet = baseQuery.Where(deleg);
            var total = filteredResultSet.Count();

//            var orderedResultset = FirstName_sort 
//                ? filteredResultSet.OrderByProperty("FirstName") 
//                : filteredResultSet.OrderByPropertyDescending("FirstName");
            var pagedResultSet = filteredResultSet
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

            this.SetTotalCountHeader(total);
            return Ok(pagedResultSet);
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
