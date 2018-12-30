﻿using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reusable.IOnymous;
using Reusable.Teapot;
using Reusable.Tests.XUnit.Fixtures;
using Telerik.JustMock.Helpers;
using Xunit;

namespace Reusable.Tests.XUnit
{
    public class TeapotExperiment : IClassFixture<TeapotFactoryFixture>
    {
        private const string Url = "http://localhost:12000";

        private readonly TeapotServer _teapot;

        public TeapotExperiment(TeapotFactoryFixture teapotFactory)
        {
            _teapot = teapotFactory.CreateTeapotServer(Url);
        }

        [Fact]
        public async Task PostsGreeting()
        {
            using (var teacup = _teapot.BeginScope())
            {
                var test =
                    teacup
                        .Mock("/api/test?param=true")
                        .ArrangePost((request, response) =>
                        {
                            request
                                .AsUserAgent("Reusable", "1.0")
                                .Occurs(1)
                                .AcceptsJson()
                                .WithApiVersion("1.0")
                                .WithContentTypeJson(content => { content.HasProperty("$.Greeting"); });
                            
                            response
                                .Once(200, new { Message = "OK" })
                                .Echo();
                        });

                using (var client = new HttpResourceProvider("http://localhost:12000/api", ResourceMetadata.Empty))
                {
                    // Request made by the application somewhere deep down the rabbit hole
                    var response = await client.PostAsync("test?param=true", () => ResourceHelper.SerializeAsJsonAsync(new { Greeting = "Hallo" }), ResourceMetadata.Empty.ConfigureRequestHeaders(headers =>
                    {
                        headers.Add("Api-Version", "1.0");
                        headers.UserAgent("Reusable", "1.0");
                        headers.AcceptJson();
                    }));

                    Assert.True(response.Exists);
                    var original = await response.DeserializeJsonAsync<object>();
                }
                
                test.Assert();
            }
        }
    }
}