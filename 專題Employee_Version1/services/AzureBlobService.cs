using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;


namespace 專題Employee_Version1.services
{
    public class AzureBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;

        
        public AzureBlobService(string connectionString, string containerName)
        {
           
            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        }

        public async Task<string> UploadFileAsync(string blobName, Stream stream)
        {
            try
            {
                BlobClient blobClient = _containerClient.GetBlobClient(blobName);
                await blobClient.UploadAsync(stream, true);
                return "Upload successful";
            }
            catch (Exception ex)
            {
                // 捕捉例外狀況並回傳錯誤訊息
                return $"Upload failed: {ex.Message}";
            }

        }


        public async Task<Stream> DownloadFileAsync(string blobName)
        {
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);
            BlobDownloadInfo download = await blobClient.DownloadAsync();
            return download.Content;
        }

        public string GetBlobUrl(string blobName)
        {
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);
            return blobClient.Uri.ToString();
        }



    }
}