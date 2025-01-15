using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FnPostDataStorage
{
    public class FnPostDataStorage
    {
        private readonly ILogger<FnPostDataStorage> _logger;

        public FnPostDataStorage(ILogger<FnPostDataStorage> logger)
        {
            _logger = logger;
        }

        [Function("DataStorage")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Upload file to Azure Storage");

            try
            {
                if(!req.Headers.TryGetValue("file-type", out var fileTypeHeader))
                {
                    return new BadRequestObjectResult("Missing file type header");
                }

                var fileType = fileTypeHeader.ToString();
                var form = await req.ReadFormAsync();
                var file = form.Files["file"];

                if (file == null || file.Length == 0)
                {
                    return new BadRequestObjectResult("Missing file");
                }

                string? connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                string containerName = fileType;
                BlobClient blobClient = new BlobClient(connectionString, containerName, file.FileName);
                BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);

                await containerClient.CreateIfNotExistsAsync();
                await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

                string blobName = file.FileName;
                var blob = containerClient.GetBlobClient(blobName);

                using (var stream = file.OpenReadStream())
                {
                    await blob.UploadAsync(stream, true);
                }

                _logger.LogInformation($"File {file.FileName} uploaded to Azure Storage");

                return new OkObjectResult(new
                {
                    Message = "File uploaded successfully",
                    BlobUrl = blobClient.Uri
                });

            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to Azure Storage");
            }

            return new BadRequestObjectResult("Error uploading file to Azure Storage");
        }
    }
}
