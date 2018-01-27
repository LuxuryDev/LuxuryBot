using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using Newtonsoft.Json;
using System.Web;
using Microsoft.EntityFrameworkCore;
using MyTelegramBot.Bot;
using MyTelegramBot.Bot.AdminModule;

namespace MyTelegramBot.Messages.Admin
{
    /// <summary>
    /// Сообщение с админскими функциями Товара
    /// </summary>
    public class AdminProductFuncMessage:BotMessage
    {
        private int ProductId { get; set; }

        private InlineKeyboardCallbackButton ProductEditNameBtn { get; set; }

        private InlineKeyboardCallbackButton ProductEditCategoryBtn { get; set; }

        private InlineKeyboardCallbackButton ProductEditPriceBtn { get; set; }

        private InlineKeyboardCallbackButton ProductEditStockBtn { get; set; }

        private InlineKeyboardCallbackButton ProductEditEnableBtn { get; set; }

        private InlineKeyboardCallbackButton ProductEditTextBtn { get; set; }

        private InlineKeyboardCallbackButton ProductEditPhotoBtn { get; set; }

        private InlineKeyboardCallbackButton ProductEditUrlBtn { get; set; }

        private InlineKeyboardCallbackButton AdminPanelBtn { get; set; }

        private InlineKeyboardCallbackButton OpenProductBtn { get; set; }

        //private InlineKeyboardCallbackButton CurrencyBtn { get; set; }

        private InlineKeyboardCallbackButton UnitBtn { get; set; }

        private InlineKeyboardCallbackButton InlineImageBtn { get; set; }

        private Product Product { get; set; }

        public AdminProductFuncMessage(int ProductId)
        {
            this.ProductId = ProductId;
            
        }

        public AdminProductFuncMessage BuildMessage()
        {
            int? Quantity = 0;

            using(MarketBotDbContext db=new MarketBotDbContext())
                Product=db.Product.Where(p => p.Id == ProductId).Include(p => p.Category).Include(p => p.ProductPrice).Include(p=>p.Unit).Include(p => p.Stock).FirstOrDefault();
            

            if (Product != null && Product.Stock != null && Product.Stock.Count > 0)
                Quantity = Product.Stock.OrderByDescending(s => s.Id).FirstOrDefault().Balance;

            if (Product != null)
            {
                // base.TextMessage = product.ToString()+ " - "+ Quantity.ToString()+" шт.";

              

                base.BackBtn = new InlineKeyboardCallbackButton("Назад", BuildCallData(AdminBot.AdminProductInCategoryCmd, AdminBot.ModuleName, Product.CategoryId));

                ProductEditNameBtn = new InlineKeyboardCallbackButton("Название", BuildCallData(ProductEditBot.ProductEditNameCmd, ProductEditBot.ModuleName, ProductId));

                ProductEditCategoryBtn = new InlineKeyboardCallbackButton("Категория", BuildCallData(ProductEditBot.ProductEditCategoryCmd, ProductEditBot.ModuleName, ProductId));

                ProductEditPriceBtn = new InlineKeyboardCallbackButton("Стоимость", BuildCallData(ProductEditBot.ProductEditPriceCmd, ProductEditBot.ModuleName, ProductId));

                ProductEditStockBtn = new InlineKeyboardCallbackButton("Остаток", BuildCallData(ProductEditBot.ProductEditStockCmd, ProductEditBot.ModuleName, ProductId));

                ProductEditTextBtn = new InlineKeyboardCallbackButton("Описание", BuildCallData(ProductEditBot.ProductEditTextCmd, ProductEditBot.ModuleName, ProductId));

                ProductEditPhotoBtn = new InlineKeyboardCallbackButton("Фотография", BuildCallData(ProductEditBot.ProductEditPhotoCmd, ProductEditBot.ModuleName, ProductId));

                ProductEditUrlBtn = new InlineKeyboardCallbackButton("Заметка", BuildCallData(ProductEditBot.ProductEditUrlCmd, ProductEditBot.ModuleName, ProductId));

                AdminPanelBtn = new InlineKeyboardCallbackButton("Панель администратора", BuildCallData(AdminBot.BackToAdminPanelCmd,AdminBot.ModuleName));

                UnitBtn = new InlineKeyboardCallbackButton("Ед.изм.", BuildCallData(ProductEditBot.ProudctUnitCmd, ProductEditBot.ModuleName, ProductId));

                //CurrencyBtn = new InlineKeyboardCallbackButton("Валюта", BuildCallData(ProductEditBot.ProudctCurrencyCmd, ProductEditBot.ModuleName, ProductId));

                InlineImageBtn = new InlineKeyboardCallbackButton("Фото в Inline", BuildCallData(ProductEditBot.ProductInlineImageCmd, ProductEditBot.ModuleName, ProductId));

                base.TextMessage = Product.AdminMessage();

                if (Product.Enable == true)
                    ProductEditEnableBtn = new InlineKeyboardCallbackButton("Скрыть от пользователей", BuildCallData(ProductEditBot.ProductEditEnableCmd, ProductEditBot.ModuleName, ProductId));

                else
                    ProductEditEnableBtn = new InlineKeyboardCallbackButton("Показывать пользователям", BuildCallData(ProductEditBot.ProductEditEnableCmd, ProductEditBot.ModuleName, ProductId));

                if (Product.Enable == true)
                    OpenProductBtn = new InlineKeyboardCallbackButton("Открыть", BuildCallData(ProductBot.GetProductCmd, ProductBot.ModuleName, Product.Id));

                       

                SetInlineKeyBoard();
            }

            return this;
        }

        private void SetInlineKeyBoard()
        {
            if(OpenProductBtn==null)
            base.MessageReplyMarkup = new InlineKeyboardMarkup(
                new[]{
                new[]
                        {
                            ProductEditNameBtn, ProductEditCategoryBtn
                        },
                new[]
                        {
                            ProductEditPriceBtn, ProductEditStockBtn
                        },

                new[]
                        {
                            ProductEditTextBtn, ProductEditPhotoBtn,UnitBtn
                        },

                new[]
                        {
                            ProductEditEnableBtn, ProductEditUrlBtn,InlineImageBtn
                        },
                new[]
                        {
                            ProductEditEnableBtn
                        },
                new[]
                         {
                            AdminPanelBtn
                         }
                ,
                new[]
                        {
                            BackBtn
                        }

                 });

            else
                base.MessageReplyMarkup = new InlineKeyboardMarkup(
                new[]{
                new[]   {
                            OpenProductBtn
                        },
                new[]
                        {
                            ProductEditNameBtn, ProductEditCategoryBtn
                        },
                new[]
                        {
                            ProductEditPriceBtn, ProductEditStockBtn
                        },

                new[]
                        {
                            ProductEditTextBtn, ProductEditPhotoBtn,UnitBtn
                        },

                new[]
                        {
                            ProductEditEnableBtn, ProductEditUrlBtn,InlineImageBtn
                        },

                new[]
                         {
                            AdminPanelBtn
                         }
                ,
                new[]
                        {
                            BackBtn
                        }

     });

        }
    }
}
