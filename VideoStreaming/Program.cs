using Azure.Storage.Blobs;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);
var connectionString = "DefaultEndpointsProtocol=https;AccountName=blobstorage656787001;AccountKey=Av9fk0Id+UEID+kEQrYd0a8XnxFZkVPg+wjOIhaI+oLjrM9fCVGS9Wb1++xNY2Wt8TchJPO+DkRl+ASt3k5h9A==;EndpointSuffix=core.windows.net";

builder.Services.AddSingleton(x => new BlobServiceClient(connectionString));
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

string wwwroot = builder.Environment.WebRootPath;


var containerName = "media";

app.MapGet("/video", () =>
{
    string filePath = Path.Combine(wwwroot, "file_example_MP4_480_1_5MG.mp4");
    return Results.Stream(new FileStream(filePath, FileMode.Open));
});



app.MapGet("/media/{filename}", async (BlobServiceClient blobServiceClient, string filename) =>
{
    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    var blobClient = containerClient.GetBlobClient(filename);

    if (!await blobClient.ExistsAsync())
    {
        return Results.NotFound();
    }

    // Set the appropriate content type based on the file extension
    var contentType = GetContentType(filename);

    // Stream the blob content as the response
    var response = await blobClient.DownloadAsync();
    return Results.File((Stream)response.Value.Content, contentType);
});



app.Run();

// Helper method to get the content type based on the file extension
string GetContentType(string filename)
{
    var provider = new FileExtensionContentTypeProvider();
    if (!provider.TryGetContentType(filename, out var contentType))
    {
        contentType = "application/octet-stream";
    }

    return contentType;
}