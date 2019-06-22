using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Api
{
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public ApiLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        /// <summary>
        /// Logging Middleware to set Request and Response bodies in context properties
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext httpContext )
        {
            try
            {

                if (httpContext.Request.Method != HttpMethods.Get) //capture body from non-GET requests only
                {
                    var requestBodyContent = await ReadRequestBody(httpContext.Request);
                    httpContext.Items["Request Body"] = requestBodyContent;

                    var originalBodyStream = httpContext.Response.Body;
                    using (var responseBody = new MemoryStream())
                    {
                        var response = httpContext.Response;
                        response.Body = responseBody;
                        await _next(httpContext);

                        if (response.Body.Length > 0)
                        {
                            string responseBodyContent = await ReadResponseBody(response);
                            await responseBody.CopyToAsync(originalBodyStream);
                            httpContext.Items["Response Body"] = responseBodyContent;
                        }
                    }
                }
                else
                {
                    await _next(httpContext);
                }
                
            }
            catch (Exception ex)
            {
                await _next(httpContext);
            }
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            request.EnableRewind();

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body.Seek(0, SeekOrigin.Begin);

            return bodyAsText;
        }

        private async Task<string> ReadResponseBody(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var bodyAsText = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return bodyAsText;
        }
        
    }
}
