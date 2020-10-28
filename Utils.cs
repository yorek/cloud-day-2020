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
    public static class Utils
    {
        public static void EnrichJsonResult(HttpRequest req, JToken result, string test)
        {
            var baseUrl = req.Scheme + "://" + req.Host + $"/api/todo/{test}";

            var InjectUrl = new Action<JObject>(i =>
            {
                if (i != null)
                {
                    var itemId = i["id"]?.Value<int>();
                    if (itemId != null) i["url"] = baseUrl + $"/{itemId}";
                }
            });

            switch (result.Type)
            {
                case JTokenType.Object:
                    InjectUrl(result as JObject);
                    break;

                case JTokenType.Array:
                    foreach (var i in result)
                    {
                        InjectUrl(i as JObject);
                    }
                    break;
            }
        }
    }
}