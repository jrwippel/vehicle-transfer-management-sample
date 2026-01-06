using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace WebAppSystemsTransp.Services
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public BlobStorageService(IConfiguration configuration)
        {
            // Corrigido para acessar a ConnectionString completa
            string connectionString = configuration["AzureBlobStorage:ConnectionString"];

            _containerName = configuration["AzureBlobStorage:ContainerName"];
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadFileAsync(string fileName, Stream fileStream)
        {
            try
            {
                // Verificar se a stream está válida
                if (fileStream == null || fileStream.Length == 0)
                {
                    throw new ArgumentException("O arquivo de upload está vazio ou inválido.");
                }

                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

                // Verificar se o contêiner existe
                if (!await containerClient.ExistsAsync())
                {
                    throw new Exception("O contêiner especificado não existe.");
                }

                var blobClient = containerClient.GetBlobClient(fileName);

                // Verifica se o arquivo já existe
                if (await blobClient.ExistsAsync())
                {
                    throw new Exception("O arquivo já existe no contêiner.");
                }

                // Fazer o upload do arquivo
                await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = "image/jpeg" });

                // Retorna a URL do arquivo no Blob Storage
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                // Log da exceção para diagnóstico
                Console.WriteLine($"Erro ao tentar fazer upload: {ex.Message}");
                throw new Exception("Erro ao fazer upload para o Blob Storage.", ex);
            }
        }


        public async Task DeleteFileAsync(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }

        // Método para baixar a imagem
        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            try
            {
                // Verifica se o fileName é uma URL
                Uri uri;
                if (Uri.TryCreate(fileName, UriKind.Absolute, out uri))
                {
                    // Extrai o nome do arquivo da URL
                    fileName = Path.GetFileName(uri.LocalPath);  // "ba41ffaa-c23d-47df-b754-d8ce7fb0ea2b.jpg"
                }

                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(fileName);

                // Verifica se o arquivo existe no Blob Storage
                if (!await blobClient.ExistsAsync())
                {
                    throw new Exception("O arquivo não existe no contêiner.");
                }

                // Faz o download do arquivo
                BlobDownloadInfo download = await blobClient.DownloadAsync();

                // Retorna o Stream com o conteúdo do arquivo
                return download.Content;
            }
            catch (Exception ex)
            {
                // Log de erro para diagnóstico
                Console.WriteLine($"Erro ao tentar baixar o arquivo: {ex.Message}");
                throw new Exception("Erro ao baixar o arquivo do Blob Storage.", ex);
            }
        }



    }
}

