using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using app.Models;
using app.Services.SiteMapService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SiteMap.Models;
using SiteMap.Services.PagenationService;

namespace SiteMap.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment env;
    private readonly ISiteMapService siteMapService;
    string LinkSite = "LinkSite";
    public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env, ISiteMapService siteMapService)
    {
        _logger = logger;
        this.env = env;
        this.siteMapService = siteMapService;
    }

    public async Task<IActionResult> Index()
    {

        var item = await siteMapService.Config(SiteMapType.Post);
        Post post = new Post();
        var list = post.posts();
        int pageid = 1;
        int pageSize = 20;
        int skip = (pageid - item.Item2) * pageSize;
        int Count = list.Count;

        for (int i = 1; i <= ((Count / pageSize) + 1); i++)
        {
            pageid = i;
            skip = (pageid - 1) * pageSize;

            var listNews = post.posts().OrderBy(x => x.Id).Skip(skip).Take(pageSize)
                               .Select(x => new SiteMapProperty()
                               {
                                   Freq = ChangeFreq.None,
                                   lastmod = x.DateCrete,
                                   Navigation = x._slug,
                                   Priority = 1,
                                   Writer = null

                               });
            siteMapService.GeneratePostSiteMap(listNews.ToList(), skip, pageid);
             
        } 
        siteMapService.AddOrUpdateSiteMapIndex();
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
            writer.WriteEndElement();
        }

        writer.Close();
        writer.FlushAsync();
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
     => System.IO.File.Exists($"{env.WebRootPath}/sitemap/{filename}");


}


