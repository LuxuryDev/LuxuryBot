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


        public IActionResult Litecoin()
        {
            db = new MarketBotDbContext();

            PaymentTypeEnum =  PaymentTypeEnum.Litecoin;


            PaymentTypeConfig = db.PaymentTypeConfig.Where(p => p.PaymentId == PaymentType.GetTypeId(PaymentTypeEnum)).OrderByDescending(p => p.Id).FirstOrDefault();

            if (PaymentTypeConfig == null)
            {
                PaymentTypeConfig = new PaymentTypeConfig
                {
                    Host = "127.0.0.1",
                    Login = "root",
                    Pass = "toor",
                    Port = "9332",
                    Enable = true,
                    PaymentId = PaymentType.GetTypeId(PaymentTypeEnum)
                };

            }

            ViewBag.Title = "Litecoin";
            ViewBag.Text = "В папке с установленными Litecoin Core создайте бат файл.Сохраните и запустите этот бат файл и дождитесь синхронизации базы данных." +
                "Содержимое бат файла: litecoin-qt.exe -server -rest -rpcuser=root -rpcpassword=toor -rpcport=9332";
            return View("CryptoCurrency", PaymentTypeConfig);
        }

        /// <summary>
        /// Сохранить изменения
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save([FromBody] PaymentTypeConfig config)
        {
            if(config!=null)
                PaymentTypeEnum = PaymentType.GetPaymentTypeEnum(config.PaymentId);

            if (config != null && config.Login == ""  && PaymentTypeEnum == PaymentTypeEnum.Qiwi)
                return Json("Заполните поле Номер телефона");

            if (config != null && config.Pass == "" && PaymentTypeEnum == PaymentTypeEnum.Qiwi)
                return Json("Заполните поле Токен доступа");

            if (config != null && config.Pass == "" && PaymentTypeEnum == PaymentTypeEnum.Litecoin)
                return Json("Заполните поле Пароль");

            if (config != null && config.Login == "" && PaymentTypeEnum == PaymentTypeEnum.Litecoin)
                return Json("Заполните поле Логин");

            if (config != null && config.Host == "" && PaymentTypeEnum == PaymentTypeEnum.Litecoin)
                return Json("Заполните поле Адрес RPC сервера");

            if (config != null && config.Port == "" && PaymentTypeEnum == PaymentTypeEnum.Litecoin)
                return Json("Заполните поле Порт RPC сервера");

            if (config != null && SaveChanges(config)>=0)
                return Json("Сохранено");
            
            else
                return Json("Ошибка"); 
        }

        /// <summary>
        /// Сохранить изменения в бд
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private int SaveChanges(PaymentTypeConfig config)
        {
            if (db == null)
                db = new MarketBotDbContext();

            if (config != null && config.Id > 0) // Есле уже сущестуюущая запись, то редактируем ее
            {
                var OldCgf = db.PaymentTypeConfig.Where(c => c.Id == config.Id).FirstOrDefault();

                if (OldCgf != null)
                {
                    OldCgf.Login = config.Login;
                    OldCgf.Pass = config.Pass;
                    OldCgf.Enable = config.Enable;
                    OldCgf.Host = config.Host;
                    OldCgf.Port = config.Port;
                    return EnablePaymentType(config.Enable, Convert.ToInt32(config.PaymentId));
                    
                }

                else
                    return 0;
            }

            if(config!=null && config.Id == 0) // Если это новая запись то добавляем ее в бд
            {
                config.TimeStamp = DateTime.Now;
                db.PaymentTypeConfig.Add(config);
                return EnablePaymentType(config.Enable, Convert.ToInt32(config.PaymentId));
            }

            else
                return -1;
        }


        /// <summary>
        /// Включить/отключить метод оплаты
        /// </summary>
        /// <param name="value"></param>
        /// <param name="typePaymentId"></param>
        /// <returns></returns>
        private int EnablePaymentType(bool value, int typePaymentId)
        {
            if (db == null)
                db = new MarketBotDbContext();

            var payment = db.PaymentType.Where(p => p.Id == typePaymentId).FirstOrDefault();

            if (payment != null)
            {
                payment.Enable = value;
                return db.SaveChanges();
            }

            else
                return -1;

        }

        /// <summary>
        /// Проверка соединения с платежной системой
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestConnection([FromBody] PaymentTypeConfig config)
        {

            if (config != null)
                PaymentTypeEnum = PaymentType.GetPaymentTypeEnum(config.PaymentId);

            if (PaymentTypeEnum == PaymentTypeEnum.Qiwi)
            {
                if (await Services.Qiwi.QiwiFunction.TestConnection(config.Login, config.Pass))
                    return new JsonResult("Успех");

                else
                    return new JsonResult("Ошибка соединения");
            }

            if (PaymentTypeEnum == PaymentTypeEnum.Litecoin)
            {
                string TxHash = "031e28733afd26153e91afd6132de1a039510597df22d00977d119da7391ea16";
                Services.BitCoinCore.Litecoin ltc = new Services.BitCoinCore.Litecoin(config.Login, config.Pass, config.Host, config.Port);
                var block= ltc.GetTxnInfo<Services.BitCoinCore.TransactionInfo>(TxHash);

                if(block!=null && block.result.blockindex>0)
                    return new JsonResult("Успех");

                else
                    return new JsonResult("Ошибка соединения");
            }

            else
                return new JsonResult("Ошибка соединения");
        }


    }
}