using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using webAdmin.Data;
using webAdmin.ViewModels;

namespace webAdmin.Controllers
{
    [Authorize]
    public class UsersGroupController : ControllerBase
    {
        private readonly ILogger<UsersGroupController> _logger;
        private readonly ApplicationDbContext _context;

        public UsersGroupController(ApplicationDbContext context, ILogger<UsersGroupController> logger) : base(context, logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("/AuthorityGroup")]
        public IActionResult Index()
        {
            BaseGetUserData();
            ViewBag.controller = "AuthorityGroup";
            string auth = AuthCheck();

            if (auth == "")
            {
                TempData["controller"] = "AuthorityGroup";
                return RedirectToAction("NoPermission", "Home");
            }

            return View();
        }

        public async Task<IActionResult> UsersGroupList()
        {
            return _context.Users_group != null ?
                View(await _context.Users_group.ToListAsync()) :
                Problem("Entity set 'ApplicationDbContext.Users_group'  is null.");
        }

        public IActionResult UsersGroupForm()
        {
            return View();
        }

        // POST: UsersGroup/CreateGroup
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [TypeFilter(typeof(LogActionAttribute))]
        public async Task<JsonResult> CreateGroup([Bind("idx,name,creat_date,group_idx")] UsersGroup users_group)
        {
            ViewBag.division = "Change";
            ViewBag.menu = "Authority group";
            ViewBag.item = "Authority group create";
            ViewBag.item_sub = "";
            ViewBag.reason = "";
            ViewBag.action = "Create";
            ViewBag.target_id = "";

            string auth = AuthCheck();

            if (auth == "" || auth == "r")
            {
                TempData["controller"] = "AuthorityGroup";
                return Json(new { status = "error", message = "No permission" });
            }

            if (ModelState.IsValid)
            {
                _context.Add(users_group);
                _context.SaveChanges();
                //TempData["success"] = "Group created successfully";
                //return RedirectToAction(nameof(Index));
                var lastinsertedId = users_group.idx;
                var groupIdx = users_group.group_idx;

                if (groupIdx > 0)
                {
                    if (_context.Users_group_menu != null)
                    {
                        var fromUsersGroupMenu = _context.Users_group_menu.Where(x => x.users_group_idx == groupIdx);
                    
                        foreach (var item in fromUsersGroupMenu)
                        {
                            UsersGroupMenu toUsersGroupMenu = new UsersGroupMenu();

                            toUsersGroupMenu.users_group_idx = lastinsertedId;
                            toUsersGroupMenu.name = item.name;
                            toUsersGroupMenu.controller = item.controller;
                            toUsersGroupMenu.action = item.action;
                            toUsersGroupMenu.allow_type = item.allow_type;

                            _context.Add(toUsersGroupMenu);
                        }

                        await _context.SaveChangesAsync();
                    }
                }

                return Json(new { status = "success", message = "Group created successfully" });
            }

            return Json(new { status = "error", message = "Error!" });
        }

        public IActionResult VerifyGroupName(string name)
        {
            if (_context.Users_group == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Users_group'  is null.");
            }

            var user = _context.Users_group.Where(x => x.name == name).FirstOrDefault();
            if (user != null)
            {
                return Json($"Name {name} is already in use.");
            }

            return Json(true);
        }

