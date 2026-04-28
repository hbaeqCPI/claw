using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Web.Extensions.ActionResults
{
    public class JsonBadRequest : JsonResult
    {

        public JsonBadRequest(string message) : base(message)
        {
            this.Value = message;
        }

        public JsonBadRequest(object value) : base(value)
        {
            this.Value = value;
        }
        public override async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.StatusCode = 400;

            response.ContentType = !String.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

            if (this.Value == null)
                return;
           
            var responseString = JsonConvert.SerializeObject(this.Value);
            var responseByteArray = Encoding.UTF8.GetBytes(responseString);
            await response.Body.WriteAsync(responseByteArray, 0, responseByteArray.Length);
            await response.Body.FlushAsync();

        }
    }
}
