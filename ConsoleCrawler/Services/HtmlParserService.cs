using HtmlAgilityPack;
// https://html-agility-pack.net/documentation

namespace Service;
public class HtmlParserService
{
    public Task<List<string>> GetImageUrls(HtmlDocument htmlDocument)
    {
        var imageUrls = new List<string>();
        var imgElements = htmlDocument.DocumentNode.Descendants("img");

        foreach (var img in imgElements)
        {
            var src = img.Attributes["src"]?.Value;

            if (!string.IsNullOrEmpty(src))
            {
                imageUrls.Add(src);
            }
        }

        return Task.FromResult(imageUrls);
    }

    public Task<List<string>> GetVideoUrls(HtmlDocument htmlDocument)
    {
        var videoUrls = new List<string>();
        var videoElements = htmlDocument.DocumentNode.Descendants("source");
        foreach (var video in videoElements)
        {
            if (video.Attributes["type"]?.Value == "video/mp4")
            {
                var src = video.Attributes["src"]?.Value;

                if (!string.IsNullOrEmpty(src))
                {
                    videoUrls.Add(src);
                }
            }
            // Blob etc
        }

        return Task.FromResult(videoUrls);
    }

    public Task<List<string>> GetLinks(HtmlDocument htmlDocument)
    {
        var linkUrls = new List<string>();

        var aElements = htmlDocument.DocumentNode.Descendants("a");

        foreach (var a in aElements)
        {
            var href = a.Attributes["href"]?.Value;

            if (!string.IsNullOrEmpty(href))
            {
                linkUrls.Add(href);
            }
        }

        return Task.FromResult(linkUrls);
    }
}
