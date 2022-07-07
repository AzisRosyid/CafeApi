using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeApi.Models;
using CafeApi.Models.Lists;
using static CafeApi.Models.Enums.UserEnum;
using CafeApi.Models.Parameters;
using static CafeApi.Models.Enums.OrderEnum;
using static CafeApi.Models.Enums.DeleteEnum;
using static CafeApi.Models.Enums.CategoryEnum;
using CafeApi.Controllers.Helpers;

namespace CafeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApiContext _context;

        public CategoriesController(ApiContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        [ProducesResponseType(typeof(CategoryList), 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories(int? page = 1, int? pick = 20, string search = "", long? parent = null, Delete? deleted = null, CategorySort? sort = null, Order? order = null, DateTime? createStart = null, DateTime? createEnd = null, DateTime? updateStart = null, DateTime? updateEnd = null, bool android = false)
        {
            var valid = Method.valid(auth());

            var st = _context.Categories.Where(s => s.Id.ToString().Contains(search) || s.ParentId.ToString().Contains(search) || s.Name.Contains(search) || s.DateCreated.ToString().Contains(search) || s.DateUpdated.ToString().Contains(search) || s.DateDeleted.ToString().Contains(search)).Select(s => new CategoryList
            {
                Id = s.Id,
                ParentId = s.ParentId,
                Name = s.Name,
                DateCreated = s.DateCreated,
                DateUpdated = s.DateUpdated,
                DateDeleted = s.DateDeleted
            }).ToList();

            if (deleted == Delete.Deleted) st = st.Where(s => s.DateDeleted != null).ToList();
            else if (deleted == Delete.NotDeleted || valid.Role != UserRole.Admin) st = st.Where(s => s.DateDeleted == null).ToList();

            if (createStart.HasValue) st = st.Where(s => s.DateCreated >= createStart).ToList();
            if (createEnd.HasValue) st = st.Where(s => s.DateCreated <= createEnd).ToList();
            if (updateStart.HasValue) st = st.Where(s => s.DateUpdated >= updateStart).ToList();
            if (updateEnd.HasValue) st = st.Where(s => s.DateUpdated <= updateEnd).ToList();

            if (sort == CategorySort.Id) st = st.OrderBy(s => s.Id).ToList();
            else if (sort == CategorySort.ParentId) st = st.OrderBy(s => s.ParentId).ToList();
            else if (sort == CategorySort.Name) st = st.OrderBy(s => s.Name).ToList();
            else if (sort == CategorySort.DateCreated) st = st.OrderBy(s => s.DateCreated).AsEnumerable().Reverse().ToList();
            else if (sort == CategorySort.DateUpdated) st = st.OrderBy(s => s.DateUpdated).AsEnumerable().Reverse().ToList();
            else if (sort == CategorySort.DateDeleted) st = st.OrderBy(s => s.DateDeleted).AsEnumerable().Reverse().ToList();
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
                if (android) return Ok(new { TotalCategories = st.Count(), TotalPages = totalPage, Categories = st.Take((int)pick * (int)page).ToList() });
                return Ok(new { TotalCategories = st.Count(), TotalPages = totalPage, Categories = st.Skip(((int)pick * (int)page) - (int)pick).Take((int)pick).ToList() });
            }
            else if (pick > 0) return Ok(new { TotalCategories = st.Count(), TotalPages = totalPage, Categories = st.Take((int)pick).ToList() });
            else return Ok(new { TotalCategories = st.Count(), TotalPages = 1, Categories = st.ToList() });
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CategoryList), 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Category>> GetCategory(long id)
        {
            var valid = Method.valid(auth());
            if (!CategoryExists(id)) return NotFound(new { errors = "Category Not Found!" });
            var category = await _context.Categories.FindAsync(id);

            if (category.DateDeleted != null && valid.Role != UserRole.Admin) return StatusCode(403, new { errors = "User Role must be Admin!" });

            var result = new
            {
                Category = new CategoryList
                {
                    Id = category.Id,
                    ParentId = category.ParentId,
                    Name = category.Name,
                    DateCreated = category.DateCreated,
                    DateUpdated = category.DateUpdated,
                    DateDeleted = category.DateDeleted
                }
            };

            return Ok(result);
        }

        // PUT: api/Categories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutCategory(long id, CategoryParameter categoryParameter)
        {
            var valid = Method.valid(auth());
            if (!valid.IsValid) return Unauthorized(new { errors = "Access Unauthorized!" });
            if (valid.Role != UserRole.Admin) return StatusCode(403, new { errors = "User Role must be Admin!" });
            if (!ModelState.IsValid) return BadRequest(Method.error(ModelState));
            if (!CategoryExists(id)) return NotFound(new { errors = "Category Not Found!" });

            var st = _context.Categories.Where(s => s.Id == id).FirstOrDefault();
            st.Name = categoryParameter.Name;
            st.ParentId = categoryParameter.ParentId;
            st.Name = categoryParameter.Name;
            st.DateUpdated = DateTime.Now;
            _context.Entry(st).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { messages = "Category successfully Updated!" });
        }

        // POST: api/Categories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Category>> PostCategory(CategoryParameter categoryParameter)
        {
            var valid = Method.valid(auth());
            if (!valid.IsValid) return Unauthorized(new { errors = "Access Unauthorized!" }); 
            if (valid.Role != UserRole.Admin) return StatusCode(403, new { errors = "User Role must be Admin!" }); 
            if (!ModelState.IsValid) return BadRequest(Method.error(ModelState));

            var st = new Category();
            st.Id = categoryId();
            st.ParentId = categoryParameter.ParentId;
            st.Name = categoryParameter.Name;
            st.DateCreated = DateTime.Now;
            st.DateUpdated = DateTime.Now;
            _context.Categories.Add(st);

            return Created("Category", new { messages = "Category successfully Created!" });
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(long id)
        {
            var valid = Method.valid(auth());
            if (!valid.IsValid) return StatusCode(401, new { errors = "Access Unauthorized!" });
            if (valid.Role != UserRole.Admin) return StatusCode(403, new { errors = "User Role must be Admin!" });
            if (!CategoryExists(id)) NotFound(new { errors = "Category Not Found!" });

            var category = await _context.Users.FindAsync(id);
            category.DateUpdated = DateTime.Now;
            category.DateDeleted = DateTime.Now;
            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { messages = "Category successfully Deleted!" });
        }

        private bool CategoryExists(long id) => (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();

        private string auth()
        {
            Request.Headers.TryGetValue("Authorization", out var auth);
            try { return auth.ToString().Replace("Bearer ", ""); } catch { return auth; }
        }

        private long categoryId()
        {
            var st = _context.Categories.OrderByDescending(s => s.Id).Select(s => s.Id);
            if (st.Any()) return st.FirstOrDefault() + 1;
            else return 1;
        }
    }
}
