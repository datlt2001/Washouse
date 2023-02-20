using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Washouse.Service.Interface;
using System.Data.Entity.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Web.Infrastructure
{
    public class ErrorLogger
    {
        private IErrorService _errorService;

        public ErrorLogger(IErrorService errorService)
        {
            this._errorService = errorService;
        }

        public async Task LogErrorAsync(Exception ex)
        {
            try
            {
                Error error = new Error();
                error.CreatedDate = DateTime.Now;
                error.Message = ex.Message;
                error.StackTrace = ex.StackTrace;
                await _errorService.Create(error);
                _errorService.Save();
            }
            catch
            {
            }
        }
    }
}
