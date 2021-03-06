﻿using System;
using CookBook.CoreProject.Interfaces;
using CookBook.Presentation.Helpers;
using Microsoft.AspNetCore.Http;

namespace CookBook.Presentation.Cookies
{
    public class CookieService : ICookieService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public CookieService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public void WriteHttpOnlyCookie(string key, string value, DateTime? expiryDate = null)
        {
            _contextAccessor.HttpContext.AppendHttpOnlyCookie(key, value, expiryDate);
        }

        public bool TryGetCookie(string key, out string value)
        {
            return _contextAccessor.HttpContext.TryGetCookie(key, out value);
        }
    }
}
