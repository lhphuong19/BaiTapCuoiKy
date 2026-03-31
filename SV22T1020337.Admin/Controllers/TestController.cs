using Microsoft.AspNetCore.Mvc;
using SV22T1020337.DataLayers.SQLServer;
using SV22T1020337.Models.Common;

namespace SV22T1020337.Admin.Controllers
{
    public class TestController : Controller
    {
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string searchValue = "")
        {
            var input = new PaginationSearchInput()
            {
                Page = page,
                PageSize = pageSize,
                SearchValue = searchValue
            };
            string connectionString = "Server=.;Database=LiteCommerceDB;Trusted_Connection=True;TrustServerCertificate=True";
            var repository = new SupplierRepository(connectionString);
            var result = await repository.ListEmployeesAsync(input);
            return Json(result);
        }
    }
}
