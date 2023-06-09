using DataAccessLibrary;
using HtmlAgilityPack;
// https://html-agility-pack.net/documentation

namespace Service;

public class CrawlerService
{
    private readonly HttpClient _httpClient;
    private readonly HtmlParserService _htmlParserService;
    private readonly DownloadService _downloadService;
    private readonly ConsoleWriteService _consoleWriteService;
    private readonly ResultData _sqlDataAccess;

    public CrawlerService()
    {
        _httpClient = new HttpClient();
        _htmlParserService = new HtmlParserService();
        _downloadService = new DownloadService();
        _consoleWriteService = new ConsoleWriteService();
        _sqlDataAccess = new ResultData();
    }

    /// <summary>
    /// Eg. www.google.com", 4, 450, "C:\\Users\\Kristoffer\\Desktop\\newTest
    /// minPixel not in use atm.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="maxDepth"></param>
    /// <param name="minPixel"></param>
    /// <param name="envPath"></param>
    /// <returns></returns>
    public async Task Crawl(string url, int maxDepth, int minPixel, string envPath)
    {
        var visitedUrls = new HashSet<string>();
        var queue = new Queue<Tuple<string, int>>();
        //var oldUrls = await _sqlDataAccess.GetVisitedUrl();
        //foreach (var q in oldUrls)
        //    if(q.Url != null)
        //        visitedUrls.Add(q.Url);
        queue.Enqueue(Tuple.Create(url, 0));
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var currentUrl = current.Item1;
            var currentDepth = current.Item2;

            if (currentDepth > maxDepth)
                continue;

            if (visitedUrls.Contains(currentUrl))
                continue;

            visitedUrls.Add(currentUrl);

            var htmlDocument = GetHtmlDocument(currentUrl);
            if (htmlDocument == null)
                continue;

            var imageUrlList = await _htmlParserService.GetImageUrls(htmlDocument);
            var videoUrlList = await _htmlParserService.GetVideoUrls(htmlDocument);
            
            // Endast för att läsa Console.
            _consoleWriteService.WriteBoxColor($"Visiting: {currentUrl} and found {imageUrlList.Count} new Images.", ConsoleColor.White, ConsoleColor.Black);
            _consoleWriteService.WriteBoxColor($"Visiting: {currentUrl} and found {videoUrlList.Count} new Videos.", ConsoleColor.White, ConsoleColor.Black);
            Task.Delay(1000).Wait();
            // Kommentera ut vid behov.

            foreach (var imageUrl in imageUrlList)
            {
                await _downloadService.SaveImage(imageUrl, envPath);
            }
            foreach (var videoUrl in videoUrlList)
            {
                await _downloadService.SaveVideo(videoUrl, envPath);
            }


            var links = await _htmlParserService.GetLinks(htmlDocument);
            foreach (var link in links)
            {
                if (IsAbsoluteUrl(link))
                {
                    queue.Enqueue(Tuple.Create(link, currentDepth + 1));
                }
                else
                {
                    var absoluteLink = GetAbsoluteUrl(currentUrl, link);
                    queue.Enqueue(Tuple.Create(absoluteLink, currentDepth + 1));
                }
            }
        }
    }

    private bool IsAbsoluteUrl(string url)
    {
        Uri uriResult;
        return Uri.TryCreate(url, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    private string GetAbsoluteUrl(string currentUrl, string link)
    {
        Uri currentUri = new Uri(currentUrl);
        Uri absoluteUri = new Uri(currentUri, link);
        return absoluteUri.ToString();
    }

    /// <summary>
    /// Gets the full HTML Document
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private HtmlDocument GetHtmlDocument(string url)
    {
        try
        {
            var response = _httpClient.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                var htmlStream = response.Content.ReadAsStreamAsync().Result;
                var htmlDocument = new HtmlDocument();
                htmlDocument.Load(htmlStream);
                return htmlDocument;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return null;
    }
}
