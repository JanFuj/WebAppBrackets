﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TestWebAppCoolName.Helpers;
using TestWebAppCoolName.Models;

namespace TestWebAppCoolName.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private ApplicationDbContext _context;

        // GET: Admin
        public AdminController()
        {
            _context = new ApplicationDbContext();
        }

        public ActionResult Index()
        {
            return View();
        }

        #region Kurz
        public ActionResult ApproveCourse(int id,bool approve)
        {
            try
            {

                var course = _context.Courses.FirstOrDefault(c => c.Id == id);
                if (course == null)
                {
                    return HttpNotFound();
                }

                course.Approved = approve;
                _context.SaveChanges();
                return RedirectToAction("Course");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        [Route("admin/kurz")]
        public ActionResult Course()
        {
            var courses = _context.Courses.Include(b => b.Lector).ToList();
            return View(courses);
        }

        [Route("admin/kurz/new")]
        public ActionResult NewCourse()
        {
            var persons = _context.Persons.ToList();
            var viewModel = new CourseViewModel()
            {
                Persons = persons
            };
            return View(viewModel);
        }

        [Route("admin/kurz/new")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewCourse(CourseViewModel vm)
        {
            var tags = TagParser.ParseTags(vm.Tagy, _context);
            var persons = _context.Persons.ToList();
            var viewModel = new CourseViewModel()
            {
                Persons = persons,
            };

            if (!ModelState.IsValid)
            {
                viewModel.Course = vm.Course;
                viewModel.Course.Tags = tags;
                return View(viewModel);
            }

            var exist = _context.Courses.FirstOrDefault(c => c.UrlTitle == vm.Course.UrlTitle);

            if (exist != null)
            {
                ModelState.AddModelError("course.UrlTitle", "Zadany url titulek již existuje");
                viewModel.Course = vm.Course;
                viewModel.Course.Tags = tags;
                return View(viewModel);
            }


            vm.Course.Created = DateTime.Now;
            vm.Course.Changed = DateTime.Now;
            vm.Course.Tags = tags;
            _context.Courses.Add(vm.Course);
            _context.SaveChanges();

            return RedirectToAction("Course");
        }

        [Route("admin/kurz/edit/{id?}")]
        public ActionResult EditCourse(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var course = _context.Courses.Include(b => b.Lector).Include(b => b.Tags).FirstOrDefault(b => b.Id == id);
            if (course == null)
            {
                return HttpNotFound();
            }

            var persons = _context.Persons.ToList();
            var viewModel = new CourseViewModel()
            {
                Persons = persons,
                Course = course
            };
            return View(viewModel);
        }

        [Route("admin/kurz/edit/{id?}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCourse(CourseViewModel vm)
        {

            var tags = TagParser.ParseTags(vm.Tagy, _context);
            if (!ModelState.IsValid)
            {

                var persons = _context.Persons.ToList();
                var viewModel = new CourseViewModel()
                {
                    Persons = persons,
                    Course = vm.Course
                };

                return View(viewModel);
            }

            var existingCourse = _context.Courses.FirstOrDefault(c => c.UrlTitle == vm.Course.UrlTitle);
            bool sameUrlInAnotherCourse = false;
            if (existingCourse != null)
            {
                sameUrlInAnotherCourse = existingCourse?.Id != vm.Course.Id;
            }

            if (sameUrlInAnotherCourse)
            {
                ModelState.AddModelError("course.UrlTitle", "Zadany url titulek již existuje");
                var persons = _context.Persons.ToList();
                var viewModel = new CourseViewModel()
                {
                    Persons = persons,
                    Course = vm.Course
                };
                return View(viewModel);
            }

            var cour = _context.Courses.Include(c => c.Tags).FirstOrDefault(c => c.Id == vm.Course.Id);
            if (cour != null)
            {
                cour.Name = vm.Course.Name;
                cour.Description = vm.Course.Description;
                cour.WillLearn = vm.Course.WillLearn;
                cour.Body = vm.Course.Body;
                cour.Lector_Id = vm.Course.Lector_Id;
                cour.Modificator = vm.Course.Modificator;
                cour.Svg = vm.Course.Svg;
                cour.UrlTitle = vm.Course.UrlTitle;
                cour.Tags = tags;
                cour.Changed = DateTime.Now;
                _context.SaveChanges();
                return RedirectToAction("Course");
            }
            else
            {
                return HttpNotFound();
            }
        }

        [Route("admin/kurz/delete/{id?}")]
        public ActionResult DeleteCourse(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var course = _context.Courses.FirstOrDefault(b => b.Id == id);
            if (course == null)
            {
                return HttpNotFound();
            }

            return View(course);
        }

        [Route("admin/kurz/delete/{id?}")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            _context.Courses.Remove(course ?? throw new InvalidOperationException());
            _context.SaveChanges();
            return RedirectToAction("Course");
        }

        #endregion

        #region Blog

        [Route("admin/blog")]
        public ActionResult Blog()
        {

            return View(_context.Blogs.Include(b => b.Author).ToList());
        }

        [Route("admin/blog/new")]
        public ActionResult NewBlog()
        {
            var persons = _context.Persons.ToList();
            var viewModel = new BlogViewModel()
            {
                Persons = persons,
                Blog = new Blog(),

            };
            return View(viewModel);
        }

        [Route("admin/blog/new")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewBlog(BlogViewModel vm)
        {

            var tags = TagParser.ParseTags(vm.Tagy, _context); // ParseTags(vm.Tagy);
            var persons = _context.Persons.ToList();
            var viewModel = new BlogViewModel()
            {
                Persons = persons
            };
            if (!ModelState.IsValid)
            {
                viewModel.Blog = vm.Blog;
                viewModel.Blog.Tags = tags;
                return View(viewModel);
            }

            var exist = _context.Blogs.FirstOrDefault(b => b.UrlTitle == vm.Blog.UrlTitle);
            if (exist != null)
            {
                ModelState.AddModelError("blog.UrlTitle", "Zadany url titulek již existuje");
                viewModel.Blog = vm.Blog;
                viewModel.Blog.Tags = tags;
                return View(viewModel);
            }


            vm.Blog.Created = DateTime.Now;
            vm.Blog.Changed = DateTime.Now;
            vm.Blog.Tags = tags;
            _context.Blogs.Add(vm.Blog);
            _context.SaveChanges();
            ViewData["Saved"] = "Blog byl vyrvořen";
            ModelState.Clear();
            return RedirectToAction("Blog");

        }
        [Route("admin/blog/edit/{id?}")]
        public ActionResult EditBlog(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Blog blog = _context.Blogs.Include(b => b.Author).Include(b => b.Tags).FirstOrDefault(b => b.Id == id);
            if (blog == null)
            {
                return HttpNotFound();
            }

            var persons = _context.Persons.ToList();
            var viewModel = new BlogViewModel()
            {
                Persons = persons,
                Blog = blog

            };
            return View(viewModel);
        }

        // POST: Blog/Edit/5
        [Route("admin/blog/edit/{id?}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBlog(BlogViewModel vm)
        {
            var tags = TagParser.ParseTags(vm.Tagy, _context); //ParseTags(vm.Tagy);
            if (!ModelState.IsValid)
            {
                var persons = _context.Persons.ToList();
                var viewModel = new BlogViewModel()
                {
                    Persons = persons,
                    Blog = vm.Blog

                };
                return View(viewModel);
            }

            //muze editovat pouze pokud stejny url title neexistuje u jineho blogu
            var existingBlog = _context.Blogs.FirstOrDefault(b => b.UrlTitle == vm.Blog.UrlTitle);
            bool sameUrlInAnotherBlog = false;
            if (existingBlog != null)
            {
                sameUrlInAnotherBlog = existingBlog.Id != vm.Blog.Id;
            }

            if (sameUrlInAnotherBlog)
            {
                ModelState.AddModelError("blog.UrlTitle", "Zadany url titulek již existuje");
                var persons = _context.Persons.ToList();
                var viewModel = new BlogViewModel()
                {
                    Persons = persons,
                    Blog = vm.Blog

                };
                return View(viewModel);
            }

            var blo = _context.Blogs.Include(b => b.Tags).FirstOrDefault(b => b.Id == vm.Blog.Id);
            if (blo != null)
            {


                blo.Tags = null;
                _context.SaveChanges();


                blo.Name = vm.Blog.Name;
                blo.Description = vm.Blog.Description;
                blo.Body = vm.Blog.Body;
                blo.Author_Id = vm.Blog.Author_Id;
                blo.UrlTitle = vm.Blog.UrlTitle;
                blo.Tags = tags;
                blo.Changed = DateTime.Now;
                _context.SaveChanges();
                return RedirectToAction("Blog");
            }
            else
            {
                return HttpNotFound();
            }
        }
        [Route("admin/blog/delete/{id?}")]
        public ActionResult DeleteBlog(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var blog = _context.Blogs.FirstOrDefault(b => b.Id == id);
            if (blog == null)
            {
                return HttpNotFound();
            }

            return View(blog);
        }


        // POST: Blog/Delete/5
        [Route("admin/blog/delete/{id?}")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteBlogConfirmed(int id)
        {
            var blog = _context.Blogs.FirstOrDefault(b => b.Id == id);
            _context.Blogs.Remove(blog);
            _context.SaveChanges();
            return RedirectToAction("Blog");
        }

        #endregion

        #region Tag

        [Route("admin/tag")]
        public async Task<ActionResult> Tag()
        {
            return View(await _context.Tags.ToListAsync());
        }
        [Route("admin/tag/new")]
        public ActionResult NewTag()
        {
            return View();
        }
        // POST: Tags/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Route("admin/tag/new")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NewTag([Bind(Include = "Id,Name")] Tag tag)
        {
            if (ModelState.IsValid)
            {
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
                return RedirectToAction("Tag");
            }

            return View(tag);
        }

        [Route("admin/tag/edit/{id?}")]
        public async Task<ActionResult> EditTag(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tag tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return HttpNotFound();
            }
            return View(tag);
        }

        // POST: Tags/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Route("admin/tag/edit/{id?}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditTag([Bind(Include = "Id,Name")] Tag tag)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(tag).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("Tag");
            }
            return View(tag);
        }


        // GET: Tags/Delete/5
        [Route("admin/tag/delete")]
        public async Task<ActionResult> DeleteTag(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tag tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return HttpNotFound();
            }
            return View(tag);
        }

        // POST: Tags/Delete/5
        [Route("admin/tag/delete")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteTagConfirmed(int id)
        {
            Tag tag = await _context.Tags.FindAsync(id);
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return RedirectToAction("Tag");
        }
        #endregion

        #region People
        [Route("admin/person")]
        public ActionResult Person()
        {
            return View(_context.Persons.ToList());
        }
        [Route("admin/person/new")]
        public ActionResult NewPerson()
        {
            return View();
        }

        // POST: People/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Route("admin/person/new")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewPerson([Bind(Include = "Name,LastName")] Person person)
        {
            if (ModelState.IsValid)
            {
                person.FullName = person.Name + " " + person.LastName;
                _context.Persons.Add(person);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(person);
        }
        [Route("admin/person/edit/{id?}")]
        public ActionResult EditPerson(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Person person = _context.Persons.Find(id);
            if (person == null)
            {
                return HttpNotFound();
            }
            return View(person);
        }

        // POST: People/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Route("admin/person/edit/{id?}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPerson([Bind(Include = "Id,Name,LastName")] Person person)
        {
            if (ModelState.IsValid)
            {
                // db.Entry(person).State = EntityState.Modified;
                var per = _context.Persons.FirstOrDefault(p => p.Id == person.Id);
                if (per != null)
                {
                    per.Name = person.Name;
                    per.LastName = person.LastName;
                    per.FullName = person.Name + " " + person.LastName;
                    _context.SaveChanges();
                    return RedirectToAction("Person");
                }
                else
                {
                    return HttpNotFound();
                }
            }
            return View(person);
        }

        // GET: People/Delete/5
        [Route("admin/person/delete")]
        public ActionResult DeletePerson(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Person person = _context.Persons.Find(id);
            if (person == null)
            {
                return HttpNotFound();
            }
            return View(person);
        }

        // POST: People/Delete/5
        [Route("admin/person/delete")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePersonConfirmed(int id)
        {
            Person person = _context.Persons.Find(id);
            _context.Persons.Remove(person);
            _context.SaveChanges();
            return RedirectToAction("Person");
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }


    }
}