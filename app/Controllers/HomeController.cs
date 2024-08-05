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
            siteMapService.GenerateSiteMap(listNews.ToList(), skip, pageid );
             
        } 
        siteMapService.AddOrUpdateSiteMapIndex();
        return View();
    }
 


}


