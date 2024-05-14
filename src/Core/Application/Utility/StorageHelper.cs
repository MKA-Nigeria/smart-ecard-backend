using Renci.SshNet;
using Renci.SshNet.Async;

namespace Application.Utility;
public class StorageHelper
{
    public static async Task UploadImage(Stream input, string fileName)
    {
        // Create a new SSH connection to the server
        using var client = new SshClient("108.181.169.96", "administrator", "5S6qs6YTJv5@+c");
        client.Connect();

        // Create an SFTP client over the SSH connection
        using (var sftp = new SftpClient(client.ConnectionInfo))
        {
            sftp.Connect();

            var remotePath = $"/home/administrator/files/photos/{fileName}";

            sftp.BufferSize = 1024 * 1024 * 10; // 10MB buffer size
            await sftp.UploadAsync(input, remotePath);

            sftp.Disconnect();
        }

        client.Disconnect();
    }

    public static async Task<MemoryStream> RetrieveImage(string imageName)
    {
        using (var client = new SshClient("108.181.169.96", "administrator", "5S6qs6YTJv5@+c"))
        {
            client.Connect();
            using (var sftp = new SftpClient(client.ConnectionInfo))
            {
                sftp.Connect();

                // Set the remote path to the image file
                var remotePath = $"/home/administrator/files/photos/{imageName}";

                // Download the file to a stream
                var stream = new MemoryStream();
                sftp.DownloadFile(remotePath, stream);
                stream.Position = 0;

                // Return the file as a response with appropriate content type
                return stream;
            }
        }
    }
}
