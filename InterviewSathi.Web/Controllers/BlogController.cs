using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InterviewSathi.Web.Data;
using InterviewSathi.Web.Models.Entities.BlogsEntity;
using InterviewSathi.Web.Models;
using InterviewSathi.Web.Models.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace InterviewSathi.Web.Controllers
{
    [Authorize]
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BlogController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _env = webHostEnvironment;
        }

        // GET: Blog
        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 5;
            var blogsWithUsers = _context.Blogs.OrderByDescending(x => x.CreatedAt).Include(blog => blog.User);
            var paginatedBlogs = await PaginatedList<Blog>.CreateAsync(blogsWithUsers.AsNoTracking(), page ?? 1, pageSize);
            ViewBag.like = _context.LikeCounts.ToList();
            ViewBag.MyProfile = _context.ApplicationUsers.FirstOrDefault(x => x.Id == User.FindFirstValue(ClaimTypes.NameIdentifier).ToString());
            return View(paginatedBlogs);
        }

        // GET: Blog/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // GET: Blog/Create
        public IActionResult Create()
        {
            return PartialView();
        }

        // POST: Blog/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog blog)
        {
            if (!ModelState.IsValid)
            {
                return PartialView(blog);
            }
            try
            {
                if (blog.BlogPath != null)
                {
                    string filename = Guid.NewGuid() + Path.GetExtension(blog.BlogPath.FileName);
                    string imgpath = Path.Combine(_env.WebRootPath, "Images/Blogs/", filename);
                    using (FileStream streamread = new FileStream(imgpath, FileMode.Create))
                    {
                        blog.BlogPath.CopyTo(streamread);
                    }
                    blog.ImgPath = filename;
                }

                _context.Add(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Blog");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return View(blog);
        }

        [HttpPost]
        public IActionResult Like(string BlogId, string UserId)
        {
            var likes = _context.LikeCounts.FirstOrDefault(x => x.LikedBlog == BlogId && x.LikedBy == UserId);
            if (likes == null)
            {
                LikeCount likeCount = new()
                {
                    LikedBlog = BlogId,
                    LikedBy = UserId
                };
                _context.LikeCounts.Add(likeCount);
                _context.SaveChanges();

                Blog blog = _context.Blogs.FirstOrDefault(x => x.Id == BlogId);
                blog.LikeCount += 1;
                _context.Blogs.Update(blog);
                _context.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
            else
            {
                _context.LikeCounts.Remove(likes);
                _context.SaveChanges();

                Blog blog = _context.Blogs.FirstOrDefault(x => x.Id == BlogId);
                blog.LikeCount -= 1;
                _context.Blogs.Update(blog);
                _context.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        public IActionResult Comment(string id, string backurl)
        {
            string myId = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString();
            ViewBag.MyId = _context.ApplicationUsers.FirstOrDefault(x => x.Id == myId);
            ViewBag.result = _context.Comments.Where(x => x.CommentBlog == id).Include(x => x.User).ToList().OrderByDescending(x => x.CreatedAt).ToList();
            ViewBag.BackUrl = backurl;
            var blogs = _context.Blogs.Include(blog => blog.User).FirstOrDefault(x => x.Id == id);
            return View(blogs);
        }

        [HttpPost]
        public async Task<IActionResult> Comment(string UserId, string BlogId, string Content, string Backurl)
        {
            Comment comment = new()
            {
                CommentBy = UserId,
                CommentBlog = BlogId,
                Content = Content
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("comment", "Blog", new { id = comment.CommentBlog, backUrl = Backurl });
        }


        // GET: Blog/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }
            return PartialView(blog);
        }

        // POST: Blog/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Blog blog)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (blog.BlogPath != null)
                    {
                        string blogPic = Guid.NewGuid() + Path.GetExtension(blog.BlogPath.FileName).ToUpper();
                        string blogPath = Path.Combine(_env.WebRootPath, "Images/Blogs/", blogPic);
                        using (FileStream stream = new(blogPath, FileMode.Create))
                        {
                            blog.BlogPath.CopyTo(stream);
                        }
                        blog.ImgPath = blogPic;
                    }
                    _context.Update(blog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(blog.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index",
                                        "Profile",
                                        new { id = _context.ApplicationUsers.FirstOrDefault(x => x.Id == blog.PostedBy)?.Id });
            }
            return PartialView(blog);
        }

        // GET: Blog/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return PartialView(blog);
        }


        [HttpPost, ActionName("DeleteComment")]
        public async Task<IActionResult> DeleteComment(string id, string backurl)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
            }
            ViewBag.BackUrl = backurl;
            await _context.SaveChangesAsync();
            return RedirectToAction("comment", "Blog", new { id = comment.CommentBlog, backUrl = backurl });
        }

        // POST: Blog/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                _context.Blogs.Remove(blog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BlogExists(string id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        }
    }
}