        [HttpGet("/UsersGroup/UsersGroupMenuList/{idx}/{admin_idx}")]
        public async Task<IActionResult> UsersGroupMenuList(int idx, int? admin_idx)
        {
            ViewData["UsersGroupIdx"] = idx;
            ViewData["AdminIdx"] = admin_idx;

            if (_context.Users == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Users'  is null.");
            }

            if (admin_idx > 0)
            {
                var user = _context.Users.Where(x => x.idx == admin_idx).FirstOrDefault();

                if (user == null)
                {
                    return Problem("Entity set 'user'  is null.");
                }

                if (user.users_group_idx == 0)
                {
                    return _context.Users_group_menu != null ?
                        View(await _context.Users_group_menu.Where(x => x.admin_idx == admin_idx).ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Users_group_menu'  is null.");
                }
            }

            return _context.Users_group_menu != null ?
                View(await _context.Users_group_menu.Where(x => x.users_group_idx == idx).ToListAsync()) :
                Problem("Entity set 'ApplicationDbContext.Users_group_menu'  is null.");
        }

		[HttpGet("/UsersGroup/UsersMenuList/{idx}/{admin_idx}")]
		public async Task<IActionResult> UsersMenuList(int idx, int? admin_idx)
		{
			ViewData["UsersGroupIdx"] = idx;
			ViewData["AdminIdx"] = admin_idx;

			if (_context.Users == null)
			{
				return Problem("Entity set 'ApplicationDbContext.Users'  is null.");
			}

			if (admin_idx > 0)
			{
				var user = _context.Users.Where(x => x.idx == admin_idx).FirstOrDefault();

				if (user == null)
				{
					return Problem("Entity set 'user'  is null.");
				}

				if (user.users_group_idx == 0)
				{
					return _context.Users_group_menu != null ?
						View(await _context.Users_group_menu.Where(x => x.admin_idx == admin_idx).ToListAsync()) :
						Problem("Entity set 'ApplicationDbContext.Users_menu'  is null.");
				}
			}

			return _context.Users_group_menu != null ?
				View(await _context.Users_group_menu.Where(x => x.users_group_idx == idx).ToListAsync()) :
				Problem("Entity set 'ApplicationDbContext.Users_menu'  is null.");
		}

		public IActionResult UsersGroupMenuForm(int? admin_idx)
        {
            var model = new UsersGroupMenu();
            ViewData["AdminIdx"] = admin_idx;
            model.allow_type = "r";
            return View(model);
        }

        // POST: UsersGroup/CreateGroupMenu
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [TypeFilter(typeof(LogActionAttribute))]
        public async Task<JsonResult> CreateGroupMenu([Bind("idx,users_group_idx,name,controller,action,allow_type,creat_date")] UsersGroupMenu users_group_menu)
        {
            ViewBag.division = "Change";
            ViewBag.menu = "Authority group";
            ViewBag.item = "Group menu create";
            ViewBag.item_sub = "";
            ViewBag.reason = "";
            ViewBag.action = "Create";
            ViewBag.target_id = "";

            int admin_idx = Convert.ToInt32(HttpContext.Request.Form["admin_idx"]);

            string auth = AuthCheck();

            if (auth == "" || auth == "r")
            {
                TempData["controller"] = "AuthorityGroup";
                return Json(new { status = "error", message = "No permission" });
            }

            if (_context.Users == null)
            {
                return Json(new { status = "error", message = "Error!" });
            }

            if (_context.Users_group_menu == null)
            {
                return Json(new { status = "error", message = "Error!" });
            }

            if (ModelState.IsValid)
            {
                if (admin_idx == 0)
                {
                    _context.Add(users_group_menu);
                    await _context.SaveChangesAsync();
                    return Json(new { status = "success", message = "Menu created successfully" });
                }
                else
                {
                    var user = _context.Users.Find(admin_idx);

                    if (user != null)
                    {
                        if (user.users_group_idx > 0)
                        {
                            List<UsersGroupMenu> UsersGroupMenuList = _context.Users_group_menu.Where(x => x.users_group_idx == user.users_group_idx).ToList();

                            foreach (var item in UsersGroupMenuList)
                            {
                                UsersGroupMenu data = new UsersGroupMenu();
                                data.admin_idx = admin_idx;
                                data.users_group_idx = 0;
                                data.name = item.name;
                                data.controller = item.controller;
                                data.action = item.action;
                                data.allow_type = item.allow_type;

                                _context.Users_group_menu.Add(data);
                                _context.SaveChanges();
                            }

                            UsersGroupMenu newData = new UsersGroupMenu();
                            newData.admin_idx = admin_idx;
                            newData.users_group_idx = 0;
                            newData.name = users_group_menu.name;
                            newData.controller = users_group_menu.controller;
                            newData.action = users_group_menu.action;
                            newData.allow_type = users_group_menu.allow_type;
                            _context.Users_group_menu.Add(newData);
                            _context.SaveChanges();

                            user.users_group_idx = 0;
                            _context.Update(user);
                            _context.SaveChanges();
                            return Json(new { status = "success", code = 0, message = "Changed to user group. Menu created successfully" });
                        }
                        else
                        {
                            users_group_menu.admin_idx = admin_idx;
                            _context.Add(users_group_menu);
                            await _context.SaveChangesAsync();
                            return Json(new { status = "success", code = 1, message = "Menu created successfully" });
                        }
                    }
                }
            }

            return Json(new { status = "error", message = "Error!" });
        }

        // POST: UsersGroup/EditGroup/5
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [TypeFilter(typeof(LogActionAttribute))]
        public async Task<IActionResult> EditGroup(int id, [Bind("idx,name,creat_date")] UsersGroup users_group)
        {
            ViewBag.division = "Change";
            ViewBag.menu = "Authority group";
            ViewBag.item = "Authority group edit";
            ViewBag.item_sub = "";
            ViewBag.reason = "";
            ViewBag.action = "Edit";
            ViewBag.target_id = "";

            string auth = AuthCheck();

            if (auth == "" || auth == "r")
            {
                TempData["controller"] = "AuthorityGroup";
                return Json(new { status = "error", message = "No permission" });
            }

            if (id != users_group.idx)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(users_group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsersGroupExists(users_group.idx))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

				return Json(new { status = "success", message = "Group edited successfully" });
            }
            return Json(new { status = "error", message = "Error!" });
        }

