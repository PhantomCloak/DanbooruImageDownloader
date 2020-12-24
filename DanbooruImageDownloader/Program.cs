#define INTERACTIVE
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Konsole;

namespace DanbooruImageDownloader
{
    class Program
    {
        private const string TemplateUrl = "https://danbooru.donmai.us/posts?page={page}&tags={tag}";
        private const string BaseUrl = "https://danbooru.donmai.us/";

        private static string ProxyServer { get; set; }
        private static string ProxyUsername { get; set; }
        private static string ProxyPassword { get; set; }
        private static string PostTag { get; set; }
        private static string SavePath { get; set; }
        private static int PageLimit { get; set; }
        private static int Downloaded { get; set; }
        private static bool CompressedOnly { get; set; }

        static void Main(string[] args)
        {
#if !INTERACTIVE
            bool success = InteractiveSession();

            if (!success)
            {
                Console.WriteLine("an error occured during parsing please try again");
                Console.ReadLine();
                return;
            }
#else
            if (args.Length <= 0)
                throw new ArgumentNullException();

            for (int i = 0; i < args.Length; i += 2)
            {
                var paramName = args[i];
                var paramValue = args[i + 1];

                switch (paramName)
                {
                    case "--proxy-address":
                    {
                        ProxyServer = paramValue;
                    }
                        break;
                    case "--proxy-username":
                    {
                        ProxyUsername = paramValue;
                    }
                        break;
                    case "--proxy-password":
                    {
                        ProxyPassword = paramValue;
                    }
                        break;
                    case "--tag":
                    {
                        PostTag = paramValue;
                    }
                        break;
                    case "--page-limit":
                    {
                        PageLimit = int.Parse(paramValue);
                    }
                        break;
                    case "--save-path":
                    {
                        if (!Directory.Exists(paramValue))
                            throw new DirectoryNotFoundException("Specified directory not found.");

                        SavePath = paramValue;
                    }
                        break;
                    case "--compressed-only":
                    {
                        CompressedOnly = bool.Parse(paramValue);
                    }
                        break;
                }
            }
#endif

            for (int i = 1; i < PageLimit + 1; i++)
            {
                var pageContent = GetPageHtmlContent(PostTag, i);
                var pagePosts = ParsePosts(pageContent);

                foreach (var postUrl in pagePosts)
                {
                    var postContent = GetPostHtmlContent(postUrl);
                    var postImgUrl = ParsePostImage(postContent, CompressedOnly);
                    var imgExtension = Path.GetExtension(postImgUrl);

                    Task.Run(async () => { await DownloadImageAsync(postImgUrl, SavePath + "/img_" + Downloaded + imgExtension); }).Wait();
                    Downloaded++;
                }
            }
        }

        private static string GetPageHtmlContent(string tag, int pageNumber)
        {
            var client = new WebClient{Proxy = GetProxy()};
            
            string requestUrl = TemplateUrl.Replace("{page}", pageNumber.ToString());
            requestUrl = requestUrl.Replace("{tag}", tag);

            return client.DownloadString(requestUrl);
        }

        private static WebProxy GetProxy()
        {
            if (string.IsNullOrEmpty(ProxyServer))
            {
                return null;
            }
            
            return new WebProxy
            {
                Address = new Uri(ProxyServer),
                Credentials = new NetworkCredential(ProxyUsername, ProxyPassword)
            };
        }

        private static string GetPostHtmlContent(string url)
        {
            var client = new WebClient
            {
                Proxy = GetProxy()
            };

            return client.DownloadString(url);
        }

        private static async Task DownloadImageAsync(string imgUrl, string path)
        {
            var pb = new ProgressBar(PbStyle.SingleLine, 100);
            var client = new WebClient
            {
                Proxy = GetProxy()
            };

            client.DownloadProgressChanged += (s, e) => { pb.Refresh(e.ProgressPercentage, $"Downloading: {Path.GetFileName(path)}"); };

            client.DownloadFileCompleted += (sender, args) => { pb.Refresh(100, $"Completed {Path.GetFileName(path)}"); };
            await client.DownloadFileTaskAsync(new Uri(imgUrl), path);
        }

        private static string[] ParsePosts(string htmlContent)
        {
            var document = new HtmlDocument();
            document.LoadHtml(htmlContent);
            var collection = document.DocumentNode.SelectNodes("//a");

            return collection
                .Select(x => x.Attributes["href"].Value)
                .Where(x => x.StartsWith("/posts/") && !x.Contains("tag"))
                .Select(x => BaseUrl + x).ToArray();
        }

        private static string ParsePostImage(string htmlContent, bool compressedOnly)
        {
            var document = new HtmlDocument();
            document.LoadHtml(htmlContent);

            if (!compressedOnly)
            {
                var sections = document.DocumentNode.Descendants("section");

                foreach (var section in sections)
                {
                    var sectionId = section.Id;

                    if (sectionId != "post-options") continue;

                    var sectionMembers = section.Descendants("li");

                    foreach (var sectionMember in sectionMembers)
                    {
                        if (sectionMember.Id != "post-option-view-original") continue;

                        var embedUrl = sectionMember.ChildNodes
                            .Select(x => x.GetAttributeValue("href", null))
                            .FirstOrDefault(x => !string.IsNullOrEmpty(x) && x.StartsWith("https://"));

                        if (embedUrl == null)
                        {
                            break;
                        }

                        return embedUrl;
                    }
                }
            }

            var mainImgNode = document.DocumentNode.SelectNodes("//*[@id=\"image\"]");

            return mainImgNode.First().GetAttributeValue("src", null);
        }

        private static bool InteractiveSession()
        {
            Console.Write("Tag (under score for spaces): ");
            string tag = Console.ReadLine();

            if (string.IsNullOrEmpty(tag))
            {
                Console.WriteLine("Tag cannot be null");
                return false;
            }

            if (tag.Any(Char.IsWhiteSpace))
            {
                Console.WriteLine("Tag cannot contain white spaces use underscore instead");
                return false;
            }

            PostTag = tag;

            Console.Write("Page limit: ");

            try
            {
                int pageLimit = int.Parse(Console.ReadLine());
                PageLimit = pageLimit;
            }
            catch
            {
                Console.WriteLine("Please enter a valid number");
                return false;
            }

            Console.Write("Save path (empty for current directory): ");
            string path = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(path))
            {
                Directory.CreateDirectory("../img");
                path = "../img";
            }

            if (!Directory.Exists(path))
            {
                Console.WriteLine("Specified save path doesn't exist");
                return false;
            }

            SavePath = path;

            Console.Write("Download full-resolution image? (y/n): ");
            var compressed = Console.ReadLine();
            
            if (compressed.ToLower() != "y")
            {
                CompressedOnly = true;
            }

            
            Console.Write("Use proxy? (y/n):");
            string answer = Console.ReadLine();

            if (answer.ToLower() != "y")
            {
                return true;
            }

            Console.Write("Proxy Address: ");
            string proxyAddress = Console.ReadLine();
            ProxyServer = proxyAddress;

            Console.Write("Proxy Username : ");
            string proxyUsername = Console.ReadLine();

            if (!string.IsNullOrEmpty(proxyUsername))
            {
                ProxyUsername = proxyUsername;
            }


            Console.Write("Proxy Password : ");
            string proxyPassword = Console.ReadLine();

            if (!string.IsNullOrEmpty(proxyPassword))
            {
                ProxyPassword = proxyPassword;
            }

            return true;
        }
    }
}