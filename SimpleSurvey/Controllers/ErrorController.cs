﻿// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.Web.Mvc;

namespace SimpleSurvey.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Index(string message, string debug)
        {
            ViewBag.Message = message;
            ViewBag.Debug = debug;
            return View("Error");
        }
    }
}