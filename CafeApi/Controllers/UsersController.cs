using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeApi.Models;
using CafeApi.Models.List;
using static CafeApi.Models.Enum.UserEnum;
using static CafeApi.Models.Enum.OrderEnum;
using static CafeApi.Models.Enum.UserAddressEnum;
using CafeApi.Models.Parameter;
using static CafeApi.Models.Enum.CoinEnum;
using static CafeApi.Models.Enum.DeleteEnum;

namespace CafeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApiContext _context;

        public UsersController(ApiContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        [ProducesResponseType(typeof(UserList), 200)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(int? page = 1, int? pick = 20, string search = "", UserRole? role = null, UserGender? gender = null, UserType? type = null, UserStatus? status = null, Delete? deleted = null, UserSort? sort = null, Order? order = null, DateTime? createStart = null, DateTime? createEnd = null, DateTime? updateStart = null, DateTime? updateEnd = null, bool android = false)
        {
            var valid = Method.Decode(auth());
            if (!valid.IsValid) return StatusCode(401, new { errors = "Access Unauthorized!" }); 
            if (valid.Role != UserRole.Admin) return StatusCode(403, new { errors = "User Role must be Admin!" }); 

            int? roleSearch = null; int? genderSearch = null; int? statusSearch = null; int? deletedSearch = null;
            try { roleSearch = (int)(UserRole)Enum.Parse(typeof(UserRole), search, true); } catch { }
            try { genderSearch = (int)(UserGender)Enum.Parse(typeof(UserGender), search, true); } catch { }
            try { statusSearch = (int)(UserStatus)Enum.Parse(typeof(UserStatus), search, true); } catch { }
            try { deletedSearch = (int)(Delete)Enum.Parse(typeof(Delete), search, true); } catch { }

            var st = new List<UserList>(); var userIds = new List<long>();
            var users = _context.Users.Where(s => s.Id.ToString().Contains(search) || s.Email.Contains(search) || s.Name.Contains(search) || s.DateOfBirth.ToString().Contains(search) || s.PhoneNumber.Contains(search) || s.Image.Contains(search) || s.DateCreated.ToString().Contains(search) || s.DateUpdated.ToString().Contains(search) || s.DateDeleted.ToString().Contains(search) || s.Role == roleSearch || s.Gender == genderSearch || s.Status == statusSearch).Select(s => s.Id);
            var addresses = _context.UserAddresses.Where(s => s.Id.ToString().Contains(search) || s.UserId.ToString().Contains(search) || s.Address.Contains(search) || s.Latitude.ToString().Contains(search) || s.Longitude.ToString().Contains(search)).Select(s => s.UserId);

            userIds.AddRange(users); userIds.AddRange(addresses);
            var userIdCom = new List<long>(); userIdCom.AddRange(userIds); userIds.Clear();
            foreach(var i in userIdCom) if (!userIds.Any(s => s == i)) userIds.Add(i);

            foreach (var i in userIds)
            {
                var user = _context.Users.Where(s => s.Id == i).FirstOrDefault();
                double coins = 0;
                foreach (var j in _context.Transactions.Where(s => s.UserId == valid.Id))
                {
                    foreach (var k in _context.Coins.Where(s => s.TransactionId == j.Id))
                    {
                        if (k.Status == (int)CoinStatus.Increase) coins += k.Value;
                        if (k.Status == (int)CoinStatus.Decrease) coins -= k.Value;
                    }
                }
                var userAddresses = _context.UserAddresses.Where(s => s.UserId == i).Select(s => new UserAddressList
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    Address = s.Address,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    Status = (UserAddressStatus)s.Status
                }).ToList();

                st.Add(new UserList
                {
                    Id = i,
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

            if (role.HasValue) st = st.Where(s => s.Role == role).ToList();
            if (gender.HasValue) st = st.Where(s => s.Gender == gender).ToList();
            if (status.HasValue) st = st.Where(s => s.Status == status).ToList();
            if (type.HasValue) st = st.Where(s => s.Type == type).ToList();

            if (deleted == Delete.Deleted) st = st.Where(s => s.DateDeleted != null).ToList();
            else if (deleted == Delete.NotDeleted) st = st.Where(s => s.DateDeleted == null).ToList();

            if (createStart.HasValue) st = st.Where(s => s.DateCreated >= createStart).ToList();
            if (createEnd.HasValue) st = st.Where(s => s.DateCreated <= createEnd).ToList();
            if (updateStart.HasValue) st = st.Where(s => s.DateUpdated >= updateStart).ToList();
            if (updateEnd.HasValue) st = st.Where(s => s.DateUpdated <= updateEnd).ToList();

            if (sort == UserSort.Id) st = st.OrderBy(s => s.Id).ToList();
            else if (sort == UserSort.Name) st = st.OrderBy(s => s.Name).ToList();
            else if (sort == UserSort.Email) st = st.OrderBy(s => s.Email).ToList();
            else if (sort == UserSort.Type) st = st.OrderBy(s => s.Type).ToList();
            else if (sort == UserSort.Role) st = st.OrderBy(s => s.Role).ToList();
            else if (sort == UserSort.Gender) st = st.OrderBy(s => s.Gender).AsEnumerable().Reverse().ToList();
            else if (sort == UserSort.DateOfBirth) st = st.OrderBy(s => s.DateOfBirth).AsEnumerable().Reverse().ToList();
            else if (sort == UserSort.PhoneNumber) st = st.OrderBy(s => s.PhoneNumber).ToList();
            else if (sort == UserSort.Address) st = st.OrderBy(s => _context.UserAddresses.Where(s => s.UserId == s.Id).Select(s => s.Address).FirstOrDefault()).ToList();
            else if (sort == UserSort.Status) st = st.OrderBy(s => s.Status).ToList();
            else if (sort == UserSort.DateCreated) st = st.OrderBy(s => s.DateCreated).AsEnumerable().Reverse().ToList();
            else if (sort == UserSort.DateUpdated) st = st.OrderBy(s => s.DateUpdated).AsEnumerable().Reverse().ToList();
            else if (sort == UserSort.DateDeleted) st = st.OrderBy(s => s.DateDeleted).AsEnumerable().Reverse().ToList();
            else st = st.OrderBy(s => s.Id).AsEnumerable().Reverse().ToList();

            if (order == Order.Descending) st = st.AsEnumerable().Reverse().ToList();

            if (!st.Any()) return NotFound(new { errors = "Search Not Found!" });

            int totalPage = 0;

            if (pick > 0)
            {
                totalPage = st.Count() / (int)pick;
                if (st.Count % (int)pick != 0) totalPage++;
            }

            if (page > 0 && pick > 0)
            {
                if (android) return Ok(new { TotalUsers = st.Count(), TotalPages = totalPage, Users = st.Take((int)pick * (int)page).ToList() });
                return Ok(new { TotalUsers = st.Count(), TotalPages = totalPage, Users = st.Skip(((int)pick * (int)page) - (int)pick).Take((int)pick).ToList() });
            }
            else if (pick > 0) return Ok(new { TotalUsers = st.Count(), TotalPages = totalPage, Users = st.Take((int)pick).ToList() });
            else return Ok(new { TotalUsers = st.Count(), TotalPages = 1, Users = st.ToList() });
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserList), 200)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<User>> GetUser(long id)
        {
            var valid = Method.Decode(auth());
            if (!valid.IsValid) return StatusCode(401, new { errors = "Access Unauthorized!" }); 
            if (valid.Role != UserRole.Admin) return StatusCode(403, new { errors = "User Role must be Admin!" });
            if (!UserExists(id)) return StatusCode(404, new { errors = "User Not Found!" });

            var user = await _context.Users.FindAsync(id);
            double coins = 0;
            foreach (var i in _context.Transactions.Where(s => s.UserId == valid.Id))
            {
                foreach (var j in _context.Coins.Where(s => s.TransactionId == i.Id))
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

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PutUser(long id, [FromForm] UserParameter userParameter)
        {
            var valid = Method.Decode(auth());
            if (!valid.IsValid) return StatusCode(401, new { errors = "Access Unauthorized!" });
            if (valid.Role != UserRole.Admin) return StatusCode(403, new { errors = "User Role must be Admin!" });
            if (!ModelState.IsValid) return BadRequest(Method.error(ModelState));
            if (userParameter.Addresses != null && userParameter.Addresses.Status != null) foreach (var i in userParameter.Addresses.Status) foreach (var j in userParameter.Addresses.Status) if (i == j) return Conflict(new { errors = "Addresses Status cannot same each other!" });

            var st = _context.Users.Where(s => s.Id == id).FirstOrDefault();

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

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<User>> PostUser([FromForm] UserParameter userParameter)
        {
            

            var valid = Method.Decode(auth());
            if (!valid.IsValid) return StatusCode(401, new { errors = "Access Unauthorized!" }); 
            if (valid.Role != UserRole.Admin) return StatusCode(403, new { errors = "User Role must be Admin!" }); 
            if (!ModelState.IsValid) return BadRequest(Method.error(ModelState)); 
            if (_context.Users.Any(s => s.Email == userParameter.Email && (UserType)s.Type == userParameter.Type)) return Conflict(new { errors = "Email already exist!" });
            if (userParameter.Addresses != null && userParameter.Addresses.Status != null) foreach (var i in userParameter.Addresses.Status) foreach (var j in userParameter.Addresses.Status) if (i == j) return Conflict(new { errors = "Addresses Status cannot same each other!" });

            string? img = null;
            if (userParameter.Image != null)
            {
                var path = Path.GetExtension(userParameter.Image.FileName);
                if (!(path == ".jpg" || path == ".png" || path == ".jpeg")) return StatusCode(400, new { errors = "Image format must be .png, .jpg, or .jpeg" });
                if (!Directory.Exists(Method.profilePath)) Directory.CreateDirectory(Method.profilePath);
                img = $"Profile_{DateTime.Now.ToString("yyyy_MM_dd_HHmmss")}_{userParameter.Email}{path}";
                using (var stream = new FileStream(Path.Combine(Method.profilePath, img), FileMode.Create)) userParameter.Image.CopyTo(stream);
            }

            var st = new User();
            st.Id = userId();
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
            st.DateCreated = DateTime.Now;
            st.DateUpdated = DateTime.Now;
            _context.Users.Add(st);
            await _context.SaveChangesAsync();

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

            return Created("User", new { messages = "User successfully Created!" });
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var valid = Method.Decode(auth());
            if (!valid.IsValid) return StatusCode(401, new { errors = "Access Unauthorized!" }); 
            if (valid.Role != UserRole.Admin) return StatusCode(403, new { errors = "User Role must be Admin!" }); 
            if (!UserExists(id)) NotFound(new { errors = "User Not Found!" });
            
            var user = await _context.Users.FindAsync(id);
            user.DateUpdated = DateTime.Now;
            user.DateDeleted = DateTime.Now;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { messages = "User successfully Deleted!" });
        }

        private bool UserExists(long id) => (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();

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
