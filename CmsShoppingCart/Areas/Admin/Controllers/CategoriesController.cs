using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CmsShoppingCart.Infrastructure;
using Microsoft.EntityFrameworkCore;
using CmsShoppingCart.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly CmsShoppingCartContext context;
        public CategoriesController(CmsShoppingCartContext context)
        {
            this.context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await context.Categories.OrderBy(x => x.Sorting).ToListAsync());
        }

        //Get /admin/categories/create
        public IActionResult Create() => View();

        //POST /admin/categories/create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.ToLower().Replace(" ", "-");
                category.Sorting = 100;

                var slug = await context.Categories.FirstOrDefaultAsync(x => x.Slug == category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "The page already exists.");
                    return View(category);
                }

                context.Add(category);
                await context.SaveChangesAsync();

                TempData["Success"] = "The page has been added";

                return RedirectToAction("Index");

            }
            return View(category);
        }

        //Get /admin/categories/edit/id
        public async Task<IActionResult> Edit(int id)
        {
            Category category = await context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            else
            {
                return View(category);
            }
        }

        //POST /admin/pages/edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.ToLower().Replace(" ", "-");
                var slug = await context.Categories.Where(c => c.Id != id).FirstOrDefaultAsync(c => c.Slug == category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "The category is exists");
                    return View(category);
                }
                context.Categories.Update(category);
                await context.SaveChangesAsync();
                TempData["Success"] = "The category has been edited";
                return RedirectToAction("Edit", new {id});
            }
            return View(category);
        }
        //get /admin/categories/delete/id
        public async Task<IActionResult> Delete(int id)
        {
            Category category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                TempData["Error"] = "Category not found";
            }
            else
            {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
                TempData["Success"] = "Category has been deleted!";
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Reorder(int[] id)
        {
            int count = 1;
            foreach(var pageId in id)
            {
                Category category = await context.Categories.FirstOrDefaultAsync(c => c.Id == pageId);
                if(category != null)
                {
                    category.Sorting = count;
                    context.Categories.Update(category);
                    await context.SaveChangesAsync();
                    count++;
                }
            }
            return Ok();
        }

    }
}
