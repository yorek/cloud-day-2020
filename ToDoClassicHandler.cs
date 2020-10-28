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
    public static class ToDoClassicHandler
    {
        const string TEST = "classic";

        private static async Task<JToken> GetBodyData(HttpRequest req)
        {
            JToken bodyData = new JObject();
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (!string.IsNullOrEmpty(requestBody))
            {
                bodyData = JsonConvert.DeserializeObject<JToken>(requestBody);
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

                string stringResult = await conn.ExecuteScalarAsync<string>(
                    sql: $"web.{verb}_todo_{TEST}",
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (!string.IsNullOrEmpty(stringResult)) result = JToken.Parse(stringResult);
            }

            return result;            
        }

        [FunctionName("get-classic")]
        public static async Task<IActionResult> Get(
            [HttpTrigger("get", Route = "todo/classic/{id?}")] HttpRequest req,
            int? id,
            ILogger log)
        {
            JToken payload = null;
            if (id.HasValue) payload = new JObject { ["id"] = id.Value };
            
            JToken result = await ExecuteProcedure("get", payload);

            Utils.EnrichJsonResult(req, result, TEST);

            return new OkObjectResult(result);
        }

        [FunctionName("post-classic")]
        public static async Task<IActionResult> Post(
            [HttpTrigger("post", Route = "todo/classic/{id?}")] HttpRequest req,
            int? id,
            ILogger log)
        {            
            JToken payload = await GetBodyData(req);      

            JToken result = await ExecuteProcedure("post", payload);

            Utils.EnrichJsonResult(req, result, TEST);

            return new OkObjectResult(result);
        }  

        [FunctionName("patch-classic")]
        public static async Task<IActionResult> Patch(
            [HttpTrigger("patch", Route = "todo/classic/{id?}")] HttpRequest req,
            int? id,
            ILogger log)
        {            
            JToken payload = new JObject
            {
                ["id"] = id.Value,
                ["todo"] = await GetBodyData(req)
            };

            JToken result = await ExecuteProcedure("patch", payload);

            Utils.EnrichJsonResult(req, result, TEST);

            return new OkObjectResult(result);
        }      

        [FunctionName("delete-classic")]
        public static async Task<IActionResult> Delete(
            [HttpTrigger("delete", Route = "todo/classic/{id?}")] HttpRequest req,
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
