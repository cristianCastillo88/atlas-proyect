using BackendAtlas.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackendAtlas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _authService.Login(request.Email, request.Password, cancellationToken);
            if (result == null)
            {
                return Unauthorized("Invalid credentials");
            }
            return Ok(result);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}