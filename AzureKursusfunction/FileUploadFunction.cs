using System;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Drawing.Imaging;
using Azure.Storage.Blobs;

namespace AzureKursusfunction
{
    public static class FileUploadFunction
    {
        [FunctionName("FileUpload")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var containerName = Environment.GetEnvironmentVariable("ContainerName");
            var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            try
            {
                //Get File from request and display name and size
                var formdata = await req.ReadFormAsync();
                var file = req.Form.Files["file"];
                var image = Image.FromStream(file.OpenReadStream());
                var resized = new Bitmap(image, new Size(256, 256));
                using var imageStream = new MemoryStream();
                resized.Save(imageStream, ImageFormat.Jpeg);
                imageStream.Position = 0;
                var bloblService = new BlobServiceClient(conn);
                await bloblService.CreateBlobContainerAsync(containerName);
                var blobClient = new BlobContainerClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), containerName);                
                var blob = blobClient.GetBlobClient(file.FileName);
                await blob.UploadAsync(imageStream);

                return new OkObjectResult(file.FileName + " - " + file.Length.ToString() +" bytes");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
    }
}
