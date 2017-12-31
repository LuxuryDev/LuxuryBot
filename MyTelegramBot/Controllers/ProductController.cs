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

        Product Product { get; set; }

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


                product.ProductPhoto = db.ProductPhoto.Where(photo => photo.ProductId == product.Id).Include(photo=>photo.AttachmentFs).ToList();

                product.ProductPrice.Add(db.ProductPrice.Where(price => price.ProductId == product.Id && price.Enabled == true).OrderByDescending(price => price.Id).FirstOrDefault());
                if (product.ProductPhoto.FirstOrDefault() != null)
                {
                    string imageBase64Data = Convert.ToBase64String(product.ProductPhoto.FirstOrDefault().AttachmentFs.Fs);
                    string imageDataURL = string.Format("data:image/png;base64,{0}", imageBase64Data);
                    ViewBag.ImageData = imageDataURL;
                }


                ViewBag.Category = new SelectList(db.Category.Where(c=>c.Enable).ToList(), "Id", "Name",product.CategoryId);
                //ViewBag.Currency = new SelectList(db.Currency.ToList(), "Id", "Name");
                ViewBag.Currency = db.Currency.ToList();
                ViewBag.Unit = new SelectList(db.Units.ToList(), "Id", "Name",product.UnitId);

                if (product != null)
                    return View(product);

                else
                    return NoContent();
            }



            else
                return null;
        }

        [HttpPost]
        public IActionResult Save (Product SaveProduct)
        {
            db = new MarketBotDbContext();
            
            if (SaveProduct != null && SaveProduct.Id > 0)
            {
                Product = db.Product.Where(p => p.Id == SaveProduct.Id).FirstOrDefault();
                Product.ProductPrice = db.ProductPrice.Where(pr => pr.ProductId == Product.Id && pr.Enabled == true).
                    Include(pr => pr.Currency).OrderByDescending(pr => pr.Id).ToList();
            }

            //Редактируется уже сущестуюий товар. Перед этим проверятся изменилось ли имя, если изменилось,
            //то мы проверям не занято ли оно
            if (SaveProduct != null && SaveProduct.Id>0 && SaveProduct.Name!=null && Product != null && Product.Name==SaveProduct.Name ||
                SaveProduct != null && SaveProduct.Id > 0 && SaveProduct.Name != null && Product != null 
                && Product.Name != SaveProduct.Name && CheckName(SaveProduct.Name))
            {
                Product.Name = SaveProduct.Name;
                Product.CategoryId = SaveProduct.CategoryId;
                Product.TelegraphUrl = SaveProduct.TelegraphUrl;
                Product.Enable = SaveProduct.Enable;
                Product.PhotoUrl = SaveProduct.PhotoUrl;
                Product.Text = SaveProduct.Text;
                Product.UnitId = SaveProduct.UnitId;

                // Проверям изменил ли пользователь цену или тип валюты.  Если изменил то добавляем новую запись в БД
                if (SaveProduct.ProductPrice.FirstOrDefault().Value != Product.ProductPrice.FirstOrDefault().Value 
                    && SaveProduct.ProductPrice.FirstOrDefault().Value >0 ||
                    SaveProduct.ProductPrice.FirstOrDefault().CurrencyId != Product.ProductPrice.FirstOrDefault().CurrencyId
                    && SaveProduct.ProductPrice.FirstOrDefault().Value > 0)
                {
                    SaveProduct.ProductPrice.FirstOrDefault().ProductId = Product.ProductPrice.FirstOrDefault().ProductId;
                    ProductPriceInsert(SaveProduct.ProductPrice.FirstOrDefault());
                    DisablePrice(Product.ProductPrice.FirstOrDefault());
                }
                db.SaveChanges();
            }

            ///добавление нового товара
            if ( SaveProduct != null && SaveProduct.Name!=null && SaveProduct.Id == 0 && CheckName(SaveProduct.Name))
               ProductInsert(SaveProduct);

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Добавить новый товар в базу данных
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public Product ProductInsert (Product product)
        {
            if (db == null)
                db = new MarketBotDbContext();

            if (product!=null && product.ProductPrice.FirstOrDefault() != null)
            {
                product.ProductPrice.FirstOrDefault().Enabled = true;
                product.ProductPrice.FirstOrDefault().DateAdd = DateTime.Now;
            }

            if(product!=null && product.Stock.FirstOrDefault() != null)
            {
                product.Stock.FirstOrDefault().DateAdd = DateTime.Now;
                product.Stock.FirstOrDefault().Quantity =Convert.ToInt32(product.Stock.FirstOrDefault().Balance);
                product.Stock.FirstOrDefault().Text = "Добавление нового товара";
            }

            if (product != null )
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

        /// <summary>
        /// Проверяем занято ли имяю Если занято то возращает FALSE
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
       public bool CheckName (string name)
        {
            if (db == null)
                db = new MarketBotDbContext();

            if (db.Product.Where(p => p.Name == name).FirstOrDefault() != null)
                return false;

            else
                return true;
        }
    }
}