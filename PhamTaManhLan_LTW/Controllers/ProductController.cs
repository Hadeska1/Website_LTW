using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace WebsiteBanHang.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // GET: Display the Add Product Form
        public IActionResult Add()
        {
            var categories = _categoryRepository.GetAllCategories();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        // POST: Process the Add Product Form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Product product, IFormFile imageUrl, List<IFormFile> imageUrls)
        {
            var categories = _categoryRepository.GetAllCategories();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            if (imageUrl != null && imageUrl.Length > 0)
            {
                product.ImageUrl = await SaveImage(imageUrl);
            }

            if (imageUrls != null && imageUrls.Any())
            {
                product.ImageUrls = new List<string>();
                foreach (var file in imageUrls)
                {
                    if (file.Length > 0)
                    {
                        product.ImageUrls.Add(await SaveImage(file));
                    }
                }
            }

            if (ModelState.IsValid)
            {
                _productRepository.Add(product);
                return RedirectToAction("Index");
            }

            return View(product);
        }

        // GET: Display a List of Products
        public IActionResult Index()
        {
            var products = _productRepository.GetAll();
            return View(products);
        }

        // GET: Display a Single Product
        public IActionResult Display(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // GET: Show the Update Form
        public IActionResult Update(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Process the Product Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(Product product)
        {
            if (ModelState.IsValid)
            {
                _productRepository.Update(product);
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // GET: Show the Delete Confirmation Page
        public IActionResult Delete(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Process the Product Deletion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            _productRepository.Delete(id);
            return RedirectToAction("Index");
        }

        // Method to Save Image
        private async Task<string> SaveImage(IFormFile image)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/images/" + uniqueFileName;
        }
    }
}
