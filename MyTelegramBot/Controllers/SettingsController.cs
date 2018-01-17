using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using Newtonsoft.Json;
namespace MyTelegramBot.Controllers
{
    [Produces("application/json")]
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            //"Server=localhost;Database=MarketBotDb;Integrated Security = FALSE;USER ID=bot;PASSWORD=bot;Trusted_Connection = True;"

            var builder = new ConfigurationBuilder()
               .SetBasePath(System.IO.Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json");
            string sql = builder.Build().GetSection("Database").Value;

            if (sql == "")
            {
                Model.SettingsDbConnection settingsDb = new Model.SettingsDbConnection
                {
                    DbName = "MarketBotDb",
                    Host = "localhost",
                    Port = "1433",
                    UserName = "",
                    Password = ""
                };

                return View(settingsDb);
            }

            else
            {
                return View();
            }

            
        }

        [HttpPost]
       
        public IActionResult Update([FromBody] Model.SettingsDbConnection settings)
        {
            if (settings != null && settings.DbName!=null && settings.Host!=null)
            {
                //открываем файл sql.json и сохраняем изменения

                using (StreamWriter sw=new StreamWriter("sql.json"))
                {
                    sw.Write(JsonConvert.SerializeObject(settings));
                    sw.Flush();
                    sw.Dispose();
                }

                //"Server=localhost;Database=MarketBotDb;Integrated Security = FALSE;USER ID=bot;PASSWORD=bot;Trusted_Connection = True;"

                string SqlConnection = "Server=" + settings.Host+ ";Database=" + settings.DbName + ";Integrated Security = FALSE;Trusted_Connection = True;";

                using (StreamWriter sw = new StreamWriter("connection.json"))
                {
                    sw.Write(SqlConnection);
                    sw.Flush();
                    sw.Dispose();
                }

                if (TestConnection())
                  return  Json("Успех!");

                else
                    return Json("Ошибка подключения");
            }

            else
                return Json("Ошибка ввода данных");
        }

        private bool TestConnection()
        {
            try
            {
                using (MarketBotDbContext db = new MarketBotDbContext())
                {
                    var list= db.Units.ToList();

                    return true;

                }
            }

            catch
            {
                return false;
            }
        }
    }



    
    
    
}