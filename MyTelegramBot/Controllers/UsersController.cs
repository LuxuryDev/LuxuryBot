﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MyTelegramBot.Controllers
{
    public class UsersController : Controller
    {
        MarketBotDbContext db;

        public IActionResult Index()
        {
            db = new MarketBotDbContext();

            var list = db.Follower.ToList();

            return View(list);
        }
    }
}