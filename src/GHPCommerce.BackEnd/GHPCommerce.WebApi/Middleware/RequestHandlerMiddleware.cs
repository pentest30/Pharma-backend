using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Application.Catalog.RequestLogs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Shared;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Infra.OS;
using GHPCommerce.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog.Core;


namespace GHPCommerce.WebApi.Middleware
{
    public class RequestHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private  Logger _logger;
        private  IRepository<LogRequest, Guid> _repository;
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        private Func<ApplicationDbContext> _factory;
        public RequestHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context,Logger logger,Func<ApplicationDbContext> factory)

        {
            _logger = logger;
            
           
            

            if (!context.Request.Path.Value.Contains("ax"))
            {
                if(!context.Response.HasStarted)
                    await _next(context);
                return;
            }
            
            
          
            var logRequest = new LogRequest();
            try
            {
                using (var dbContext = factory.Invoke())
                {
                    try
                    {
                        await GetRequestDetails(context, logRequest);
                        if (!context.Response.HasStarted) await _next(context);
                        await ResponseLogger(context, logRequest);
                        logRequest.Duration = (int)(logRequest.ResponseTime - logRequest.RequestTime).TotalMilliseconds;
                        dbContext.Add(logRequest);
                        await dbContext.SaveChangesAsync();
                    }

                    catch (Exception e)
                    {
                        try
                        {
                            _logger.Error(e.Message);
                            _logger.Error(e.InnerException?.Message);
                            await HandleExceptionAsync(context, e, logRequest);
                            logRequest.Duration = (int)(logRequest.ResponseTime - logRequest.RequestTime).TotalMilliseconds;
                            dbContext.Add(logRequest);
                            await dbContext.SaveChangesAsync();
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                            _logger.Error(exception.Message);
                            _logger.Error(exception.InnerException?.Message);
                            // throw;
                        }


                        //throw;
                    }
             
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                _logger.Error(exception.Message);
                _logger.Error(exception.InnerException?.Message);
            }
        }

        private static async Task GetRequestDetails(HttpContext context, LogRequest logRequest)
        {
            try
            {
                context.Request.EnableBuffering();
                var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                logRequest.ClientIP = context.Connection.RemoteIpAddress.ToString();
                logRequest.Action = context.Request.Path;
                logRequest.Methode = context.Request.Method;
                logRequest.Body = body;
                logRequest.RequestTime = DateTime.Now;
                context.Request.Body.Position = 0;
                context.Request.Body.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
        }

        public async Task ResponseLogger(HttpContext context, LogRequest model)
        {
            using (Stream originalRequest = context.Response.Body)
            {
                try
                {
                    using (var memStream = new MemoryStream())
                    {
                        context.Response.Body = memStream;
                        memStream.Position = 0;
                        // read the memory stream till the end
                        var response = await new StreamReader(memStream)
                            .ReadToEndAsync();
                        model.StatusCode = context.Response.StatusCode.ToString();
                        model.ResponseTime = DateTime.Now;
                        model.Header = response;
                        memStream.Position = 0;
                        await memStream.CopyToAsync(originalRequest);
                        context.Request.Body.Seek(0, SeekOrigin.Begin);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    // assign the response body to the actual context
                    context.Response.Body = originalRequest;
                }
            }
            
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, LogRequest logApiModel)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            logApiModel.ResponseTime = DateTime.Now;
            logApiModel.Header = exception.Message;
            logApiModel.StatusCode = ((int)HttpStatusCode.InternalServerError).ToString();
            // await context.Response.WriteAsync(JsonConvert.SerializeObject(new ErrorDetails
            // {
            //     StatusCode = 500,
            //     Message = exception.Message,
            //     ExceptionStack = exception.InnerException?.Message
            // }));
        }
    }

    internal class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string ExceptionStack { get; set; }
    }
}