using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    // Version 2 Controller
    [Route("api/v{version:apiVersion}/products")]
    [ApiController]
    [ApiVersion("2.0")]
    public class ProductsV2Controller : ControllerBase
    {
        // Version 2 - User และ Admin
        [HttpGet]
        [Authorize(Policy = "UserOnly")]
        public IActionResult GetV2()
        {
            return Ok(new { Version = "v2", Data = "Updated Product List (User & Admin)" });
        }
    }
}
