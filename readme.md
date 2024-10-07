# FFmpeg-to-HLS-S3 Uploader

This application is a CLI tool built in C# that wraps FFmpeg to convert videos into HLS format and uploads them to an S3-compliant object storage (Linode Object Storage) in a structured folder format. It securely retrieves credentials from HashiCorp Vault and uses these credentials to interact with the S3 service.

## Features

- Converts videos to HLS format using FFmpeg.
- Uploads HLS `.m3u8` playlists and segment `.ts` files to Linode Object Storage.
- Credentials (access key, secret key, endpoint, and bucket name) are securely stored and retrieved from HashiCorp Vault.
- Organizes uploaded HLS files into folders named after the input video file (without extension).

## Prerequisites

Before using the application, ensure that you have the following prerequisites:

### 1. **Install FFmpeg**
   FFmpeg is required for converting videos to HLS format.

   - **Linux**:
     ```bash
     sudo apt-get install ffmpeg
     ```
   - **macOS** (using Homebrew):
     ```bash
     brew install ffmpeg
     ```
   - **Windows**: Download FFmpeg from [here](https://ffmpeg.org/download.html) and add it to your system's PATH.

### 2. **HashiCorp Vault**

   - Install and run HashiCorp Vault to store and retrieve your Linode S3 credentials.
   - Store the following secrets in Vault under the `kv/linode` path:
     ```bash
     vault kv put kv/linode accessKey="<your-access-key>" secretKey="<your-secret-key>" endpoint="<your-s3-endpoint>" bucketName="<your-bucket-name>"
     ```

### 3. **Install .NET SDK**

   - Ensure you have .NET SDK 6.0 or later installed. You can download it [here](https://dotnet.microsoft.com/download).

### 4. **Install VaultSharp**

   The app uses VaultSharp to securely retrieve credentials from Vault.

   Install the VaultSharp NuGet package in your project:
   ```bash
   dotnet add package VaultSharp
