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
                result = await conn.QueryFirstOrDefaultAsync<ToDo>("select id, todo as title, completed from dbo.todos where id = @id", new { @id = id });
            }

            return new OkObjectResult(result);
        }


        [FunctionName("test2")]
        public static async Task<IActionResult> Test2([HttpTrigger("get", Route = "todo/test2/{id?}")] HttpRequest req, int? id, ILogger log)
        {
            JToken result = new JObject();

            using (var conn = new SqlConnection(Environment.GetEnvironmentVariable("AzureSQLConnectionString")))
            {
                var stringResult = await conn.QueryFirstOrDefaultAsync<string>("select id, todo as title, completed from dbo.todos where id = @id for json auto", new { @id = id });
                if (!string.IsNullOrEmpty(stringResult)) result = JToken.Parse(stringResult);
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
                    sql: $"web.get_todo",
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (!string.IsNullOrEmpty(stringResult)) result = JToken.Parse(stringResult);
            }

            return new OkObjectResult(result);
        }
    }
}
