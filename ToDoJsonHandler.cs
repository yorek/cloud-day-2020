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
    public static class ToDoJsonHandler
    {
        const string TEST = "json";

        private static async Task<JToken> GetBodyData(HttpRequest req)
        {
            JToken bodyData = new JObject();
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (!string.IsNullOrEmpty(requestBody))
            {
                // Can't do this: need a schema
                //bodyData = JsonConvert.DeserializeObject<JToken>(requestBody);

                // This is the way :)
                var t = JsonConvert.DeserializeObject<ToDo>(requestBody);
                bodyData = JObject.FromObject(t);
            }

            return bodyData;
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

                var resultSet = await conn.QueryAsync<string>(
                    sql: $"web.{verb}_todo_{TEST}",
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                );

                var jr = new JArray();
                resultSet.ToList().ForEach(i =>
                {
                    jr.Add(JObject.Parse(i));
                });

                if (jr.Count() == 1)
                    result = jr[0];
                else
                    result = jr;
            }

            return result;
        }

        [FunctionName("get-json")]
        public static async Task<IActionResult> Get(
            [HttpTrigger("get", Route = "todo/json/{id?}")] HttpRequest req,
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

        [FunctionName("post-json")]
        public static async Task<IActionResult> Post(
            [HttpTrigger("post", Route = "todo/json/{id?}")] HttpRequest req,
            int? id,
            ILogger log)
        {
            JToken payload = await GetBodyData(req);

            JToken result = await ExecuteProcedure("post", payload);

            Utils.EnrichJsonResult(req, result, TEST);

            return new OkObjectResult(result);
        }

        [FunctionName("patch-json")]
        public static async Task<IActionResult> Patch(
            [HttpTrigger("patch", Route = "todo/json/{id?}")] HttpRequest req,
            int? id,
            ILogger log)
        {
            // Load existing todo            
            var target_json = await ExecuteProcedure("get", new JObject { ["id"] = id.Value });

            // Get new todo
            var source_json = (JObject)(await GetBodyData(req));

            // Patch
            ((JObject)target_json).Merge(source_json);

            // Save to database
            var payload = new JObject
            {
                ["id"] = id.Value,
                ["todo"] = target_json
            };
            JToken result = await ExecuteProcedure("patch", payload);

            Utils.EnrichJsonResult(req, result, TEST);

            return new OkObjectResult(result);
        }

        [FunctionName("delete-json")]
        public static async Task<IActionResult> Delete(
            [HttpTrigger("delete", Route = "todo/json/{id?}")] HttpRequest req,
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
