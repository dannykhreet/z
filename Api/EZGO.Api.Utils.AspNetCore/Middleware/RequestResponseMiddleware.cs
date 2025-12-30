using EZGO.Api.Interfaces.Data;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using EZGO.Api.Data.Helpers;
using EZGO.Api.Utils.Security;
using EZGO.Api.Utils.Json;

//TODO add configuration
namespace EZGO.Api.Utils.Middleware
{
    /// <summary>
    /// RequestResponseMiddleware; Middleware for handling specific request and response logic that can't be done on every call within a controller or other structures.
    /// This can be certain authorizations as well as logging.
    /// </summary>
    public class RequestResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDatabaseLogWriter _logWriter;

        public RequestResponseMiddleware(RequestDelegate next, IDatabaseLogWriter logWriter)
        {
            _next = next;
            _logWriter = logWriter;
        }

        /// <summary>
        /// Invoke; Called on MiddleWare invoke. Will be used for getting the response and request streams.
        /// The middle ware must be configured within the config method in the startup class.
        /// <code> app.UseMiddleware<RequestResponseMiddleware>(dblogWriter); </code>
        /// The middleware will intercept the header, request stream and response stream and some basic information and write it to a database.
        /// </summary>
        /// <param name="context">HttpContext containing request/response.</param>
        public async Task Invoke(HttpContext context)
        {
            //Check if upload and test routes aren't used. These can be ignored.
            if(!context.Request.Path.Value.Contains("/upload/") && !context.Request.Path.Value.Contains("/export/") && !context.Request.Path.Value.Contains("/test/"))
            {
                //Get header information.
                var currentHeader = await FormatHeader(context.Request); //TODO disabled for now due to issues with docker on a host, locally with docker everything is fine. Unknown how production and acceptance will handle this.
                bool redact = false;
                var currentRequest = "";

                if (!context.Request.Path.Value.Contains("/login") && !(context.Request.Path.Value == "/v1/userprofile") && !context.Request.Path.Value.Contains("password"))
                {
                    //Enable buffering (EnableRewind can not be used any more in .net 3.1, so now use enable buffering, make sure you reset the body afterwards or else you will get an empty response.
                    context.Request.EnableBuffering();

                    //Format the request and return a string with data.
                    currentRequest = await FormatRequest(context.Request);

                    //Reset body to position 0 for the next read, note this will only work when EnableBuffering is enabled!.
                    context.Request.Body.Position = 0;
                } else
                {
                    currentRequest = "-******REDACTED******-"; //Do not store login data in logging structure.
                    if (!(context.Request.Path.Value == "/v1/userprofile")) {
                        redact = true;
                    }
                }

                //Set the orignal body.
                var originalBodyStream = context.Response.Body;

                using (var responseBody = new MemoryStream()) //TODO move to Recyclable Memory Manager for optimizations.
                {
                    //Set response stream body.
                    context.Response.Body = responseBody;

                    //Continue Middleware pipeline.
                    await _next(context);

                    //Get and format the outgoing response by getting the body stream, converting it to a string (using the format method)
                    var currentResponse = await FormatResponse(context.Response);

                    //Write response and request to database.
                    if (redact) { currentResponse = currentRequest;  } //use redacted text, login message.
                    //TODO fire separate thread for writing....
                    try
                    {
                        var uid = context.User.GetClaim(ClaimTypes.NameIdentifier);
                        if (await _logWriter.GetRequestResponseLoggingEnabled(!string.IsNullOrEmpty(uid) && uid.All(Char.IsDigit) ? Convert.ToInt32(uid) : 0)) {
                            await _logWriter.WriteToLog(domain: string.Concat(context.Request.Scheme, "//", context.Request.Host),
                                                path: context.Request.Path,
                                                query: context.Request.QueryString.ToString(),
                                                status: context.Response.StatusCode.ToString(),
                                                header: currentHeader,
                                                request: currentRequest,
                                                response: currentResponse);
                        }
                    } catch (Exception ex)
                    {
                        var exc = ex;
                        //DO not log.
                        //Try Catch structure is for writing to DB, but seeing docker is a bit picky on what items from a request we can use, try catch the writer.
                    }

                    //Copy the contents of the new stream to the original stream body.
                    await responseBody.CopyToAsync(originalBodyStream);

                }
            } else
            {
                await _next(context); //just continue the middleware pipe.
            }

        }

        /// <summary>
        /// FormatHeader; Gets the request header information and formats it to a string.
        /// </summary>
        /// <param name="request">Incoming request</param>
        /// <returns>String containing possible header</returns>
        private async Task<string> FormatHeader(HttpRequest request)
        {
            await Task.CompletedTask;
            //return request.Headers.ToList().ToJsonFromObject();
            return request.Headers.Where(x=> x.Key != "Authorization").ToList().ToJsonFromObject();
        }

        /// <summary>
        /// FormatRequest; Gets request information and formats it to a string containing some general data and body.
        /// </summary>
        /// <param name="request">Incoming request.</param>
        /// <returns>String containing possible body.</returns>
        private async Task<string> FormatRequest(HttpRequest request)
        {
            var body = request.Body;

            //Create a buffer and read the stream.
            var streamBuffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(streamBuffer, 0, streamBuffer.Length);

            //Create the output text
            var bodyString = Encoding.UTF8.GetString(streamBuffer);

            //Assign the body back to the request.
            request.Body = body;

            return $"{bodyString}";
        }

        /// <summary>
        /// FormatResponse; Gets the response from stream and formats it to a string containing some general data and body.
        /// </summary>
        /// <param name="response">Outgoing response.</param>
        /// <returns>String containing possible body.</returns>
        private async Task<string> FormatResponse(HttpResponse response)
        {
            //Read the stream from pos 0.
            response.Body.Seek(0, SeekOrigin.Begin);

            //Create the output text.
            string bodyString = await new StreamReader(response.Body).ReadToEndAsync();

            //Reset the reader to 0.
            response.Body.Seek(0, SeekOrigin.Begin);

            //Return the string
            return $"{bodyString}";
        }
    }
}
