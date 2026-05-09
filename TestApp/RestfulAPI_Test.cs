using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace TestApp
{
    internal class RestfulAPI_Test : PageTest
    {
        private IAPIRequestContext apiRequest;
        [SetUp]
        public async Task SetUp()
        {
            await CreateAPIRequestContext();
        }

        private async Task CreateAPIRequestContext()
        {
            apiRequest = await this.Playwright.APIRequest.NewContextAsync(new()
            {
                // All requests we send go to this API endpoint.
                BaseURL = "https://api.restful-api.dev",
            });
        }

        [Test]
        public async Task CheckListItems()
        {
            var objectsResponse = await apiRequest.GetAsync("/objects");
            await Expect(objectsResponse).ToBeOKAsync();

            var objects = await objectsResponse.JsonAsync();

            foreach (var item in objects?.EnumerateArray())
            {
                var phone = item.Deserialize<Phone>();
                Console.WriteLine(phone.ToString());
            }
        }

        [Test]
        public async Task PostData()
        {
            var postOb = await apiRequest.PostAsync("/objects", new()
            {
                DataObject = new
                {
                    name = "Apple MacBook Pro 16",
                    data = new
                    {
                        year = 2019,
                        price = 1849.99,
                        CPUmodel = "Intel Core i9",
                        Harddisksize = "1 TB"
                    }
                }
            });
            await Expect(postOb).ToBeOKAsync();
            JsonElement? postResponse = await postOb.JsonAsync();

            Console.WriteLine(postResponse);
        }

        [TearDown]
        public async Task CleanUp()
        {
            await apiRequest.DisposeAsync();
        }
    }

    public class Phone
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("data")]
        public PhoneProperties Properties { get; set; }

        public override string ToString()
        {
            return $"{Id}: {Name} {Properties}";
        }
    }
    public class PhoneProperties
    {
        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("capacity")]
        public string Capacity { get; set; }

        public override string ToString()
        {
            return $"{Color} color with {Capacity} GB storage Capacity";
        }
    }
}
