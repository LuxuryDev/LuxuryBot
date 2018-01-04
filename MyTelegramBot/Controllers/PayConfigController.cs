using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyTelegramBot.Services;
namespace MyTelegramBot.Controllers
{
    [Produces("application/json")]
    public class PayConfigController : Controller
    {
        PaymentTypeConfig PaymentTypeConfig;

        PaymentTypeEnum PaymentTypeEnum;

        MarketBotDbContext db;


        public IActionResult Qiwi()
        {
            db = new MarketBotDbContext();

            PaymentTypeEnum = PaymentTypeEnum.Qiwi;


            PaymentTypeConfig = db.PaymentTypeConfig.Where(p => p.PaymentId == PaymentType.GetTypeId(PaymentTypeEnum)).OrderByDescending(p => p.Id).FirstOrDefault();

            if (PaymentTypeConfig == null)
            {
                PaymentTypeConfig = new PaymentTypeConfig
                {
                    Host = "https://qiwi.com/api",
                    Login = "",
                    Pass = "",
                    Port = "80",
                    Enable = true,
                    PaymentId = PaymentType.GetTypeId(PaymentTypeEnum)
                };

            }

            return View("Qiwi", PaymentTypeConfig);
        }

        [HttpPost]
        public IActionResult Save(PaymentTypeConfig config)
        {
            if (config != null)
            {
                return RedirectToAction("Index", "Home");
            }

            else
                return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestConnection([FromBody] PaymentTypeConfig config)
        {

            if (PaymentType.GetPaymentTypeEnum(config.PaymentId) == PaymentTypeEnum.Qiwi)
            {
                if (await Services.Qiwi.QiwiFunction.TestConnection(config.Login, config.Pass))
                    return new JsonResult("Успех");

                else
                    return new JsonResult("Ошибка соединения");
            }

            else
            return new JsonResult("Hello Response Back");
        }

        [HttpPost]
        public JsonResult Upload()
        {
           
            return Json("файл загружен");
        }
    }
}