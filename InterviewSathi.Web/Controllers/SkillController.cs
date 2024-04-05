using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InterviewSathi.Web.Data;
using InterviewSathi.Web.Models.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace InterviewSathi.Web.Controllers
{
    public class SkillController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SkillController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Skill
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Skills.ToListAsync());
        }

        public async Task<IActionResult> ListUserSkill(string id)
        {
            return View(await _context.UserSkills.Where(x => x.UserId == id).Include(x => x.Skill).ToListAsync());
        }

        public async Task<IActionResult> AddSkill(string id)
        {
            List<UserSkill> userSkills = await _context.UserSkills.Where(x => x.UserId == id).ToListAsync();
            List<Skill> allSkills = await _context.Skills.ToListAsync();

            // Filter out skills that are already selected by the user
            List<Skill> remainingSkills = allSkills
                .Where(skill => !userSkills.Any(userSkill => userSkill.SkillId == skill.Id))
                .ToList();

            ViewBag.UserSkills = userSkills;
            ViewBag.Skills = remainingSkills;
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSkill(List<string> skillSelected)
        {
            foreach (string item in skillSelected)
            {
                UserSkill userSkill = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString(),
                    SkillId = item,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserSkills.Add(userSkill);
                await _context.SaveChangesAsync();
                TempData["success"] = "New Skill Added";
            }
            return RedirectToAction("ListUserSkill", "Skill", new { id = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString() });
        }


        public async Task<IActionResult> DeleteSkill(string Id)
        {
            var skill = await _context.UserSkills.FindAsync(Id);
            if (skill != null)
            {
                _context.UserSkills.Remove(skill);
            }
            await _context.SaveChangesAsync();
            TempData["error"] = "Skill Deleted";
            return RedirectToAction("ListUserSkill", "Skill", new { id = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString() });
        }

        // GET: Skill/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("NameOfSkill,DescofSkill,Id,CreatedAt")] Skill skill)
        {
            if (ModelState.IsValid)
            {
                skill.Id = Guid.NewGuid().ToString();
                _context.Add(skill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return PartialView(skill);
        }

        // GET: Skill/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skill = await _context.Skills.FindAsync(id);
            if (skill == null)
            {
                return NotFound();
            }
            return PartialView(skill);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id, [Bind("NameOfSkill,DescofSkill,Id,CreatedAt")] Skill skill)
        {
            if (id != skill.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(skill);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SkillExists(skill.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return PartialView(skill);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string Id)
        {
            var skill = await _context.Skills.FindAsync(Id);
            if (skill != null)
            {
                _context.Skills.Remove(skill);
            }
            await _context.SaveChangesAsync();
            TempData["error"] = "Skill Deleted";
            return RedirectToAction("Index", "Skill");
        }

        private bool SkillExists(string id)
        {
            return _context.Skills.Any(e => e.Id == id);
        }
    }
}
