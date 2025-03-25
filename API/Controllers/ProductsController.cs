using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/v{version:apiVersion}/products")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ProductsController : ControllerBase
    {
        // Version 1 - ทุกคนเข้าถึงได้
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get()
        {
            return Ok(new { Version = "v1", Data = "Product List (Public)" });
        }

        // Version 1 - เฉพาะ Admin เท่านั้น
        [HttpGet("admin")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult GetForAdmin()
        {
            return Ok(new { Version = "v1", Data = "Product List (Admin only)" });
        }
    }
}
