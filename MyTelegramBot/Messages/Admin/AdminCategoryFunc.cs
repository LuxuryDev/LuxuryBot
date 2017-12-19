using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using MyTelegramBot.Bot;
using MyTelegramBot.Bot.AdminModule;

namespace MyTelegramBot.Messages.Admin
{
    /// <summary>
    /// Сообщение с админскими функциями для редактирования категории
    /// </summary>
    public class AdminCategoryFuncMessage:BotMessage
    {
        private InlineKeyboardCallbackButton EditCategoryNameBtn { get; set; }

        private InlineKeyboardCallbackButton EditCategoryEnableBtn { get; set; }

        private InlineKeyboardCallbackButton BackToAdminPanelBtn { get; set; }

        private Category Category { get; set; }

        private int CategoryId { get; set; }

        public AdminCategoryFuncMessage(int CategoryId)
        {
            this.CategoryId = CategoryId;
        }

        public AdminCategoryFuncMessage BuildMessage()
        {
            using (MarketBotDbContext db=new MarketBotDbContext())
                Category = db.Category.Where(c => c.Id == CategoryId).FirstOrDefault();
            

            if (Category != null)
            {
                EditCategoryNameBtn = new InlineKeyboardCallbackButton("Изменить название", BuildCallData(Bot.CategoryEditBot.CategoryEditNameCmd, Bot.CategoryEditBot.ModuleName, CategoryId));

                if(Category.Enable)
                    EditCategoryEnableBtn = new InlineKeyboardCallbackButton("Скрыть от пользователя", BuildCallData(Bot.CategoryEditBot.CategoryEditEnableCmd, Bot.CategoryEditBot.ModuleName, CategoryId));

                else
                    EditCategoryEnableBtn = new InlineKeyboardCallbackButton("Показывать пользователям", BuildCallData(Bot.CategoryEditBot.CategoryEditEnableCmd, Bot.CategoryEditBot.ModuleName, CategoryId));

                BackToAdminPanelBtn = new InlineKeyboardCallbackButton("Панель Администратора", BuildCallData(AdminBot.BackToAdminPanelCmd, Bot.AdminModule.AdminBot.ModuleName));

                SetInlineKeyBoard();               

                base.TextMessage =Category.Name+ " выберите действие";
                
            }

            return this;
        }

        private void SetInlineKeyBoard()
        {
                base.MessageReplyMarkup = new InlineKeyboardMarkup(
                    new[]{
                new[]
                        {
                            EditCategoryNameBtn
                        },
                new[]
                        {
                            EditCategoryEnableBtn
                        },

                new[]
                        {
                            BackToAdminPanelBtn
                        },

                     });


        }
    }
}