        // POST: UsersGroup/EditGroupMenu/5
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [TypeFilter(typeof(LogActionAttribute))]
        public async Task<IActionResult> EditGroupMenu(int id, [Bind("idx,users_group_idx,name,controller,action,allow_type,creat_date")] UsersGroupMenu users_group_menu)
        {
            ViewBag.division = "Change";
            ViewBag.menu = "Authority group";
            ViewBag.item = "Group menu edit";
            ViewBag.item_sub = "";
            ViewBag.reason = "";
            ViewBag.action = "Edit";
            ViewBag.target_id = "";

            int admin_idx = Convert.ToInt32(HttpContext.Request.Form["admin_idx"]);

            string auth = AuthCheck();
            if (auth == "" || auth == "r")
            {
                TempData["controller"] = "AuthorityGroup";
                return Json(new { status = "error", message = "No permission" });
            }

            if (id != users_group_menu.idx)
            {
                return NotFound();
            }

            if (_context.Users == null)
            {
                return Json(new { status = "error", message = "Error!" });
            }

            if (_context.Users_group_menu == null)
            {
                return Json(new { status = "error", message = "Error!" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (admin_idx == 0)
                    {
                        _context.Update(users_group_menu);
                        await _context.SaveChangesAsync();

                        return Json(new { status = "success", message = "Menu edited successfully" });
                    } else
                    {
                        var user = _context.Users.Find(admin_idx);

                        if (user != null)
                        {
                            if (user.users_group_idx > 0)
                            {
                                List<UsersGroupMenu> UsersGroupMenuList = _context.Users_group_menu.Where(x => x.users_group_idx == user.users_group_idx).ToList();

                                foreach (var item in UsersGroupMenuList)
                                {
                                    UsersGroupMenu data = new UsersGroupMenu();
                                    data.admin_idx = admin_idx;
                                    data.users_group_idx = 0;

                                    if (item.idx == id)
                                    {
                                        data.name = users_group_menu.name;
                                        data.controller = users_group_menu.controller;
                                        data.action = users_group_menu.action;
                                        data.allow_type = users_group_menu.allow_type;
                                    }
                                    else { 
                                        data.name = item.name;
                                        data.controller = item.controller;
                                        data.action = item.action;
                                        data.allow_type = item.allow_type;
                                    }

                                    _context.Users_group_menu.Add(data);
                                    _context.SaveChanges();
                                }

                                user.users_group_idx = 0;
                                _context.Update(user);
                                _context.SaveChanges();
                                return Json(new { status = "success", code = 0, message = "Changed to user group. Menu edited successfully" });
                            }
                            else
                            {
                                users_group_menu.admin_idx = admin_idx;
                                _context.Update(users_group_menu);
                                await _context.SaveChangesAsync();
                                return Json(new { status = "success", code = 1, message = "Menu edited successfully" });
                            }
                        }

                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsersGroupMenuExists(users_group_menu.idx))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return Json(new { status = "error", message = "Error!" });
        }

        // POST: UsersGroup/DeleteGroup/5
        [HttpPost, ActionName("DeleteGroup")]
        [TypeFilter(typeof(LogActionAttribute))]
        public async Task<IActionResult> DeleteGroupConfirmed(int id)
        {
            ViewBag.division = "Change";
            ViewBag.menu = "Authority group";
            ViewBag.item = "Authority group delete";
            ViewBag.item_sub = "";
            ViewBag.reason = "";
            ViewBag.action = "Delete";
            ViewBag.target_id = "";

            string auth = AuthCheck("DeleteGroup");

            if (auth == "" || auth == "r")
            {
                TempData["controller"] = "AuthorityGroup";
                return Json(new { status = "error", message = "No permission" });
            }

            if (_context.Users_group == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Users_group'  is null.");
            }
            
            if (_context.Users_group_menu == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Users_group_menu'  is null.");
            }

            var usersGroup = await _context.Users_group.FindAsync(id);
            if (usersGroup != null)
            {
                _context.Users_group.Remove(usersGroup);
            }

            var usersGroupMenu = _context.Users_group_menu.Where(x => x.users_group_idx == id);
            _context.Users_group_menu.RemoveRange(usersGroupMenu);

            _context.SaveChanges();

            return Json(new { status = "success", message = "Group deleted successfully" });
        }

        private bool UsersGroupExists(int idx)
        {
            return (_context.Users_group?.Any(e => e.idx== idx)).GetValueOrDefault();
        }

        private bool UsersGroupMenuExists(int idx)
        {
            return (_context.Users_group_menu?.Any(e => e.idx == idx)).GetValueOrDefault();
        }

        // POST: UsersGroup/DeleteGroupMenu/5
        [HttpPost, ActionName("DeleteGroupMenu")]
        [TypeFilter(typeof(LogActionAttribute))]
        public IActionResult DeleteGroupMenuConfirmed(int id)
        {
            ViewBag.division = "Change";
            ViewBag.menu = "Authority group";
            ViewBag.item = "Group menu delete";
            ViewBag.item_sub = "";
            ViewBag.reason = "";
            ViewBag.action = "Delete";

            int admin_idx = Convert.ToInt32(HttpContext.Request.Form["admin_idx"]);
            
            string auth = AuthCheck("DeleteGroupMenu");
            if (auth == "" || auth == "r")
            {
                TempData["controller"] = "AuthorityGroup";
                return Json(new { status = "error", message = "No permission" });
            }

            if (_context.Users_group_menu == null || _context.Users == null)
            {
                return Problem("Entity set 'ApplicationDbContext'  is null.");
            }

            _logger.LogInformation("users_group_menu.admin_idx {0}", admin_idx);

            if (admin_idx == 0)
            {
                var usersGroupMenu = _context.Users_group_menu.Find(id);
                if (usersGroupMenu != null)
                {
                    _context.Users_group_menu.Remove(usersGroupMenu);
                }

                _context.SaveChanges();
            } else
            {
                var user = _context.Users.Find(admin_idx);
 
                if (user != null)
                {
                    if (user.users_group_idx > 0)
                    {
                        List<UsersGroupMenu> UsersGroupMenuList = _context.Users_group_menu.Where(x => x.users_group_idx == user.users_group_idx).ToList();

                        foreach (var item in UsersGroupMenuList)
                        {
                            if (item.idx == id)
                            {
                                continue;
                            }

                            UsersGroupMenu data = new UsersGroupMenu();

                            data.admin_idx = admin_idx;
                            data.users_group_idx = 0;
                            data.name = item.name;
                            data.controller = item.controller;
                            data.action = item.action;
                            data.allow_type = item.allow_type;
                            _context.Users_group_menu.Add(data);
                            _context.SaveChanges();
                        }

                        user.users_group_idx = 0;
                        _context.Update(user);
                        _context.SaveChanges();
                        return Json(new { status = "success", code = 0, message = "Changed to user group. Menu deleted successfully" });
                    } else
                    {
                        var usersGroupMenu = _context.Users_group_menu.Find(id);
                        if (usersGroupMenu != null)
                        {
                            _context.Users_group_menu.Remove(usersGroupMenu);
                        }

                        _context.SaveChanges();
                        return Json(new { status = "success", code = 1, message = "Menu deleted successfully" });
                    }
                }
            }

            return Json(new { status = "success", message = "Menu deleted successfully" });
        }
    }
}
