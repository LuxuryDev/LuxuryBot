using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                p.ProductPrice = db.ProductPrice.Where(pr => pr.ProductId == pr.ProductId && pr.Enabled == true).Include(pr=>pr.Currency).OrderByDescending(pr => pr.Id).ToList();            

            return View(products);
        }

        [HttpGet]
        public IActionResult Editor (int id)
        {
            if (id > 0)
            {
                db = new MarketBotDbContext();

                var product = db.Product.Where(p => p.Id == id).Include(p => p.Unit).Include(p=>p.Category).Include(p => p.ProductPrice).Include(p => p.ProductPrice).FirstOrDefault();

                product.ProductPhoto = db.ProductPhoto.Where(photo => photo.ProductId == product.Id).Include(photo => photo.AttachmentFs).ToList();
                string imageBase64Data = Convert.ToBase64String(product.ProductPhoto.FirstOrDefault().AttachmentFs.Fs);
                string imageDataURL = string.Format("data:image/png;base64,{0}", imageBase64Data);
                ViewBag.ImageData = imageDataURL;

                return View(product);
            }

            else
                return null;
        }
    }
}