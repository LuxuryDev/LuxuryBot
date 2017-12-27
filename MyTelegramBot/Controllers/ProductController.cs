using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
namespace MyTelegramBot.Controllers
{
    public class ProductController : Controller
    {
        MarketBotDbContext db;

        public IActionResult Index()
        {
            db = new MarketBotDbContext();
            var products = db.Product.Include(p => p.Category).ToList();
           
            foreach(Product p in products)
                p.ProductPrice = db.ProductPrice.Where(pr => pr.ProductId == p.Id && pr.Enabled == true).Include(pr=>pr.Currency).OrderByDescending(pr => pr.Id).ToList();            

            return View(products);
        }

        [HttpGet]
        public IActionResult Creator()
        {
            db = new MarketBotDbContext();
            Product product = new Product();
            product.Id = 0;
            product.Name = String.Empty;
            product.CategoryId = 0;
            product.UnitId = 1;
            product.TelegraphUrl = String.Empty;
            product.Text = String.Empty;
            product.PhotoUrl = String.Empty;
            product.ProductPrice.Add(new ProductPrice { CurrencyId = 1, Value = 0 });
            ViewBag.Phones = new SelectList(db.Category.ToList(), "Id", "Name", db.Category.FirstOrDefault().Id);
            ViewBag.Currency = db.Currency.ToList();
            ViewBag.Unit = new SelectList(db.Units.ToList(), "Id", "Name", product.UnitId);
            return View("Editor", product);
        }

        [HttpGet]
        public IActionResult Editor (int id)
        {
            if (id > 0)
            {
                db = new MarketBotDbContext();
                var product = db.Product.Where(p => p.Id == id).Include(p => p.Unit).Include(p=>p.Category).FirstOrDefault();
                product.ProductPhoto = db.ProductPhoto.Where(photo => photo.ProductId == product.Id).Include(photo => photo.AttachmentFs).ToList();
                product.ProductPrice.Add(db.ProductPrice.Where(price => price.ProductId == product.Id && price.Enabled == true).OrderByDescending(price => price.Id).FirstOrDefault());
                if (product.ProductPhoto.FirstOrDefault() != null)
                {
                    string imageBase64Data = Convert.ToBase64String(product.ProductPhoto.FirstOrDefault().AttachmentFs.Fs);
                    string imageDataURL = string.Format("data:image/png;base64,{0}", imageBase64Data);
                    ViewBag.ImageData = imageDataURL;
                }


                ViewBag.Phones = new SelectList(db.Category.ToList(), "Id", "Name",product.CategoryId);
                //ViewBag.Currency = new SelectList(db.Currency.ToList(), "Id", "Name");
                ViewBag.Currency = db.Currency.ToList();
                ViewBag.Unit = new SelectList(db.Units.ToList(), "Id", "Name",product.UnitId);


                return View(product);
            }



            else
                return null;
        }

        [HttpPost]
        public IActionResult Save (Product product)
        {
            if (product != null && product.Id>0)
            {
                db = new MarketBotDbContext();
                var prod= db.Product.Where(p => p.Id == product.Id).FirstOrDefault();
                prod.ProductPrice = db.ProductPrice.Where(pr => pr.ProductId == prod.Id && pr.Enabled == true).Include(pr => pr.Currency).OrderByDescending(pr => pr.Id).ToList();
                prod.Name = product.Name;
                prod.CategoryId = product.CategoryId;
                prod.TelegraphUrl = product.TelegraphUrl;
                prod.Enable = product.Enable;
                prod.PhotoUrl = product.PhotoUrl;
                prod.Text = product.Text;
                prod.UnitId = product.UnitId;

                // Проверям изменил ли пользователь цену или тип валюты.  Если изменил то добавляем новую запись в БД
                if (product.ProductPrice.FirstOrDefault().Value != prod.ProductPrice.FirstOrDefault().Value ||
                    product.ProductPrice.FirstOrDefault().CurrencyId != prod.ProductPrice.FirstOrDefault().CurrencyId)
                {
                    product.ProductPrice.FirstOrDefault().ProductId = prod.ProductPrice.FirstOrDefault().ProductId;
                    ProductPriceInsert(product.ProductPrice.FirstOrDefault());
                    DisablePrice(prod.ProductPrice.FirstOrDefault());
                }
                db.SaveChanges();
            }

            if (product != null && product.Id == 0)
               ProductInsert(product);
                
            
            return RedirectToAction("Index");
        }

        public Product ProductInsert (Product product)
        {
            if (db == null)
                db = new MarketBotDbContext();

            if (product!=null && product.ProductPrice.FirstOrDefault() != null)
            {
                product.ProductPrice.FirstOrDefault().Enabled = true;
                product.ProductPrice.FirstOrDefault().DateAdd = DateTime.Now;
            }

            if (product != null)
            {
                product.DateAdd = DateTime.Now;
                db.Product.Add(product);
                db.SaveChanges();
            }

            return product;
        }

        /// <summary>
        /// Добавить новую цену на товар
        /// </summary>
        /// <param name="NewPrice"></param>
        /// <param name="OldPrice"></param>
        /// <returns></returns>
        private ProductPrice ProductPriceInsert(ProductPrice NewPrice)
        {
            if (NewPrice.Value > 0)
            {
                NewPrice.DateAdd = DateTime.Now;
                NewPrice.Enabled = true;
                NewPrice.Volume = 1;
                db.ProductPrice.Add(NewPrice);
                db.SaveChanges();
            }

            return NewPrice;
        }

        /// <summary>
        /// Диактивировать цену на товар
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        private int DisablePrice (ProductPrice price)
        {
            if (price != null && price.Id > 0)
            {
                price.Enabled = false;
                db.Entry(price).State = EntityState.Modified;
                return db.SaveChanges();
            }

            else
                return -1;
        }

        [HttpPost]

        public IActionResult PhotoInsert(IFormFile file)
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Content("file not selected");


            return RedirectToAction("Files");
        }
    }
}