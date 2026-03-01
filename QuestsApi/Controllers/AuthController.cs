using Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QuestsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(string Username, string Password)
        {
            var res = await _authService.CreateUserAsync(Username, Password);

            return Ok(new { message = res });
        }

        [HttpPost("Authorize")]
        public async Task<IActionResult> Authorize(string Username, string Password)
        {
            var res = await _authService.CreateUserAsync(Username, Password);

            return Ok(new { message = res });
        }
    }
}
