﻿using System;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore
{
    [UsedImplicitly]
    [PublicAPI]
    public class SemanticLogger
    {
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;
        private readonly Func<HttpContext, object> _getCorrelationId;

        public SemanticLogger
        (
            ILoggerFactory loggerFactory,
            RequestDelegate next,
            Func<HttpContext, object> getCorrelationId
        )
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<SemanticLogger>();
            _getCorrelationId = getCorrelationId;
        }

        public async Task Invoke(HttpContext context)
        {
            using (_logger.UseStopwatch())
            using (_logger.UseScope(correlationId: _getCorrelationId(context)))
            {
                // todo - what do I need this for?
                //_configuration?.ConfigureScope(scope, context);
                _logger.Log(Abstraction.Layer.Service().Meta(new
                {
                    UserAgent = 
                        context.Request.Headers.TryGetValue("User-Agent", out var userAgent)
                        ? userAgent.First()
                        : "Unknown"
                }));

                _logger.Log(Abstraction.Layer.Network().Argument(new
                {
                    Request = new
                    {
                        Path = context.Request.Path.Value,
                        Host = context.Request.Host.Value,
                        context.Request.ContentLength,
                        context.Request.ContentType,
                        //context.Request.Cookies,
                        context.Request.Headers,
                        context.Request.IsHttps,
                        context.Request.Method,
                        context.Request.Protocol,
                        context.Request.QueryString,
                    }
                }));

                try
                {
                    var body = default(string);
                    var bodyBackup = context.Response.Body;

                    using (var memory = new MemoryStream())
                    {
                        context.Response.Body = memory;

                        await _next(context);

                        memory.Rewind();
                        using (var reader = new StreamReader(memory))
                        {
                            if (context.ResponseBodyLoggingEnabled())
                            {
                                body = await reader.ReadToEndAsync();
                            }

                            // Restore Response.Body
                            memory.Rewind();
                            if (!context.Response.StatusCode.In(304))
                            {
                                await memory.CopyToAsync(bodyBackup);
                            }

                            context.Response.Body = bodyBackup;
                        }
                    }

                    _logger.Log(Abstraction.Layer.Network().Argument(new
                    {
                        Response = new
                        {
                            context.Response.ContentLength,
                            context.Response.ContentType,
                            context.Response.Headers,
                            context.Response.StatusCode,
                        }
                    }), log =>
                    {
                        log.Level(MapStatusCode(context.Response.StatusCode));
                        if (context.ResponseBodyLoggingEnabled())
                        {
                            log.Message(body);
                        }
                    });
                }
                catch (Exception inner)
                {
                    _logger.Log(Abstraction.Layer.Network().Routine("next").Faulted(), inner);
                    throw;
                }
            }
        }

        private static LogLevel MapStatusCode(int statusCode)
        {
            if (statusCode >= 500)
            {
                return LogLevel.Fatal;
            }

            if (statusCode >= 400)
            {
                return LogLevel.Error;
            }

            if (statusCode >= 300)
            {
                return LogLevel.Warning;
            }

            return LogLevel.Information;
        }
    }

    public static class HttpContextExtensions
    {
        public static void EnableResponseBodyLogging(this HttpContext context)
        {
            context.Items[nameof(ResponseBodyLoggingEnabled)] = true;
        }

        public static bool ResponseBodyLoggingEnabled(this HttpContext context)
        {
            return context.Items[nameof(ResponseBodyLoggingEnabled)] is bool enabled && enabled;
        }
    }
}