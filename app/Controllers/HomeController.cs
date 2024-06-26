using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using SiteMap.Models;

namespace SiteMap.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment env;
    string LinkSite = "LinkSite";
    public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        this.env = env;
    }

    public IActionResult Index()
    {
        Post post = new Post();
        var listNews = post.posts();
        int pageid = 1;
        int page = 20;
        int skip = (pageid - 1) * page;
        int Count = post.posts().Count();
        for (int i = 1; i <= Count / page; i++)
        {
            skip = (i) * page;

            if ((Count - skip) < page)
            {
                GeneratePostSiteMap(listNews.OrderByDescending(x => x.DateCrete).Skip(skip).Take(page).ToList(), i);
                System.Console.WriteLine("append");
            }

            else
            {
                if (!ChecKHasFile($"SiteMap{i}.xml"))
                {
                    GeneratePostSiteMap(listNews.OrderByDescending(x => x.DateCrete).Skip(skip).Take(page).ToList(), i);
                    System.Console.WriteLine("new file ...");
                }


            }

        }



        /*
       <urlset>
       <url>
       <loc>LinkSite/post/8/title8</loc>
       <lastmod>2024-06-26T19:56:01.8937031+03:30</lastmod>
       <changefreq>Daily</changefreq>
       <priority>0.9</priority>
       </url>
       </urlset>
        */
        return View();
    }





    private void GeneratePostSiteMap(List<Post> posts, int index)
    {


        XmlWriter writer = XmlWriter.Create($"{env.WebRootPath}/sitemap/SiteMap{index}.xml");

        writer.WriteStartDocument();
        writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
        if (index == 1)
        {
            WriteTag("1", "Daily", LinkSite, DateTime.Now, writer);

            WriteTag("1", "Daily", LinkSite + "/Home/Index", DateTime.Now, writer);
        }

        foreach (var item in posts)
        {
            WriteTag("0.9", "Daily", LinkSite + item._slug, item.DateCrete, writer);
        }

        writer.WriteEndDocument();

        writer.Close();
    }


    private void WriteTag(string Priority, string freq, string Navigation, DateTime lastmod, XmlWriter MyWriter)
    {
        MyWriter.WriteStartElement("url");

        MyWriter.WriteStartElement("loc");
        MyWriter.WriteValue(Navigation);
        MyWriter.WriteEndElement();

        MyWriter.WriteStartElement("lastmod");
        MyWriter.WriteValue(lastmod);
        MyWriter.WriteEndElement();

        MyWriter.WriteStartElement("changefreq");
        MyWriter.WriteValue(freq);
        MyWriter.WriteEndElement();

        MyWriter.WriteStartElement("priority");
        MyWriter.WriteValue(Priority);
        MyWriter.WriteEndElement();

        MyWriter.WriteEndElement();
    }
    private void AppendTag(string url)
    {
        XDocument doc = XDocument.Load(url);
        XElement demoNode = new XElement("url");
        demoNode.Add(new XElement("loc", "test"));
        demoNode.Add(new XElement("lastmod", "test"));
        demoNode.Add(new XElement("changefreq", DateTime.Now));
        demoNode.Add(new XElement("priority", 0.9));
        doc.Document.Root.Add(demoNode);//(demoNode);
        doc.Save($"{env.WebRootPath}/sitemap/SiteMap{10}.xml");
    }
    private bool ChecKHasFile(string filename)
    {
        return System.IO.File.Exists($"{env.WebRootPath}/sitemap/{filename}");

    }
}


public class Post
{

    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public DateTime DateCrete { get; set; }
    public string Slug { get; set; }

    public string _slug
    {
        get
        {
            return $"/post/{this.Id}/{this.Title}";
        }
    }

    public List<Post> posts()
    {
        List<Post> list = new List<Post>();
        for (int i = 1; i < 353; i++)
        {
            list.Add(new Post()
            {
                Id = i,
                DateCrete = DateTime.Now.AddHours(i),
                Title = $"title{i}"


            });
        }

        return list;
    }



}
