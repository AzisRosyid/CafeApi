using CafeApi.Models;
using CafeApi.Models.List;
using CafeApi.Models.Parameter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static CafeApi.Models.Enum.CoinEnum;
using static CafeApi.Models.Enum.UserAddressEnum;
using static CafeApi.Models.Enum.UserEnum;

namespace CafeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly ApiContext _context;

        public ProfilesController(ApiContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(UserList), 200)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> MyProfile()
        {
            var valid = Method.Decode(auth());
            if (!valid.IsValid) return Unauthorized(new { errors = "Access Unauthorized!" });

            var user = _context.Users.Where(s => s.Id == valid.Id).FirstOrDefault();
            double coins = 0;
            foreach (var i in _context.Transactions.Where(s => s.UserId == valid.Id))
            {
                foreach(var j in _context.Coins.Where(s => s.TransactionId == i.Id))
                {
                    if (j.Status == (int)CoinStatus.Increase) coins += j.Value;
                    if (j.Status == (int)CoinStatus.Decrease) coins -= j.Value;
                }
            }
            var userAddresses = _context.UserAddresses.Where(s => s.UserId == user.Id).Select(s => new UserAddressList
            {
                Id = s.Id,
                UserId = s.UserId,
                Address = s.Address,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                Status = (UserAddressStatus)s.Status,
            }).ToList();

            return Ok(new UserList
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Type = (UserType)user.Type,
                Role = (UserRole)user.Role,
                Gender = (UserGender)user.Gender,
                DateOfBirth = user.DateOfBirth,
                PhoneNumber = user.PhoneNumber,
                Address = userAddresses,
                Image = user.Image,
                Coins = coins,
                Status = (UserStatus)user.Status,
                DateCreated = user.DateCreated,
                DateUpdated = user.DateUpdated,
                DateDeleted = user.DateDeleted
            });
        }

        [HttpGet("Bookmark/{menuId}")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Bookmark(long menuId)
        {
            var valid = Method.Decode(auth());
            if (!valid.IsValid) return Unauthorized(new { errors = "Access Unauthorized!" }); 
            if (!_context.Menus.Any(s => s.Id == menuId)) { return NotFound(new { errors = "Menu Not Found!" }); }

            var bookmarks = _context.Bookmarks.Where(s => s.UserId == valid.Id && s.MenuId == menuId);

            if (bookmarks.Any())
            {
                var id = bookmarks.FirstOrDefault();
                id.DateUpdated = DateTime.Now;
                if (id.DateDeleted == null) id.DateDeleted = DateTime.Now;
                else id.DateDeleted = null;
                _context.Entry(id).State = EntityState.Modified;
                await _context.SaveChangesAsync();   
            }
            else
            {
                var st = new Bookmark();
                st.Id = bookmarkId();
                st.UserId = (long)valid.Id;
                st.MenuId = menuId;
                st.DateCreated = DateTime.Now;
                st.DateUpdated = DateTime.Now;
                _context.Bookmarks.Add(st);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpGet("Like/{menuId}")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Like(long menuId)
        {
            var valid = Method.Decode(auth());
            if (!valid.IsValid) return Unauthorized(new { errors = "Access Unauthorized!" });
            if (!_context.Menus.Any(s => s.Id == menuId)) { return NotFound(new { errors = "Menu Not Found!" }); }

            var likes = _context.Likes.Where(s => s.UserId == valid.Id && s.MenuId == menuId);

            if (likes.Any())
            {
                var id = likes.FirstOrDefault();
                id.DateUpdated = DateTime.Now;
                if (id.DateDeleted == null) id.DateDeleted = DateTime.Now;
                else id.DateDeleted = null;
                _context.Entry(id).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            else
            {
                var st = new Like();
                st.Id = likeId();
                st.UserId = (long)valid.Id;
                st.MenuId = menuId;
                st.DateCreated = DateTime.Now;
                st.DateUpdated = DateTime.Now;
                _context.Likes.Add(st);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpGet("Review/{transactionDetailId}")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Like(long transactionDetailId, [BindRequired][Required][RegularExpression(@"^\d+(\.\d{1,1})?$")][Range(1.0, 5.0)] decimal rating, string? description = null, bool delete = false)
        {
            var valid = Method.Decode(auth());
            if (!valid.IsValid) return Unauthorized(new { errors = "Access Unauthorized!" });
            if (!ModelState.IsValid) return BadRequest(Method.error(ModelState));
            if (!_context.TransactionDetails.Any(s => s.Id == transactionDetailId)) { return NotFound(new { errors = "Transaction Detail Id Not Found!" }); }

            var reviews = _context.Reviews.Where(s => s.UserId == valid.Id && s.TransactionDetailId == transactionDetailId);

            if (reviews.Any())
            {
                var id = reviews.FirstOrDefault();
                id.Rating = rating;
                id.Description = description;
                id.DateUpdated = DateTime.Now;
                if (delete) id.DateDeleted = DateTime.Now;
                else id.DateDeleted = null;
                _context.Entry(id).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            else
            {
                var st = new Review();
                st.Id = reviewId();
                st.UserId = (long)valid.Id;
                st.TransactionDetailId = transactionDetailId;
                st.Rating = rating;
                st.Description = description;
                st.DateCreated = DateTime.Now;
                st.DateUpdated = DateTime.Now;
                _context.Reviews.Add(st);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PutUser([FromForm] UserParameter userParameter)
        {
            var valid = Method.Decode(auth());
            if (!valid.IsValid) return StatusCode(401, new { errors = "Access Unauthorized!" });
            if (!ModelState.IsValid) return BadRequest(Method.error(ModelState));
            if (userParameter.Addresses != null && userParameter.Addresses.Status != null) foreach (var i in userParameter.Addresses.Status) foreach (var j in userParameter.Addresses.Status) if (i == j) return Conflict(new { errors = "Addresses Status cannot same each other!" });

            var st = _context.Users.Where(s => s.Id == valid.Id).FirstOrDefault();

            if (_context.Users.Any(s => s.Email == userParameter.Email && (UserType)s.Type == userParameter.Type && s.Email != st.Email)) return Conflict(new { errors = "Email already exist!" });

            string? img = null;
            if (userParameter.Image != null)
            {
                var path = Path.GetExtension(userParameter.Image.FileName);
                if (!(path == ".jpg" || path == ".png" || path == ".jpeg")) return StatusCode(400, new { errors = "Image format must be .png, .jpg, or .jpeg" });
                if (!Directory.Exists(Method.profilePath)) Directory.CreateDirectory(Method.profilePath);
                img = $"Profile_{DateTime.Now.ToString("yyyy_MM_dd_HHmmss")}_{userParameter.Email}{path}";
                using (var stream = new FileStream(Path.Combine(Method.profilePath, img), FileMode.Create))
                {
                    if (st.Image != null) new FileInfo(Path.Combine(Method.profilePath, img)).Delete();
                    userParameter.Image.CopyTo(stream);
                }
            }

            st.Email = userParameter.Email;
            st.Name = userParameter.Name;
            st.Password = Method.Encrypt(userParameter.Password);
            st.Type = (int)userParameter.Type;
            st.Role = (int)userParameter.Role;
            st.Gender = (int)userParameter.Gender;
            st.DateOfBirth = userParameter.DateOfBirth;
            st.PhoneNumber = userParameter.PhoneNumber;
            st.Image = img;
            st.Status = (int)userParameter.Status;
            st.DateUpdated = DateTime.Now;
            _context.Entry(st).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            foreach (var i in _context.UserAddresses.Where(s => s.UserId == st.Id)) _context.UserAddresses.Remove(i); await _context.SaveChangesAsync();
            if (userParameter.Addresses != null && (userParameter.Addresses.Address != null && userParameter.Addresses.Latitude != null && userParameter.Addresses.Longitude != null && userParameter.Addresses.Status != null))
            {
                var addresses = userParameter.Addresses.Address.Zip(userParameter.Addresses.Latitude, (ad, la) => new { ad, la }).Zip(userParameter.Addresses.Longitude, (x, lo) => new { x.ad, x.la, lo }).Zip(userParameter.Addresses.Status, (x, s) => new { Address = x.ad, Latitude = x.la, Longitude = x.lo, Status = s }).ToList();
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

            return Ok(new { messages = "User successfully Updated!" });
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteUser()
        {
            var valid = Method.Decode(auth());
            if (!valid.IsValid) return StatusCode(401, new { errors = "Access Unauthorized!" });

            var user = await _context.Users.FindAsync(valid.Id);
            user.DateDeleted = DateTime.Now;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { messages = "User successfully Deleted!" });
        }

        private string auth()
        {
            Request.Headers.TryGetValue("Authorization", out var auth);
            try { return auth.ToString().Replace("Bearer ", ""); } catch { return auth; }
        }
        private long addressId()
        {
            var st = _context.UserAddresses.OrderByDescending(s => s.Id).Select(s => s.Id);
            if (st.Any()) return st.FirstOrDefault() + 1;
            else return 1;
        }
        private long bookmarkId()
        {
            var st = _context.Bookmarks.OrderByDescending(s => s.Id).Select(s => s.Id);
            if (st.Any()) return st.FirstOrDefault() + 1;
            else return 1;
        }
        private long likeId()
        {
            var st = _context.Likes.OrderByDescending(s => s.Id).Select(s => s.Id);
            if (st.Any()) return st.FirstOrDefault() + 1;
            else return 1;
        }
        private long reviewId()
        {
            var st = _context.Reviews.OrderByDescending(s => s.Id).Select(s => s.Id);
            if (st.Any()) return st.FirstOrDefault() + 1;
            else return 1;
        }
    }
}
