using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeApi.Models;
using static CafeApi.Models.Enum.UserEnum;
using CafeApi.Models.Parameter;
using static CafeApi.Models.Enum.MenuContentEnum;
using CafeApi.Models.List;
using static CafeApi.Models.Enum.MenuEnum;
using static CafeApi.Models.Enum.DeleteEnum;
using static CafeApi.Models.Enum.OrderEnum;

namespace CafeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenusController : ControllerBase
    {
        private readonly ApiContext _context;

        public MenusController(ApiContext context)
        {
            _context = context;
        }

        // GET: api/Menus
        [HttpGet]
        [ProducesResponseType(typeof(MenuList), 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Menu>>> GetMenus(int? page = 1, int? pick = 20, string search = "", long? categoryId = null, double? minPrice = null, double? maxPrice = null, decimal? minRating = null, decimal? maxRating = null, MenuType? type = null, bool soldOut = false, Delete? deleted = null, MenuSort? sort = null, Order? order = null, DateTime? createStart = null, DateTime? createEnd = null, DateTime? updateStart = null, DateTime? updateEnd = null, bool android = false)
        {
            var valid = Method.Decode(auth());

            var st = new List<MenuList>();
            var menuIds = new List<long>();
            var searchList = new List<string>();

            searchList.Add(search);
            searchList.AddRange(search.Replace("&&", "").Replace(" ", "").Split("||").ToList());

            void comMenu()
            {
                var menuIdCom = new List<long>(); menuIdCom.AddRange(menuIds); menuIds.Clear();
                foreach (var i in menuIdCom) if (!menuIds.Any(s => s == i)) menuIdCom.Add(i);
            }

            void getMenu(string search)
            {
                var st1 = from s in _context.Menus
                          join t in _context.Categories on s.CategoryId equals t.Id
                          where t.Id.ToString().Contains(search) || t.ParentId.ToString().Contains(search) || t.Name.Contains(search)
                          select s.Id;
                var st2 = _context.Menus.Where(s => _context.TransactionDetails.Where(s => s.MenuId == s.Id).Sum(s => s.Quantity).ToString().Contains(search)).Select(s => s.Id);
                var st3 = from s in _context.Menus
                          join t in _context.MenuContents on s.Id equals t.MenuId
                          where t.Id.ToString().Contains(search) || ((MenuContentType)t.Type).ToString().Contains(search) || t.TypeOrder.ToString().Contains(search) || t.Value.Contains(search)
                          select s.Id;
                var st4 = _context.Menus.Where(s => s.Id.ToString().Contains(search) || s.CategoryId.ToString().Contains(search) || s.Name.Contains(search) || s.Price.ToString().Contains(search) || s.Stock.ToString().Contains(search) || s.DateCreated.ToString().Contains(search) || s.DateUpdated.ToString().Contains(search) || s.DateDeleted.ToString().Contains(search)).Select(s => s.Id);

                menuIds.AddRange(st1); menuIds.AddRange(st2); menuIds.AddRange(st3); menuIds.AddRange(st4);
                comMenu();
            }

            void getMenuCategory(long? id)
            {
                var st = _context.Categories.Where(s => s.ParentId == id).Select(s => s.Id);
                foreach (var search in searchList)
                {
                    var ids = from s in _context.Menus
                              join t in _context.MenuContents on s.Id equals t.MenuId
                              join c in _context.Categories on s.CategoryId equals c.Id
                              where (s.Id.ToString().Contains(search) || s.CategoryId.ToString().Contains(search) || s.Name.Contains(search) || s.Price.ToString().Contains(search) || s.Stock.ToString().Contains(search) || s.DateCreated.ToString().Contains(search) || s.DateUpdated.ToString().Contains(search) || s.DateDeleted.ToString().Contains(search) || t.Id.ToString().Contains(search) || ((MenuContentType)t.Type).ToString().Contains(search) || t.TypeOrder.ToString().Contains(search) || t.Value.Contains(search) || c.Id.ToString().Contains(search) || c.ParentId.ToString().Contains(search) || c.Name.Contains(search) || _context.TransactionDetails.Where(s => s.MenuId == s.Id).Sum(s => s.Quantity).ToString().Contains(search)) && s.CategoryId == id
                              select s.Id;
                    menuIds.AddRange(ids);
                    comMenu();
                }
                if (st.Any()) foreach (var i in st) getMenuCategory(i);
            }

            foreach (var i in searchList) if (i != "") getMenu(i);
            if (!menuIds.Any()) getMenu(search);
            if (categoryId.HasValue) { menuIds.Clear(); getMenuCategory(categoryId); }

            foreach (var i in menuIds)
            {
                var menu = await _context.Menus.FindAsync(i);
                CategoryList? category = null;
                if (menu.CategoryId.HasValue) category = await _context.Categories.Where(s => s.Id == menu.CategoryId && s.DateDeleted == null).Select(s => new CategoryList
                {
                    Id = s.Id,
                    ParentId = s.ParentId,
                    Name = s.Name,
                    DateCreated = s.DateCreated,
                    DateUpdated = s.DateUpdated
                }).FirstOrDefaultAsync();

                st.Add(new MenuList
                {
                    Id = menu.Id,
                    CategoryId = category,
                    Name = menu.Name,
                    Price = menu.Price,
                    Stock = menu.Stock,
                    Sold = _context.TransactionDetails.Where(s => s.MenuId == menu.Id).Sum(s => s.Quantity),
                    Images = _context.MenuContents.Where(s => s.Type == (int)MenuContentType.Image && s.DateDeleted == null).Select(s => new MenuContentList
                    {
                        Id = s.Id,
                        MenuId = s.MenuId,
                        Type = (MenuContentType)s.Type,
                        TypeOrder = s.TypeOrder,
                        Value = s.Value,
                        DateCreated = s.DateCreated,
                        DateUpdated = s.DateUpdated
                    }).ToList(),
                    Descriptions = _context.MenuContents.Where(s => s.Type == (int)MenuContentType.Description && s.DateDeleted == null).Select(s => new MenuContentList
                    {
                        Id = s.Id,
                        MenuId = s.MenuId,
                        Type = (MenuContentType)s.Type,
                        TypeOrder = s.TypeOrder,
                        Value = s.Value,
                        DateCreated = s.DateCreated,
                        DateUpdated = s.DateUpdated
                    }).ToList(),
                    Like = _context.Likes.Any(s => s.UserId == valid.Id && s.MenuId == menu.Id && s.DateDeleted == null),
                    Bookmark = _context.Bookmarks.Any(s => s.UserId == valid.Id && s.MenuId == menu.Id && s.DateDeleted == null),
                    Rating = _context.TransactionDetails.Join(_context.Reviews, s => s.Id, t => t.TransactionDetailId, (s, t) => new { s, t }).Where(st => st.s.MenuId == menu.Id).Average(st => st.t.Rating),
                    Reviews = _context.TransactionDetails.Join(_context.Reviews, s => s.Id, t => t.TransactionDetailId, (s, t) => new { s, t }).Where(st => st.s.MenuId == menu.Id).Select(st => new ReviewList
                    {
                        Id = st.t.Id,
                        UserId = st.t.UserId,
                        TransactionDetailId = st.t.TransactionDetailId,
                        Rating = st.t.Rating,
                        Description = st.t.Description,
                        DateCreated = st.t.DateCreated,
                        DateUpdated = st.t.DateUpdated
                    }).ToList(),
                    DateCreated = menu.DateCreated,
                    DateUpdated = menu.DateUpdated,
                    DateDeleted = menu.DateDeleted
                });
            }

            if (minPrice.HasValue) st = st.Where(s => s.Price >= minPrice).ToList();
            if (maxPrice.HasValue) st = st.Where(s => s.Price <= maxPrice).ToList();
            if (minRating.HasValue) st = st.Where(s => s.Rating >= minRating).ToList();
            if (maxRating.HasValue) st = st.Where(s => s.Rating <= maxRating).ToList();
            
            if (type == MenuType.Like) st = st.Where(s => s.Like).ToList();
            else if (type == MenuType.Bookmark) st = st.Where(s => s.Bookmark).ToList();
            else if (type == MenuType.Review) st = st.Where(s => s.Reviews.Any()).ToList();
            else if (type == MenuType.Transaction) st = st.Where(s => _context.Transactions.Join(_context.TransactionDetails, s => s.Id, t => t.TransactionId, (s, t) => new { s, t }).Where(st => st.s.DateDeleted == null && st.t.DateDeleted == null && st.t.MenuId == s.Id && st.s.UserId == valid.Id).Any()).ToList();

            if (soldOut) st = st.Where(s => s.Stock <= 0).ToList();

            if (valid.IsValid)
            {
                if (deleted == Delete.Deleted) st = st.Where(s => s.DateDeleted != null).ToList();
                else if (deleted == Delete.NotDeleted) st = st.Where(s => s.DateDeleted == null).ToList();
            }
            else st.Where(s => s.DateDeleted == null).ToList();

            if (createStart.HasValue) st = st.Where(s => s.DateCreated >= createStart).ToList();
            if (createEnd.HasValue) st = st.Where(s => s.DateCreated <= createEnd).ToList();
            if (updateStart.HasValue) st = st.Where(s => s.DateUpdated >= updateStart).ToList();
            if (updateEnd.HasValue) st = st.Where(s => s.DateUpdated <= updateEnd).ToList();

            if (sort == MenuSort.Id) st = st.OrderBy(s => s.Id).ToList();
            else if (sort == MenuSort.CategoryId) st = st.OrderBy(s => s.CategoryId).ToList();
            else if (sort == MenuSort.Name) st = st.OrderBy(s => s.Name).ToList();
            else if (sort == MenuSort.Price) st = st.OrderBy(s => s.Price).ToList();
            else if (sort == MenuSort.Stock) st = st.OrderBy(s => s.Stock).ToList();
            else if (sort == MenuSort.Image) st = st.OrderBy(s => s.Images.Count()).ToList();
            else if (sort == MenuSort.Description) st = st.OrderBy(s => s.Descriptions).ToList();
            else if (sort == MenuSort.Like) st = st.OrderBy(s => s.Like).ToList();
            else if (sort == MenuSort.Bookmark) st = st.OrderBy(s => s.Bookmark).ToList();
            else if (sort == MenuSort.Rating) st = st.OrderBy(s => s.Rating).ToList();
            else if (sort == MenuSort.Review) st = st.OrderBy(s => s.Reviews.Count()).ToList();
            else if (sort == MenuSort.DateCreated) st = st.OrderBy(s => s.DateCreated).AsEnumerable().Reverse().ToList();
            else if (sort == MenuSort.DateUpdated) st = st.OrderBy(s => s.DateUpdated).AsEnumerable().Reverse().ToList();
            else if (sort == MenuSort.DateDeleted) st = st.OrderBy(s => s.DateDeleted).AsEnumerable().Reverse().ToList();
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

        // GET: api/Menus/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MenuList), 200)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Menu>> GetMenu(long id)
        {
            var valid = Method.Decode(auth());
            if (!MenuExists(id)) return NotFound(new { errors = "Menu Not Found" });

            var menu = await _context.Menus.FindAsync(id);
            CategoryList? category = null;
            if (menu.CategoryId.HasValue) category = await _context.Categories.Where(s => s.Id == menu.CategoryId && s.DateDeleted == null).Select(s => new CategoryList
            {
                Id = s.Id,
                ParentId = s.ParentId,
                Name = s.Name,
                DateCreated = s.DateCreated,
                DateUpdated = s.DateUpdated
            }).FirstOrDefaultAsync();

            return Ok(new MenuList
            {
                Id = menu.Id,
                CategoryId = category,
                Name = menu.Name,
                Price = menu.Price,
                Stock = menu.Stock,
                Sold = _context.TransactionDetails.Where(s => s.MenuId == menu.Id).Sum(s => s.Quantity),
                Images = _context.MenuContents.Where(s => s.Type == (int)MenuContentType.Image && s.DateDeleted == null).Select(s => new MenuContentList
                {
                    Id = s.Id,
                    MenuId = s.MenuId,
                    Type = (MenuContentType)s.Type,
                    TypeOrder = s.TypeOrder,
                    Value = s.Value,
                    DateCreated = s.DateCreated,
                    DateUpdated = s.DateUpdated
                }).ToList(),
                Descriptions = _context.MenuContents.Where(s => s.Type == (int)MenuContentType.Description && s.DateDeleted == null).Select(s => new MenuContentList
                {
                    Id = s.Id,
                    MenuId = s.MenuId,
                    Type = (MenuContentType)s.Type,
                    TypeOrder = s.TypeOrder,
                    Value = s.Value,
                    DateCreated = s.DateCreated,
                    DateUpdated = s.DateUpdated
                }).ToList(),
                Like = _context.Likes.Any(s => s.UserId == valid.Id && s.MenuId == menu.Id && s.DateDeleted == null),
                Bookmark = _context.Bookmarks.Any(s => s.UserId == valid.Id && s.MenuId == menu.Id && s.DateDeleted == null),
                Rating = _context.TransactionDetails.Join(_context.Reviews, s => s.Id, t => t.TransactionDetailId, (s , t) => new {s, t}).Where(st => st.s.MenuId == menu.Id).Average(st => st.t.Rating),
                Reviews = _context.TransactionDetails.Join(_context.Reviews, s => s.Id, t => t.TransactionDetailId, (s, t) => new { s, t }).Where(st => st.s.MenuId == menu.Id).Select(st => new ReviewList
                {
                    Id = st.t.Id,
                    UserId = st.t.UserId,
                    TransactionDetailId = st.t.TransactionDetailId,
                    Rating = st.t.Rating,
                    Description = st.t.Description,
                    DateCreated = st.t.DateCreated,
                    DateUpdated = st.t.DateUpdated
                }).ToList(),
                DateCreated = menu.DateCreated,
                DateUpdated = menu.DateUpdated,
                DateDeleted = menu.DateDeleted
            });
        }

        // PUT: api/Menus/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMenu(long id, MenuParameter menuParameter)
        {
            var valid = Method.Decode(auth());
            if (!valid.IsValid) return StatusCode(401, new { errors = "Access Unauthorized!" });
            if (valid.Role != UserRole.Admin) return StatusCode(403, new { errors = "User Role must be Admin!" });
            if (!ModelState.IsValid) return BadRequest(Method.error(ModelState));
            if (menuParameter.CategoryId != null && !_context.Categories.Any(s => s.Id == menuParameter.CategoryId)) return BadRequest(new { errors = "Category Id Not Valid!" });
            if (menuParameter.Images != null && menuParameter.Images.Count() > 3) return BadRequest(new { errors = "Images Cannot Exceed 3!" });
            if (menuParameter.Images != null) foreach (var i in menuParameter.Images) if (!(Path.GetExtension(i.FileName) == ".png" || Path.GetExtension(i.FileName) == ".jpg" || Path.GetExtension(i.FileName) == ".jpeg")) return BadRequest(new { errors = "Image format must be png, jpg, or jpeg!" });
            if (!MenuExists(id)) return NotFound(new { errors = "Menu Not Found!" });

            var st = _context.Menus.Where(s => s.Id == id).FirstOrDefault();
            st.Name = menuParameter.Name;
            st.CategoryId = menuParameter.CategoryId;
            st.Price = menuParameter.Price;
            st.Stock = menuParameter.Stock;
            st.DateUpdated = DateTime.Now;
            _context.Entry(st).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            if (menuParameter.Images != null && menuParameter.ImageOrders != null)
            {
                if (!Directory.Exists(Method.imgMenuPath)) Directory.CreateDirectory(Method.imgMenuPath);
                var images = menuParameter.Images.Zip(menuParameter.ImageOrders, (s, t) => new { Image = s, Order = t });
                foreach (var i in images)
                {
                    var mc = _context.MenuContents.Where(s => s.MenuId == st.Id && s.Type == (int)MenuContentType.Image && s.TypeOrder == i.Order).FirstOrDefault();
                    mc.Value = $"Image_{DateTime.Now.ToString("yyyy_MM_dd_HHmmss")}_{mc.Id}{Path.GetExtension(i.Image.FileName)}";
                    using (var fs = new FileStream(Path.Combine(Method.imgMenuPath, mc.Value), FileMode.Create))
                    {
                        if (mc.Value != null) new FileInfo(Path.Combine(Method.imgMenuPath, mc.Value)).Delete();
                        i.Image.CopyTo(fs);
                    }
                    mc.DateUpdated = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }

            if (menuParameter.ImageDelete != null && menuParameter.ImageDelete.Value && menuParameter.ImageOrders != null)
            {
                foreach (var i in menuParameter.ImageOrders)
                {
                    var mc = _context.MenuContents.Where(s => s.MenuId == st.Id && s.Type == (int)MenuContentType.Image && s.TypeOrder == i).FirstOrDefault();
                    mc.DateUpdated = DateTime.Now;
                    mc.DateDeleted = DateTime.Now;
                    _context.Entry(mc).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }

            if (menuParameter.Descriptions != null && menuParameter.DescriptionOrders != null)
            {
                foreach (var i in menuParameter.Descriptions.Select((s, i) => new { Value = s, Index = i }))
                {
                    var mc = new MenuContent();
                    mc.Id = contentId();
                    mc.MenuId = st.Id;
                    mc.Type = (int)MenuContentType.Description;
                    mc.TypeOrder = i.Index + 1;
                    mc.Value = i.Value;
                    mc.DateCreated = DateTime.Now;
                    mc.DateUpdated = DateTime.Now;
                    _context.MenuContents.Add(mc);
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(new { messages = "Menu successfully Updated!" });
        }

        // POST: api/Menus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Menu>> PostMenu([FromForm] MenuParameter menuParameter)
        {
            var valid = Method.Decode(auth());
            if (!valid.IsValid) return StatusCode(401, new { errors = "Access Unauthorized!" });
            if (valid.Role != UserRole.Admin) return StatusCode(403, new { errors = "User Role must be Admin!" });
            if (!ModelState.IsValid) return BadRequest(Method.error(ModelState));
            if (menuParameter.CategoryId != null && !_context.Categories.Any(s => s.Id == menuParameter.CategoryId)) return BadRequest(new { errors = "Category Id Not Valid!" });
            if (menuParameter.Images != null && menuParameter.Images.Count() > 3) return BadRequest(new { errors = "Images Cannot Exceed 3!" });
            if (menuParameter.Images != null) foreach (var i in menuParameter.Images) if (!(Path.GetExtension(i.FileName) == ".png" || Path.GetExtension(i.FileName) == ".jpg" || Path.GetExtension(i.FileName) == ".jpeg")) return BadRequest(new { errors = "Image format must be png, jpg, or jpeg!" });

            var st = new Menu();
            st.Id = menuId();
            st.CategoryId = menuParameter.CategoryId;
            st.Name = menuParameter.Name;
            st.Price = menuParameter.Price;
            st.Stock = menuParameter.Stock;
            st.DateCreated = DateTime.Now;
            st.DateUpdated = DateTime.Now;
            _context.Menus.Add(st);
            await _context.SaveChangesAsync();

            for (var i = 1; i <= 3; i++)
            {
                var mc = new MenuContent();
                mc.Id = contentId();
                mc.MenuId = st.Id;
                mc.Type = (int)MenuContentType.Image;
                mc.TypeOrder = i;
                mc.DateCreated = DateTime.Now;
                mc.DateUpdated = DateTime.Now;
                _context.MenuContents.Add(mc);
                await _context.SaveChangesAsync();
            }

            if (menuParameter.Images != null)
            {
                if (!Directory.Exists(Method.imgMenuPath)) Directory.CreateDirectory(Method.imgMenuPath);
                var images = menuParameter.Images.Zip(_context.MenuContents.Where(s => s.MenuId == st.Id).Select(s => s.Id), (s, t) => new { Image = s, Id = t });
                foreach (var i in images)
                {
                    var mc = _context.MenuContents.Where(s => s.Id == i.Id).FirstOrDefault();
                    mc.Value = $"Image_{DateTime.Now.ToString("yyyy_MM_dd_HHmmss")}_{mc.Id}{Path.GetExtension(i.Image.FileName)}";
                    using (var fs = new FileStream(Path.Combine(Method.imgMenuPath, mc.Value), FileMode.Create)) i.Image.CopyTo(fs);
                    mc.DateUpdated = DateTime.Now;
                    _context.Entry(mc).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }

            if (menuParameter.Descriptions != null)
            {
                foreach (var i in menuParameter.Descriptions.Select((s, i) => new { Value = s, Index = i }))
                {
                    var mc = new MenuContent();
                    mc.Id = contentId();
                    mc.MenuId = st.Id;
                    mc.Type = (int)MenuContentType.Description;
                    mc.TypeOrder = i.Index + 1;
                    mc.Value = i.Value;
                    mc.DateCreated = DateTime.Now;
                    mc.DateUpdated = DateTime.Now;
                    _context.MenuContents.Add(mc);
                    await _context.SaveChangesAsync();
                }
            }

            return Created("Menu", new { messages = "Menu successfully Created!" });
        }

        // DELETE: api/Menus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenu(long id)
        {
            var valid = Method.Decode(auth());
            if (!valid.IsValid) return StatusCode(401, new { errors = "Access Unauthorized!" });
            if (valid.Role != UserRole.Admin) return StatusCode(403, new { errors = "User Role must be Admin!" });
            if (!MenuExists(id)) NotFound(new { errors = "Menu Not Found!" });

            var menu = await _context.Menus.FindAsync(id);
            menu.DateUpdated = DateTime.Now;
            menu.DateDeleted = DateTime.Now;
            _context.Entry(menu).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { messages = "Menu successfully Deleted!" });
        }

        private bool MenuExists(long id) => (_context.Menus?.Any(e => e.Id == id)).GetValueOrDefault();

        private string auth()
        {
            Request.Headers.TryGetValue("Authorization", out var auth);
            try { return auth.ToString().Replace("Bearer ", ""); } catch { return auth; }
        }

        private long menuId()
        {
            var st = _context.Menus.OrderByDescending(s => s.Id).Select(s => s.Id);
            if (st.Any()) return st.FirstOrDefault() + 1;
            else return 1;
        }

        private long contentId()
        {
            var st = _context.MenuContents.OrderByDescending(s => s.Id).Select(s => s.Id);
            if (st.Any()) return st.FirstOrDefault() + 1;
            else return 1;
        }
    }
}
