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
   ```
## How to Use

1. **Clone or Download the Repository**:
   Clone the repository or download the source code.

   ```bash
   git clone https://github.com/truedeity/FFMpegToHLSUploader.git
   ```

2. **Build the Application**:
   Build the application using the .NET CLI.

   ```bash
   dotnet build
   ```

3. **Run the Application**:
   Run the application by providing the input video file and a folder name for S3 upload.

   ```bash
   dotnet run <input-video-file> <s3-folder-name>
   ```

   - `<input-video-file>`: The full path of the video file you want to convert and upload.
   - `<s3-folder-name>`: The name of the folder in S3 where the HLS files will be uploaded.

   **Example**:

   ```bash
   dotnet run "/path/to/video.mp4" "my-video-folder"
   ```

4. **Access the Uploaded HLS Files**:
   After the process is complete, the HLS playlist (`output.m3u8`) and segment files (`.ts`) will be uploaded to your S3 bucket in the specified folder.
## Configuration and Customization

### Retrieving Credentials from Vault

By default, the application retrieves Linode S3 credentials from Vault. You can modify the Vault path or token as needed in the `S3Uploader` class.

```csharp
var vaultAddress = "http://localhost:8200";  // Replace with your Vault server address
var vaultToken = "myroot";  // Use your Vault token
```

Make sure the correct secrets are stored in Vault at the path `kv/linode`:
```bash
vault kv put kv/linode accessKey="<your-access-key>" secretKey="<your-secret-key>" endpoint="<your-s3-endpoint>" bucketName="<your-bucket-name>"
```

### FFmpeg Arguments

If you need to customize the FFmpeg arguments (e.g., change segment duration or codec), you can modify the `ConvertToHLS` method in the `FFmpegWrapper` class:

```csharp
var ffmpegArgs = $"-i \"{inputFilePath}\" " +
                 "-c:v libx264 " +
                 "-profile:v baseline " +
                 "-level 3.0 " +
                 "-g 48 " +
                 "-keyint_min 48 " +
                 "-sc_threshold 0 " +
                 "-c:a aac " +
                 "-b:a 128k " +
                 "-vf \"scale=1280:720\" " +
                 "-hls_time 10 " +
                 "-hls_list_size 0 " +
                 "-f hls " +
                 $"\"{outputDirectory}/output.m3u8\"";
```
## Project Structure

```
FFmpegToHLSUploader/
├── Program.cs            // Main program logic
├── S3Uploader.cs         // Handles S3 upload and interacts with Vault for credentials
├── FFmpegWrapper.cs      // FFmpeg wrapper to convert video to HLS format
├── README.md             // Documentation
```
## Troubleshooting

- **FFmpeg not found**: Ensure FFmpeg is installed and available in your system's PATH.
- **Vault errors**: Check that your Vault server is running and accessible. Verify that the correct token and secrets are in place.
- **S3 upload errors**: Ensure your Linode S3 credentials are valid and the endpoint/bucket are correctly configured.

## License

This project is licensed under the MIT License.
