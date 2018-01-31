using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.EntityFrameworkCore;
using MyTelegramBot.Bot;
using MyTelegramBot.Messages.Admin;
using MyTelegramBot.Messages;
using MyTelegramBot.Bot.AdminModule;

namespace MyTelegramBot.Messages.Admin
{

    public class AdminPanelCmdMessage:Bot.BotMessage
    {
        private InlineKeyboardCallbackButton EditProductBtn { get; set; }

        private InlineKeyboardCallbackButton EditCategoryBtn { get; set; }

        private InlineKeyboardCallbackButton NoConfirmOrdersBtn { get; set; }

        private InlineKeyboardCallbackButton ContactEditPanelBtn { get; set; }

        private InlineKeyboardCallbackButton PaymentsEnableListBtn { get; set; }

        private InlineKeyboardCallbackButton StockViewBtn { get; set; }
        private MyTelegramBot.Admin Admin { get; set; }

        private int FollowerId { get; set; }

        private int AdminId { get; set; }
        public AdminPanelCmdMessage(int FollowerId)
        {
            this.FollowerId = FollowerId;
        }

        public AdminPanelCmdMessage BuildMessage()
        {


                EditProductBtn = new InlineKeyboardCallbackButton("Изменить товар"+ " \ud83d\udd8a", BuildCallData(AdminBot.ProductEditCmd, AdminBot.ModuleName));
                EditCategoryBtn = new InlineKeyboardCallbackButton("Изменить категорию"+ " \ud83d\udd8a", BuildCallData(AdminBot.CategoryEditCmd, CategoryEditBot.ModuleName));
                ContactEditPanelBtn= new InlineKeyboardCallbackButton("Изменить контактные данные"+ " \ud83d\udd8a", BuildCallData(AdminBot.ContactEditCmd, AdminBot.ModuleName));
                NoConfirmOrdersBtn = new InlineKeyboardCallbackButton("Показать необработанные заказы" + " \ud83d\udcd2", BuildCallData(AdminBot.NoConfirmOrderCmd, AdminBot.ModuleName));
                PaymentsEnableListBtn = new InlineKeyboardCallbackButton("Выбрать доступные методы оплаты" + " \ud83d\udcb0", BuildCallData(AdminBot.PayMethodsListCmd, AdminBot.ModuleName));
                StockViewBtn = new InlineKeyboardCallbackButton("Посмотреть остатки", BuildCallData("ViewStock", AdminBot.ModuleName));
            base.TextMessage = Bold("Панель администратора") + NewLine() +
                               "1) Импорт новых товаров из CSV файла /import" + NewLine() +
                               "2) Экспорт всех заказов в CSV файл /export" + NewLine() +
                               "3) Экспорт истории изменения остатков /stockexport" + NewLine() +
                               "4) Добавить новый товар /newprod" + NewLine() +
                               "5) Создать новую категорию /newcategory" + NewLine() +
                               "6) Выбрать доступные способы оплаты /paymethods" + NewLine() +
                               "7) Статистика /stat" + NewLine() +
                               "8) Список операторов / Добавить нового / Удалить /operators" + NewLine() +
                               "9) Список доступных городов /cities" + NewLine()+
                               "10) Бот рассылает уведомления в ЛС. Что бы выключить нажмите /off , что бы включить нажмите /on";

                SetInlineKeyBoard();
                return this;

        }

        private void SetInlineKeyBoard()
        {
            
                base.MessageReplyMarkup = new InlineKeyboardMarkup(
                    new[]{
                        new[]
                        {
                            StockViewBtn
                        },
                new[]
                        {
                            EditProductBtn
                        },
                new[]
                        {
                            EditCategoryBtn,
                        },

                new[]
                        {
                            NoConfirmOrdersBtn
                        },
                new[]
                        {
                            ContactEditPanelBtn
                        },

                     });
        }
    }


}
