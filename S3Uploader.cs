using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;
using System.Threading.Tasks;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;

public class S3Uploader
{
    private string accessKey;
    private string secretKey;
    private string endpoint;
    private string bucketName;

    private AmazonS3Client s3Client;

    public S3Uploader()
    {
        GetCredentialsFromVault().Wait();  

        var s3Config = new AmazonS3Config
        {
            ServiceURL = $"https://{endpoint}",  
            UseHttp = true,  
            ForcePathStyle = true  
        };

        s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);  
    }

    /// <summary>
    /// 
    /// choco install vault
    /// 
    /// vault kv put kv/linode accessKey="<your-access-key>" secretKey="<your-secret-key>" endpoint="<your-s3-endpoint>" bucketName="<your-bucket-name>"
    /// </summary>
    /// <returns></returns>
    private async Task GetCredentialsFromVault()
    {
        try
        {
            // Set up Vault client
            var vaultAddress = "http://localhost:8200";  // Replace with your Vault server address
            var vaultToken = "myroot";  // Use your Vault token
            IAuthMethodInfo authMethod = new TokenAuthMethodInfo(vaultToken);
            var vaultClientSettings = new VaultClientSettings(vaultAddress, authMethod);
            IVaultClient vaultClient = new VaultClient(vaultClientSettings);

            // Read secrets from Vault
            var secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync("kv/linode");

            // Extract the values from the Vault secret
            accessKey = secret.Data.Data["accessKey"].ToString();
            secretKey = secret.Data.Data["secretKey"].ToString();
            endpoint = secret.Data.Data["endpoint"].ToString();
            bucketName = secret.Data.Data["bucketName"].ToString();

            Console.WriteLine("Successfully retrieved S3 credentials from Vault.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving credentials from Vault: {ex.Message}");
        }
    }

    public async Task UploadDirectoryAsync(string directoryPath, string folderName)
    {
        var files = Directory.GetFiles(directoryPath);

        foreach (var file in files)
        {
            await UploadFileAsync(file, folderName);
        }
    }

    public async Task UploadFileAsync(string filePath, string folderName)
    {
        var fileName = Path.GetFileName(filePath);
        var keyName = $"{folderName}/{fileName}";  // Store files in a folder named after the input file without extension

        try
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = keyName,
                FilePath = filePath,
                ContentType = "application/octet-stream",
                CannedACL = S3CannedACL.PublicRead // Makes the file publicly accessible
            };

            var response = await s3Client.PutObjectAsync(putRequest);
            Console.WriteLine($"Uploaded {fileName} to S3 at {keyName}");
        }
        catch (AmazonS3Exception e)
        {
            Console.WriteLine($"Error uploading {fileName}: {e.Message}");
        }
    }
}
