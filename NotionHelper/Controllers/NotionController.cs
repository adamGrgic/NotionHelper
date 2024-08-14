using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace NotionHelper.Controllers
{
    [Route("api/{route}")]
    public class NotionController : Controller
    {
        private readonly HttpClient _httpClient;

        public NotionController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> PurgeDailyTaskboard()
        {
            var NOTION_KEY = "";
            var NOTION_DATABASE_ID = ""; 

            // Step 1: Query the database to get all entries
            var queryUrl = $"https://api.notion.com/v1/databases/{NOTION_DATABASE_ID}/query";

            using (var queryRequest = new HttpRequestMessage(HttpMethod.Post, queryUrl))
            {
                queryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", NOTION_KEY);
                queryRequest.Headers.Add("Notion-Version", "2022-06-28");

                var queryResponse = await _httpClient.SendAsync(queryRequest);
                if (!queryResponse.IsSuccessStatusCode)
                {
                    return StatusCode((int)queryResponse.StatusCode, await queryResponse.Content.ReadAsStringAsync());
                }

                var content = await queryResponse.Content.ReadAsStringAsync();
                var jsonResponse = JsonDocument.Parse(content);
                var results = jsonResponse.RootElement.GetProperty("results");

                foreach (var result in results.EnumerateArray())
                {
                    var pageId = result.GetProperty("id").GetString();

                    // Step 2: Delete each page
                    var deleteUrl = $"https://api.notion.com/v1/pages/{pageId}";
                    using (var deleteRequest = new HttpRequestMessage(HttpMethod.Patch, deleteUrl))
                    {
                        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", NOTION_KEY);
                        deleteRequest.Headers.Add("Notion-Version", "2022-06-28");

                        // Setting the archived property to true to delete the page
                        var body = new StringContent("{\"archived\":true}", Encoding.UTF8, "application/json");
                        deleteRequest.Content = body;

                        var deleteResponse = await _httpClient.SendAsync(deleteRequest);
                        if (!deleteResponse.IsSuccessStatusCode)
                        {
                            return StatusCode((int)deleteResponse.StatusCode, await deleteResponse.Content.ReadAsStringAsync());
                        }
                    }
                }
            }

            return Ok("All entries deleted.");
        }

    }
}
