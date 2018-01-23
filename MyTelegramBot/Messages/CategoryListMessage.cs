using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;
using Newtonsoft.Json;
using System.Web;
using Telegram.Bot.Types.InlineKeyboardButtons;
using MyTelegramBot.Bot.AdminModule;

namespace MyTelegramBot.Messages
{
    /// <summary>
    /// Сообщение с категориями товаров в виде кнопок
    /// </summary>
    public class CategoryListMessage:Bot.BotMessage
    {
        private string Cmd { get; set; }

        private string ModuleName { get; set; }

        private string BackCmd { get; set; }

        /// <summary>
        /// id товара которому нужно поменять категорию
        /// </summary>
        private int EditProductId { get; set; }

        private InlineKeyboardCallbackButton [][] CategoryListBtn { get; set; }

        private List<Category> Categorys { get; set; }

        private InlineKeyboardCallbackButton ViewAllBtn { get; set; }

        //флаг который показывает отображать ли кнопку "Показать все все товары"
        private bool VisableAllProductBtn { get; set; }

        /// <summary>
        /// Для меню. Показывает кнопки всех категорий и кнопку "Показать весь ассортимент"
        /// </summary>
        public CategoryListMessage()
        {
            VisableAllProductBtn = true;
            Cmd = "ProductInCategory";
            BackCmd = "MainMenu";
            BackBtn = new InlineKeyboardCallbackButton("Назад", BuildCallData(BackCmd,Bot.MainMenuBot.ModuleName));
            
        }

        /// <summary>
        /// Другое действие
        /// </summary>
        /// <param name="Cmd">Название команды</param>
        /// <param name="VisableAllProductBtn">Отображать ли кнопку "Показать все товары"</param>
        public CategoryListMessage(string Cmd, string ModuleName ,bool VisableAllProductBtn=false)
        {
            this.Cmd = Cmd;
            this.ModuleName = ModuleName;
            this.BackCmd =AdminBot.BackToAdminPanelCmd;
            BackBtn= new InlineKeyboardCallbackButton("Назад", BuildCallData(BackCmd, AdminBot.ModuleName));
        }

        /// <summary>
        /// Выбираем новое значение категории в которой будет находится товар
        /// </summary>
        /// <param name="EditProductId"></param>
        /// <param name="Cmd"></param>
        public CategoryListMessage (int EditProductId, string Cmd = Bot.ProductEditBot.ProductUpdateCategoryCmd, string ModuleName= Bot.ProductEditBot.ModuleName)
        {
            this.VisableAllProductBtn = false;
            this.EditProductId = EditProductId;
            this.Cmd = Cmd;
            this.ModuleName = ModuleName;
            this.BackCmd = "SelectProduct";
            BackBtn = new InlineKeyboardCallbackButton("Назад", BuildCallData(BackCmd,Bot.ProductEditBot.ModuleName,this.EditProductId));
        }

        public CategoryListMessage BuildMessage()
        {           
            using (MarketBotDbContext db=new MarketBotDbContext())
                Categorys=db.Category.ToList();

            if (VisableAllProductBtn)
            {

                ViewAllBtn = new InlineKeyboardCallbackButton("Показать весь ассортимент",
                        BuildCallData("ViewAllProduct", Bot.CategoryBot.ModuleName));

                CategoryListBtn = new InlineKeyboardCallbackButton[Categorys.Count + 2][];

                CategoryListBtn[Categorys.Count] = new InlineKeyboardCallbackButton[1];

                CategoryListBtn[Categorys.Count][0] = ViewAllBtn;


            }

            else
                CategoryListBtn = new InlineKeyboardCallbackButton[Categorys.Count + 1][];
         

            int count = 0;
            if (Categorys.Count > 0)
            {
                foreach (Category cat in Categorys)
                {
                    if (EditProductId > 0) // Если меняем категорию в которой находится товар. Для админа
                    {
                        InlineKeyboardCallbackButton button = new InlineKeyboardCallbackButton(cat.Name, 
                            base.BuildCallData(Cmd, ModuleName,EditProductId,cat.Id));
                        CategoryListBtn[count] = new InlineKeyboardCallbackButton[1];
                        CategoryListBtn[count][0] = button;
                    }

                    else 
                    {
                        InlineKeyboardCallbackButton button = new InlineKeyboardCallbackButton(cat.Name, 
                            base.BuildCallData(Cmd,ModuleName ,cat.Id));
                        CategoryListBtn[count] = new InlineKeyboardCallbackButton[1];
                        CategoryListBtn[count][0] = button;
                    }
                    count++;

                }

                base.TextMessage = "Выберите категорию";
                CategoryListBtn[CategoryListBtn.Length-1] = new InlineKeyboardCallbackButton[1];
                CategoryListBtn[CategoryListBtn.Length-1][0] = BackBtn;

                base.MessageReplyMarkup = new InlineKeyboardMarkup(CategoryListBtn);
            }

            else
                base.TextMessage = "Данные отсутствуют";
            
            return this;
        }


    }
}
