using Microsoft.AspNetCore.Mvc;

namespace FCG_Payments.Api.Controllers
{
    [ApiController]
    [Route("payments")]
    public class PaymentController : ControllerBase
    {
        [HttpGet("health")]
        public IActionResult Get()
        {
            return Ok("Payments API is healthy");
        }
    }
}
