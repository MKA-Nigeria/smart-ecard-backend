using Application.Common.FileStorage;
using BunnyCDN.Net.Storage;
using Domain.Common;
using Infrastructure.Common.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace maanportal.Infrastructure.FileStorage;
public class CloudFileStorageService(IWebHostEnvironment webHostEnvironment) : IFileStorageService
{
    private const string _storageZoneName = "smartcard"; // Replace with your storage zone name
    private const string _accessKey = "b630398a-cad6-498d-b8aa5656cdec-2a4f-411d"; // Replace with your Bunny.net access key
    private const string _region = "jh"; // Replace with your region (empty string for Germany)
    public async Task<string> UploadAsync<T>(FileUploadRequest? request, FileType supportedFileType, CancellationToken cancellationToken = default)
    where T : class
    {
        if (request == null || request.Data == null)
        {
            return string.Empty;
        }

        if (request.Extension is null || !supportedFileType.GetDescriptionList().Contains(request.Extension.ToLower()))
            throw new InvalidOperationException("File Format Not Supported.");
        if (request.Name is null)
            throw new InvalidOperationException("Name is required.");

        string base64Data = Regex.Match(request.Data, "data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;

        var streamData = new MemoryStream(Convert.FromBase64String(base64Data));
        if (streamData.Length > 0)
        {
            string fileName = request.Name.Trim('"');
            fileName = RemoveSpecialCharacters(fileName);
            fileName = fileName.ReplaceWhitespace("-");
            fileName += request.Extension.Trim();
            string path = $"/{_storageZoneName}/{fileName}";
            var bunnyCDNStorage = new BunnyCDNStorage(_storageZoneName, _accessKey, _region);
            await bunnyCDNStorage.UploadAsync(streamData, path);
            return path;
        }
        else
        {
            return string.Empty;
        }
    }

    public static string RemoveSpecialCharacters(string str)
    {
        return Regex.Replace(str, "[^a-zA-Z0-9_.]+", string.Empty, RegexOptions.Compiled);
    }

    public void Remove(string? path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private const string NumberPattern = "-{0}";

    private static string NextAvailableFilename(string path)
    {
        if (!File.Exists(path))
        {
            return path;
        }

        if (Path.HasExtension(path))
        {
            return GetNextFilename(path.Insert(path.LastIndexOf(Path.GetExtension(path), StringComparison.Ordinal), NumberPattern));
        }

        return GetNextFilename(path + NumberPattern);
    }

    private static string GetNextFilename(string pattern)
    {
        string tmp = string.Format(pattern, 1);

        if (!File.Exists(tmp))
        {
            return tmp;
        }

        int min = 1, max = 2;

        while (File.Exists(string.Format(pattern, max)))
        {
            min = max;
            max *= 2;
        }

        while (max != min + 1)
        {
            int pivot = (max + min) / 2;
            if (File.Exists(string.Format(pattern, pivot)))
            {
                min = pivot;
            }
            else
            {
                max = pivot;
            }
        }

        return string.Format(pattern, max);
    }

    public static async Task DownloadFileFromBunnyNet(string fileName, string downloadPath)
    {
        var bunnyCDNStorage = new BunnyCDNStorage(_storageZoneName, _accessKey, _region);

        using var fileStream = System.IO.File.OpenWrite(downloadPath);
        await bunnyCDNStorage.DownloadObjectAsStreamAsync(fileName);

        // await bunnyCDNStorage.DownloadObjectAsStreamAsync("/storagezonename/helloworld.txt");
    }

    public async Task<string> GetImageDataAsync(string imagePath)
    {
        var bunnyCDNStorage = new BunnyCDNStorage(_storageZoneName, _accessKey, _region);

        try
        {
            // Download the image as a stream from BunnyCDN
            using var stream = await bunnyCDNStorage.DownloadObjectAsStreamAsync(imagePath);
            if (stream == null)
            {
                return null;
            }

            // Read the stream and convert it to byte array
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            // Convert byte array to base64 string
            return Convert.ToBase64String(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            // throw new Exception($"Failed to retrieve image data: {ex.Message}");
            return string.Empty;
        }
    }
}