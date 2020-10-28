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
    public static class ToDoSampleHandler
    {
        [FunctionName("test1")]
        public static async Task<IActionResult> Test1([HttpTrigger("get", Route = "todo/test1/{id?}")] HttpRequest req, int? id, ILogger log)
        {
            ToDo result;

            using (var conn = new SqlConnection(Environment.GetEnvironmentVariable("AzureSQLConnectionString")))
            {
                result = await conn.QuerySingleOrDefaultAsync<ToDo>("web.get_todo_sample_classic", new { @id = id }, commandType: CommandType.StoredProcedure);
            }

            return new OkObjectResult(result);
        }


        [FunctionName("test2")]
        public static async Task<IActionResult> Test2([HttpTrigger("get", Route = "todo/test2/{id?}")] HttpRequest req, int? id, ILogger log)
        {
            JToken result = new JObject();

            using (var conn = new SqlConnection(Environment.GetEnvironmentVariable("AzureSQLConnectionString")))
            {
                var stringResult = await conn.ExecuteScalarAsync<string>("web.get_todo_sample_json", new { @id = id }, commandType: CommandType.StoredProcedure);
                if (!string.IsNullOrEmpty(stringResult)) result = JToken.Parse(stringResult); // or JsonConvert.DeserializeObject<ToDo>(stringResult)
            }

            return new OkObjectResult(result);
        }

        [FunctionName("test3")]
        public static async Task<IActionResult> Test3(
            [HttpTrigger("get", Route = "todo/test3/{id?}")] HttpRequest req,
            int? id,
            ILogger log)
        {
            JToken result = new JObject();

            JObject payload = null;
            if (id.HasValue)
            {
                payload = new JObject
                {
                    ["id"] = id.Value
                };
            }

            using (var conn = new SqlConnection(Environment.GetEnvironmentVariable("AzureSQLConnectionString")))
            {
                DynamicParameters parameters = new DynamicParameters();

                if (payload != null)
                {
                    parameters.Add("payload", payload.ToString());
                }

                string stringResult = await conn.ExecuteScalarAsync<string>(
                    sql: $"web.get_todo_sample_json2",
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (!string.IsNullOrEmpty(stringResult)) result = JToken.Parse(stringResult);
            }

            return new OkObjectResult(result);
        }
    }
}
