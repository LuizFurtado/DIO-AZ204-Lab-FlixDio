using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FnPostDatabase
{
    public class FnPostDatabase
    {
        private readonly ILogger<FnPostDatabase> _logger;

        public FnPostDatabase(ILogger<FnPostDatabase> logger)
        {
            _logger = logger;
        }

        [Function("Movie")]
        [CosmosDBOutput("%DatabaseName%", "%ContainerName%", Connection = "CosmosDBConnection", CreateIfNotExists = true)]
        public async Task<object?> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("New movie creation started...");

            MovieRequest movie = null;

            var content = await new StreamReader(req.Body).ReadToEndAsync();

            try
            {
                movie = JsonConvert.DeserializeObject<MovieRequest>(content);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }

            return JsonConvert.SerializeObject(movie);
        }
    }
}
