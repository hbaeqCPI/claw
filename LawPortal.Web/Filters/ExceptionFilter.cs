using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using LawPortal.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LawPortal.Core.Interfaces;
using System.Data.SqlClient;
using LawPortal.Core.Entities;
using System.Text.Encodings.Web;

namespace LawPortal.Web.Filters
{
    public class ExceptionFilter:ExceptionFilterAttribute
    {
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;
        private readonly ICPiDbContext _cpiDbContext;

        public ExceptionFilter(IStringLocalizer<SharedResource> sharedLocalizer, ICPiDbContext cpiDbContext)
        {
            _sharedLocalizer = sharedLocalizer;
            _cpiDbContext = cpiDbContext;
        }               

        public override async Task OnExceptionAsync(ExceptionContext context)
        {
            var exception = context.Exception;
            var errorMsg = "";
            var errorMsg2 = "";
            var statusCode = 500;

            if (exception is DbUpdateConcurrencyException)
            {
                //errorMsg = "Save operation cancelled. The record that you want to edit was just modified by another user.";
                //generic message to include delete
                errorMsg = "Unable to perform operation because the record has been modified by another user.";
            }
            
            else if (exception is DbUpdateException || exception is SqlException || exception is Microsoft.Data.SqlClient.SqlException) {
                string innerExceptionMsg;

                if (exception is SqlException || exception is Microsoft.Data.SqlClient.SqlException)
                    innerExceptionMsg = exception.Message;
                else
                    innerExceptionMsg = exception.InnerException.Message.Replace("The transaction ended in the trigger. The batch has been aborted.", "");

                if (innerExceptionMsg.ToLower().Contains("cannot insert duplicate key"))
                {
                    errorMsg = "Unable to save record as this will create duplicate entries in the database.";
                }
                else if (innerExceptionMsg.ToLower().Contains("the delete statement conflicted with the reference constraint"))
                {
                    errorMsg = "Record cannot be deleted, it is already in use.";
                    errorMsg2 = GetTableNameForDisplay(innerExceptionMsg);
                }
                else if (innerExceptionMsg.ToLower().Contains("statement conflicted with the foreign key constraint")) {
                    var errorString = innerExceptionMsg.Substring(0, innerExceptionMsg.IndexOf('.'));
                    errorMsg = await GetErrorMessage(errorString.ToLower());
                }
                else if (innerExceptionMsg.ToLower().Contains("statement conflicted with the reference constraint"))
                {
                    errorMsg = "Record cannot be modified, it is already in use.";
                }
                else if (innerExceptionMsg.ToLower().Contains("record cannot be disabled"))
                {
                    errorMsg = "Record cannot be disabled, it is already in use.";
                    errorMsg2 = GetTableNameForDisplay(innerExceptionMsg);
                }
                else
                {
                    errorMsg = innerExceptionMsg;
                }
            }
            else if (exception is Microsoft.Graph.ServiceException)
            {
                var graphServiceException = ((Microsoft.Graph.ServiceException)exception);
                statusCode = (int)graphServiceException.StatusCode;
                if (graphServiceException.StatusCode == System.Net.HttpStatusCode.NotFound)
                    errorMsg = "Item not found or has been deleted.";
                else
                    errorMsg = graphServiceException.Error.Message;
            }
            else
            {
                JavaScriptEncoder jsEncoder = JavaScriptEncoder.Default; //mitigate xss 
                errorMsg = jsEncoder.Encode(exception.Message);
            }

            if (!string.IsNullOrEmpty(errorMsg))
            {
                errorMsg = _sharedLocalizer[errorMsg] + errorMsg2;
                if (statusCode == 500 && errorMsg.ToLower().Contains("access token"))
                    statusCode = 401;

                var result = new Microsoft.AspNetCore.Mvc.ContentResult();
                result.Content = errorMsg;
                context.Result = result;
                context.HttpContext.Response.StatusCode = statusCode;
                
            }

            base.OnException(context);
        }

        private async Task<string> GetErrorMessage(string errorMessage)
        {
            var error = await _cpiDbContext.GetReadOnlyRepositoryAsync<ErrorMapping>().QueryableList.FirstOrDefaultAsync(e => errorMessage.Contains(e.Message));
            if (error != null)
                return error.Key;

            return errorMessage;
        }

        private string GetTableNameForDisplay(string errorMessage)
        {
            var expression = @"(?i)(?s)table ""(.*?)""";
            var m = Regex.Match(errorMessage.ToLower(), expression); //get the table name
            var value = m.Groups[1].Value.ToLower();

            string[] pattern = { "dbo.tblpat", "dbo.tbltmk", "dbo.tblgm", "dbo.tbldms", "dbo.tblpl", "dbo.tbltl", "dbo.tbl" };
            string result = Regex.Replace(value, string.Join("|", pattern.Select(item => $"(?:{item})")), ""); //remove the pattern above
            
            return  string.IsNullOrEmpty(result) ? "" : " [" + char.ToUpper(result[0]) + result.Substring(1) + "]";
        }
    }
}
