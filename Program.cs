using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("No argument pased");
            return;
        }

        string inputFilePath = args[0];  // Use the first argument as the input file 

        string fileName = Path.GetFileNameWithoutExtension(inputFilePath);

        string outputDirectory = Path.Combine(Path.GetTempPath(), fileName);

        try
        {
            Console.WriteLine($"Converting video {fileName} to HLS format...");
            if (!FFmpegWrapper.IsFFmpegInstalled())
            {
                throw new Exception("FFmpeg is not installed or not found in PATH.");
            }
            FFmpegWrapper.ConvertToHLS(inputFilePath, outputDirectory);

            Console.WriteLine($"Uploading HLS segments for {fileName} to S3...");
            var s3Uploader = new S3Uploader();
            await s3Uploader.UploadDirectoryAsync(outputDirectory, fileName);

            Console.WriteLine("Process completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public class FFmpegWrapper
    {
       
            public static void ConvertToHLS(string inputFilePath, string outputDirectory)
            {
                System.IO.Directory.CreateDirectory(outputDirectory);

                // FFmpeg command to convert to HLS
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

                // Run the process
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = ffmpegArgs,
                        RedirectStandardOutput = true,   // Capture standard output
                        RedirectStandardError = true,    // Capture standard error
                        UseShellExecute = false,         // Needed for redirection
                        CreateNoWindow = true            // Do not create a window
                    }
                };

                // Capture the output and error streams asynchronously
                process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
                process.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg conversion failed with exit code {process.ExitCode}.");
                }

                Console.WriteLine("FFmpeg conversion completed successfully.");
            }
        


        public static bool IsFFmpegInstalled()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;

                    // Capture the output and error asynchronously
                    process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
                    process.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);

                    process.Start();

                    // Begin reading the output and error streams
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();

                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking FFmpeg installation: {ex.Message}");
                return false;
            }
        }

    }
}
