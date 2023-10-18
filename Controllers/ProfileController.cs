using Microsoft.AspNetCore.Mvc;
using webAdmin.Data;
using webAdmin.Extenstions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using System.Data.Entity.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using webAdmin.ViewModels;

namespace webAdmin.Controllers
{
	[Authorize]
	public class ProfileController : ControllerBase
	{
		private readonly ILogger<AdminController> _logger;
		private readonly ApplicationDbContext _context;
		private readonly IStringLocalizer<AdminController> _localizer;

		public ProfileController(ApplicationDbContext context, ILogger<AdminController> logger, IStringLocalizer<AdminController> localizer) : base(context, logger)
		{
			_context = context;
			_logger = logger;
			_localizer = localizer;
		}

		// GET: Profle/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			BaseGetUserData();

			ViewBag.controller = "Profile";
			ViewBag.admin_idx = id;

			if (id == null || _context.Admins == null)
			{
				return NotFound();
			}

			var user = await _context.Admins.FindAsync(id);

			if (user == null)
			{
				return NotFound();
			}

			if (_context.Users_group == null)
			{
				return NotFound();
			}

			var usersGroupList = _context.Users_group.ToList();
			ViewBag.users_group_list = new SelectList((System.Collections.IEnumerable)usersGroupList, "idx", "name", user.users_group_idx);

			ViewBag.status_list = new SelectList(new[]
				{
					new { Idx="0", Name=_localizer["unauthenticated"] },
					new { Idx="10", Name=_localizer["authenticated"] },
					new { Idx="20", Name=_localizer["withdrawn"] },
					new { Idx="21", Name=_localizer["blocked"] }
				}, "Idx", "Name", user.status);

			return View(user);
		}

		// POST: Profile/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int? id, [Bind("idx,user_id,user_pw,user_pw_confirm,name,dept,email,status,pw_error_count,users_group_idx,pw_update_date,reason")] Admin admin)
		{
			if (id == null || _context.Users == null || _context.Users_group_menu == null)
			{
				return NotFound();
			}

			User? user = _context.Users.Where(x => x.idx == id).FirstOrDefault();

			if (user == null || user.user_id == null)
			{
				return NotFound();
			}

			ViewBag.target_id = user.user_id;

			if (admin.user_pw == null)
			{
				admin.user_pw = user.user_pw;
			}
			else
			{
				string password = admin.user_pw;
				var result = PasswordHasher.HashPassword(password);

				admin.user_pw = result;
			}

			user.user_pw = admin.user_pw;
			user.name = admin.name;
			user.dept = admin.dept;
			user.email = admin.email;

			try
			{
				_context.Update(user);
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException /* ex */)
			{
				TempData["error"] = "My info modified fail";
				return Redirect("/Profile/Edit/" + id);
			}

			TempData["success"] = "My info modified successfully";
			return Redirect("/Profile/Edit/" + id);
		}
	}
}
