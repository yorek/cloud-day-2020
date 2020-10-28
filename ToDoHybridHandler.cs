using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;
using System.Linq;

namespace azure_sql_todo_backend_func_dotnet
{
    public static class ToDoHybridHandler
    {
        const string TEST = "hybrid";

        private class DatabaseResult
        {
            public string Todo;
            public string Extension;
        }

        private static async Task<JToken> GetBodyData(HttpRequest req, JObject existing_json = null)
        {
            JObject bodyData = new JObject();
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (!string.IsNullOrEmpty(requestBody))
            {
                bodyData = JsonConvert.DeserializeObject<JObject>(requestBody);
            }

            // Patch
            if (existing_json != null) existing_json.Merge(bodyData);

            var todo = new JObject {
                ["id"] = bodyData["id"],
                ["title"] = bodyData["title"],
                ["completed"] = bodyData["completed"]
            };

            bodyData.Property("id")?.Remove();
            bodyData.Property("title")?.Remove();
            bodyData.Property("completed")?.Remove();
            bodyData.Property("url")?.Remove();

            todo.Add("extension", bodyData);

            return todo;
        }

        private static async Task<JToken> ExecuteProcedure(string verb, JToken payload)
        {
            JToken result = new JArray();

            using (var conn = new SqlConnection(Environment.GetEnvironmentVariable("AzureSQLConnectionString")))
            {
                DynamicParameters parameters = new DynamicParameters();

                if (payload != null)
                {
                    parameters.Add("payload", payload.ToString());
                }

                 var resultSet = await conn.QueryAsync<DatabaseResult>(
                    sql: $"web.{verb}_todo_{TEST}",
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                );

                var jr = new JArray();
                resultSet.ToList().ForEach(i =>
                {
                    JObject todo = JObject.Parse(i.Todo);
                    if (i.Extension != null ) {
                        JObject extension = JObject.Parse(i.Extension);
                        todo.Merge(extension);
                    }

                    jr.Add(todo);
                });

                if (jr.Count() == 1)
                    result = jr[0];
                else
                    result = jr;
            }

            return result;            
        }

        [FunctionName("get-hybrid")]
        public static async Task<IActionResult> Get(
            [HttpTrigger("get", Route = "todo/hybrid/{id?}")] HttpRequest req,
            int? id,
            ILogger log)
        {
            JToken payload = null;
            if (id.HasValue) payload = new JObject { ["id"] = id.Value };
            
            JToken result = await ExecuteProcedure("get", payload);

            // If requesting ALL todo, always return an array
            if (id == null && result.Type == JTokenType.Object)
                result = new JArray() { result };
                
            Utils.EnrichJsonResult(req, result, TEST);

            return new OkObjectResult(result);
        }

        [FunctionName("post-hybrid")]
        public static async Task<IActionResult> Post(
            [HttpTrigger("post", Route = "todo/hybrid/{id?}")] HttpRequest req,
            int? id,
            ILogger log)
        {            
            JToken payload = await GetBodyData(req);      

            JToken result = await ExecuteProcedure("post", payload);

            Utils.EnrichJsonResult(req, result, TEST);

            return new OkObjectResult(result);
        }  

        [FunctionName("patch-hybrid")]
        public static async Task<IActionResult> Patch(
            [HttpTrigger("patch", Route = "todo/hybrid/{id?}")] HttpRequest req,
            int? id,
            ILogger log)
        {            
            // Load existing todo            
            var existing_json = (JObject)(await ExecuteProcedure("get", new JObject { ["id"] = id.Value }));

            // Get new todo
            var new_json = (JObject)(await GetBodyData(req, existing_json));            

            // Save to database
            var payload = new JObject
            {
                ["id"] = id.Value,
                ["todo"] = new_json
            };
            JToken result = await ExecuteProcedure("patch", payload);

            Utils.EnrichJsonResult(req, result, TEST);

            return new OkObjectResult(result);
        }      

        [FunctionName("delete-hybrid")]
        public static async Task<IActionResult> Delete(
            [HttpTrigger("delete", Route = "todo/hybrid/{id?}")] HttpRequest req,
            int? id,
            ILogger log)
        {            
             JToken payload = null;
            if (id.HasValue) payload = new JObject { ["id"] = id.Value };
            
            JToken result = await ExecuteProcedure("delete", payload);

            Utils.EnrichJsonResult(req, result, TEST);

            return new OkObjectResult(result);
        }        
    }    
}
