using CafeApi.Models;
using CafeApi.Models.Parameter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CafeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApiContext _context;

        public AuthController(ApiContext context)
        {
            _context = context;
        }

        [HttpPost("Login")]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> Login(LoginParameter loginParameter)
        {
            if (!ModelState.IsValid) { return StatusCode(400, Method.error(ModelState)); }
            var st = _context.Users.Where(s => s.Email == loginParameter.Email);
            if (st.Count() < 1) { return StatusCode(404, new { errors = "Email does Not Found!" }); }
            if (st.Count() > 0 && st.Where(s => s.Password == Method.Encrypt(loginParameter.Password)).Count() > 0)
            {
                var id = st.FirstOrDefault();
                return Ok(new { Token = Method.Encode(id, (bool)loginParameter.Android) });
            }
            return StatusCode(401, new { errors = "Email and Password does not correct!" });
        }

        [HttpGet("RefreshToken")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken()
        {
            if (!Method.Decode(auth()).IsValid) { return Unauthorized(new { errors = "Access Unautorized!" }); }
            var st = _context.Users.Where(s => s.Id == Method.Decode(auth()).Id);
            var email = st.FirstOrDefault();
            return Ok(new { Token = Method.Encode(email, false) });
        }

        private string auth()
        {
            Request.Headers.TryGetValue("Authorization", out var auth);
            try { return auth.ToString().Replace("Bearer ", ""); } catch { return auth; }
        }
    }
}
