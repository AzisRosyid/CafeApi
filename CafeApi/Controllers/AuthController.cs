using CafeApi.Controllers.Helpers;
using CafeApi.Models;
using CafeApi.Models.Parameters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static CafeApi.Models.Enums.UserAddressEnum;
using static CafeApi.Models.Enums.UserEnum;

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
                return Ok(new { Token = Method.token(id, (bool)loginParameter.Android) });
            }
            return StatusCode(401, new { errors = "Email and Password does not correct!" });
        }

        [HttpGet("RefreshToken")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken()
        {
            var valid = Method.valid(auth());
            if (!valid.IsValid) { return Unauthorized(new { errors = "Access Unautorized!" }); }
            var st = _context.Users.Where(s => s.Id == valid.Id).FirstOrDefault();
            return Ok(new { Token = Method.token(st, false) });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<User>> Register([FromForm] RegisterParameter registerParameter)
        {
            if (!ModelState.IsValid) return BadRequest(Method.error(ModelState));
            if (_context.Users.Any(s => s.Email == registerParameter.Email && (UserType)s.Type == registerParameter.Type)) return Conflict(new { errors = "Email already exist!" });
            if (registerParameter.Addresses != null && registerParameter.Addresses.Status != null) foreach (var i in registerParameter.Addresses.Status) foreach (var j in registerParameter.Addresses.Status) if (i == j) return Conflict(new { errors = "Addresses Status cannot same each other!" });

            string? img = null;
            if (registerParameter.Image != null)
            {
                var path = Path.GetExtension(registerParameter.Image.FileName);
                if (!(path == ".jpg" || path == ".png" || path == ".jpeg")) return StatusCode(400, new { errors = "Image format must be .png, .jpg, or .jpeg" });
                if (!Directory.Exists(Method.profilePath)) Directory.CreateDirectory(Method.profilePath);
                img = $"Profile_{DateTime.Now.ToString("yyyy_MM_dd_HHmmss")}_{registerParameter.Email}{path}";
                using (var stream = new FileStream(Path.Combine(Method.profilePath, img), FileMode.Create)) registerParameter.Image.CopyTo(stream);
            }

            var st = new User();
            st.Id = userId();
            st.Email = registerParameter.Email;
            st.Name = registerParameter.Name;
            st.Password = Method.Encrypt(registerParameter.Password);
            st.Type = (int)registerParameter.Type;
            st.Role = (int)UserRole.Customer;
            st.Gender = (int)registerParameter.Gender;
            st.DateOfBirth = registerParameter.DateOfBirth;
            st.PhoneNumber = registerParameter.PhoneNumber;
            st.Image = img;
            st.Status = (int)registerParameter.Status;
            st.DateCreated = DateTime.Now;
            st.DateUpdated = DateTime.Now;
            _context.Users.Add(st);
            await _context.SaveChangesAsync();

            if (registerParameter.Addresses != null && (registerParameter.Addresses.Address != null && registerParameter.Addresses.Latitude != null && registerParameter.Addresses.Longitude != null && registerParameter.Addresses.Status != null))
            {
                var addresses = registerParameter.Addresses.Address.Zip(registerParameter.Addresses.Latitude, (ad, la) => new { ad, la }).Zip(registerParameter.Addresses.Longitude, (x, lo) => new { x.ad, x.la, lo }).Zip(registerParameter.Addresses.Status, (x, s) => new { Address = x.ad, Latitude = x.la, Longitude = x.lo, Status = s }).ToList();
                foreach (var i in addresses)
                {
                    var ad = new UserAddress();
                    ad.Id = addressId();
                    ad.UserId = st.Id;
                    ad.Address = i.Address;
                    ad.Latitude = (double)i.Latitude;
                    ad.Longitude = (double)i.Longitude;
                    ad.Status = (int)i.Status;
                    if (addresses.Count <= 1) ad.Status = (int)UserAddressStatus.Primary;
                    _context.UserAddresses.Add(ad);
                    await _context.SaveChangesAsync();
                }
            }

            return Created("User", new { messages = "User successfully Created!" });
        }

        private string auth()
        {
            Request.Headers.TryGetValue("Authorization", out var auth);
            try { return auth.ToString().Replace("Bearer ", ""); } catch { return auth; }
        }

        private long userId()
        {
            var st = _context.Users.OrderByDescending(s => s.Id).Select(s => s.Id);
            if (st.Any()) return st.FirstOrDefault() + 1;
            else return 1;
        }

        private long addressId()
        {
            var st = _context.UserAddresses.OrderByDescending(s => s.Id).Select(s => s.Id);
            if (st.Any()) return st.FirstOrDefault() + 1;
            else return 1;
        }
    }
}
