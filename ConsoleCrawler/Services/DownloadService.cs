using System.Drawing.Imaging;

namespace Service;

public class DownloadService
{
    private readonly FileCheckerService _fileCheckerService;
    private readonly HttpClient _httpClient;
    private readonly ConsoleWriteService _consoleWrite;

    public DownloadService()
    {
        _fileCheckerService = new FileCheckerService();
        _httpClient = new HttpClient();
        _consoleWrite = new ConsoleWriteService();

    }

    public async Task SaveImage(string url, string envPath)
    {
        try
        {
            if (url.StartsWith("data:image/png;base64"))
            {
                var fileName = new Guid().ToString();
                var request = _httpClient.GetAsync(url).Result;
                if (request.IsSuccessStatusCode)
                {
                    var response = await request.Content.ReadAsStringAsync();
                    await SaveFromBase64(envPath, response, fileName);
                }
            }
            else
            {
                var fileName = Path.GetFileName(new Uri(url).AbsolutePath);
                await SaveImageStreamAsFile(envPath, await GetStream(url), fileName);
            }

        }
        catch (Exception ex)
        {
            _consoleWrite.WriteBoxColor(ex.Message.ToString(), ConsoleColor.Red, ConsoleColor.Black);
        }
        return;
    }

    public async Task SaveVideo(string url, string envPath)
    {
        try
        {
            var uri = new Uri(url);
            var fileName = Path.GetFileName(uri.AbsolutePath);
            var filePath = Path.Combine(envPath, fileName);
            using (var s = await _httpClient.GetStreamAsync(uri))
            {
                using (var fs = new FileStream(filePath, FileMode.CreateNew))
                {
                    await s.CopyToAsync(fs);
                }
            }
            _consoleWrite.WriteBoxColor("DOWNLOADED - VIDEO", ConsoleColor.Green, ConsoleColor.Black);
        }
        catch (Exception ex)
        {
            _consoleWrite.WriteBoxColor(ex.Message.ToString(), ConsoleColor.Red, ConsoleColor.Black);
        }
        return;
    }


    #region Private
    /// <summary>
    /// Checking File Extension
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private ImageFormat? GetImageFormatFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLower();
        if (extension == null)
            return null;
        return extension switch
        {
            ".jpg" or ".jpeg" => ImageFormat.Jpeg,
            ".png" => ImageFormat.Png,
            ".gif" => ImageFormat.Gif,
            ".bmp" => ImageFormat.Bmp,
            ".webp" => ImageFormat.Webp,
            ".tiff" => ImageFormat.Tiff,
            ".emf" => null,
            ".wmf" => null,
            ".exif" => null,
            ".svg" => null,
            _ => null
        };
    }
    #endregion
    /// <summary>
    /// Gets the object
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private Task<Stream> GetStream(string url)
    {
        try
        {
            var request = _httpClient.GetAsync(url).Result;
            if (!request.IsSuccessStatusCode)
            {
                _consoleWrite.WriteTextColor("Could not get Stream from: " + $"[{url}]", ConsoleColor.Red);
                return (Task<Stream>)Task.CompletedTask;
            }
            var response = request.Content.ReadAsStreamAsync().Result;
            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            return (Task<Stream>)Task.FromException(ex);
        }
    }
    /// <summary>
    /// Takes a Base64Image and saves it.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="base64Image"></param>
    /// <param name="fileName"></param>
    private Task SaveFromBase64(string filePath, string base64Image, string fileName)
    {
        try
        {
            string savePath = filePath + fileName;
            // Decode the base64 image string into a byte array
            byte[] imageBytes = Convert.FromBase64String(base64Image);
            // Write the byte array to a file
            File.WriteAllBytes(savePath, imageBytes);
            _consoleWrite.WriteBoxColor("DOWNLOADED - IMAGE", ConsoleColor.Green, ConsoleColor.Black);
            return Task.CompletedTask;

        }
        catch (Exception ex)
        {
            _consoleWrite.WriteBoxColor(ex.Message.ToString(), ConsoleColor.Red, ConsoleColor.Black);
            return Task.FromException(ex);
        }
    }


    #region Private
    /// <summary>
    /// Takes a Stream
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="inputStream"></param>
    /// <param name="fileName"></param>
    private Task SaveImageStreamAsFile(string filePath, Stream inputStream, string fileName)
    {
        var imageFormat = GetImageFormatFromFileName(fileName);
        if (imageFormat == null)
        {
            _consoleWrite.WriteBoxColor("SaveStreamAsFile: FORMAT IS NULL!", ConsoleColor.Yellow, ConsoleColor.Black);
            return Task.CompletedTask;
        }

        DirectoryInfo directoryToSaveFiles = new DirectoryInfo(filePath);
        if (!directoryToSaveFiles.Exists)
        {
            directoryToSaveFiles.Create();
            _consoleWrite.WriteBoxColor("PATH CREATED!", ConsoleColor.Cyan, ConsoleColor.Black);
        }

        string path = Path.Combine(filePath, fileName);
        if (Path.Exists(path))
        {
            if (!_fileCheckerService.AreFilesEqual(inputStream, path).Result)
            {
                string? tmpName;
                do
                {
                    int i = 0;
                    tmpName = new Guid().ToString() + Path.GetExtension(fileName);
                    _consoleWrite.WriteTextColor($"FileName: [{fileName}] already exist, tried new name: {i++} times.", ConsoleColor.Yellow);
                    path = Path.Combine(filePath, tmpName);
                }
                while (Path.Exists(path));

                using (FileStream outputFileStream = new FileStream(tmpName, FileMode.Create))
                {
                    _consoleWrite.WriteBoxColor("DOWNLOADED - IMAGE", ConsoleColor.Green, ConsoleColor.Black);
                    inputStream.CopyTo(outputFileStream);
                }
                return Task.CompletedTask;
            }
            else
            {
                _consoleWrite.WriteBoxColor("IMAGE Already exists.", ConsoleColor.DarkGray, ConsoleColor.White);
                return Task.CompletedTask;
            }
        }
        else
        {

            try
            {
                using (FileStream outputFileStream = new FileStream(path, FileMode.Create))
                {
                    inputStream.CopyTo(outputFileStream);
                    _consoleWrite.WriteBoxColor("DOWNLOADED - IMAGE", ConsoleColor.Green, ConsoleColor.Black);
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }
    }
    #endregion
}