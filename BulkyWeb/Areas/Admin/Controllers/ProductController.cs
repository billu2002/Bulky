using Bulky.DataAccess.Repository.IRepository;
using Bulky.DataAcess.Data;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
   [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
           
            return View(objProductList);
        }



	

		public IActionResult Upsert(int? id)
        {

            ProductVM productVM = new()
            {

                CategoryList = _unitOfWork.Category.GetAll().Select(u =>
               new SelectListItem
               {
                   Text = u.Name,
                   Value = u.Id.ToString()
               }),
                Product = new Product()
            };
            if (id == null || id == 0)
            {
                // Create
                return View(productVM);
            }
            else
            {
                // Update 
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
                return View(productVM);
            }
        }

		//[HttpPost]
		//public IActionResult UploadCSV(ProductVM productVM, IFormFile file)
		//{
		//	if (ModelState.IsValid)
		//	{
		//		string wwwRootPath = _webHostEnvironment.WebRootPath;
		//		if (file != null && file.Length > 0)
		//		{
		//			string csvFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
		//			string csvFilePath = Path.Combine(wwwRootPath, @"csvfiles", csvFileName);

		//			// Save the uploaded CSV file to the server
		//			using (var fileStream = new FileStream(csvFilePath, FileMode.Create))
		//			{
		//				file.CopyTo(fileStream);
		//			}

		//			// Read the CSV file and process the records
		//			using (var reader = new StreamReader(csvFilePath))
		//			{
		//				while (!reader.EndOfStream)
		//				{
		//					var line = reader.ReadLine();
		//					var values = line.Split(',');

		//					// Extract the data from the CSV file
		//					string productName = values[0];
		//					double productPrice = double.Parse(values[1]);
		//					// Extract other properties as needed

		//					// Check if the product already exists



		//						// Create a new product
		//						Product newProduct = new Product
		//						{
		//							Title = productName,
		//							Price = productPrice,
		//							// Set other properties accordingly
		//						};
		//						_unitOfWork.Product.Add(newProduct);
		//				}
		//			}

		//			// Delete the uploaded CSV file after processing
		//			System.IO.File.Delete(csvFilePath);
		//		}

		//		_unitOfWork.Save();
		//		TempData["success"] = "Product(s) created successfully";
		//		return RedirectToAction("Index");
		//	}
		//	else
		//	{
		//		productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u =>
		//		   new SelectListItem
		//		   {
		//			   Text = u.Name,
		//			   Value = u.Id.ToString()
		//		   });
		//		return View(productVM);
		//	}
		//}

		
		public IActionResult UploadCSV(IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				ModelState.AddModelError("file", "Please select a CSV file to upload.");
				return View();
			}

			if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
			{
				TempData["success"] = "Incorrect file format!";
				return View();
			}

			// Read the CSV file
			using (var reader = new StreamReader(file.OpenReadStream()))
			{
				// Skip the header row if needed
				reader.ReadLine();

				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					var values = line.Split(',');

					// Extract data from the CSV line
					int Id = int.Parse(values[0]);
					string Title = values[1];
					string Description = values[2];
					string ISBN = values[3];
					string Author = values[4];
					double ListPrice = double.Parse(values[5]);
					double Price = double.Parse(values[6]);
					double Price50 = double.Parse(values[7]);
					double Price100 = double.Parse(values[8]);
					int CategoryId = int.Parse(values[9]);
					string ImageURL = values[10];
					// Extract other properties as needed

					// Create a new product instance
					var product = new Product
					{
						
						Title = Title,
						Description = Description,
						ISBN = ISBN,
						Author = Author,
						ListPrice = ListPrice,
						Price = Price,
						Price50 = Price50,
						Price100 = Price100,
						CategoryId = CategoryId,
						ImageUrl = ImageURL
					// Set other properties accordingly
				};

					// Add the product to the database 
					
						_unitOfWork.Product.Add(product);
				}
			}

			_unitOfWork.Save();
			TempData["success"] = "CSV file imported successfully.";
			return RedirectToAction("Index");
		}


		[HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); 
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if(!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        // delete the old image
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using ( var fileStream = new FileStream(Path.Combine(productPath, fileName),FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }

                if(productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                }
                else
                {
					_unitOfWork.Product.Update(productVM.Product);

				}
				_unitOfWork.Save();
                TempData["success"] = "Product created succefully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u =>
                   new SelectListItem
                   {
                       Text = u.Name,
                       Value = u.Id.ToString()
                   });
				return View(productVM);
			}
            
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Product? obj = _unitOfWork.Product.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Product deleted succefully";
            return RedirectToAction("Index");
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
			List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objProductList });
		}
        [HttpDelete]
        public IActionResult Delete(int? id) 
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message ="Error while deletin" });
            }
            //Deleting the image
			var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));

			if (System.IO.File.Exists(oldImagePath))
			{
				System.IO.File.Delete(oldImagePath);
			}

            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json(new { succuss = true, message = "Delete Successful" });
		}
		#endregion
	}
}
