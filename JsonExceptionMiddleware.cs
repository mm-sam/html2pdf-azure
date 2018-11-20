using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;

namespace code
{
    public class JsonExceptionMiddleware
    {
        protected IHostingEnvironment env;
        public JsonExceptionMiddleware(IHostingEnvironment _env)
        {
            env = _env;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            if (ex == null) return;

            var error = new
            {
                Message = "API Error",
                Detail = ex.Message
            };

            if (env.IsDevelopment())
            {
                error = new
                {
                    Message = ex.Message,
                    Detail = ex.StackTrace
                };
            }

            context.Response.ContentType = "application/json";

            using (var writer = new StreamWriter(context.Response.Body))
            {
                new JsonSerializer().Serialize(writer, error);
                await writer.FlushAsync().ConfigureAwait(false);
            }
        }
    }
}