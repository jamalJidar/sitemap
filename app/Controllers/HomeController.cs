using app.Models;
using app.Services.SiteMapService;
using Microsoft.AspNetCore.Mvc;
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
        int JsonTotalCount = item.Item1;
        var NewConfig = item.Item3;
        bool order = false;
        Post post = new Post();
        var list = post.posts();
        int pageid = 1;
        int pageSize = 20;
        int TotlaCount = 0;
        if (JsonTotalCount < 1)
            TotlaCount = list.Count();
        else if ((list.Count() - JsonTotalCount) == 0)
        {
            return View();
        }
        else
        {
            JsonTotalCount = ((list.Count() - JsonTotalCount));
            order = true;
        }
        int skip = (pageid - item.Item2) * pageSize;
        for (int i = 1; i <= ((TotlaCount / pageSize) + 1); i++)
        {
            pageid = i;
            skip = (pageid - 1) * pageSize;
            var listNews =
                !order ?
                post.posts().OrderBy(x => x.Id).Skip(skip).Take(pageSize)
                               .Select(x => new SiteMapProperty()
                               {
                                   Freq = ChangeFreq.None,
                                   lastmod = x.DateCrete,
                                   Navigation = x._slug,
                                   Priority = 1,
                                   Writer = null

                               })
                               :
               post.posts().OrderByDescending(x => x.Id).Take(JsonTotalCount)
                               .Select(x => new SiteMapProperty()
                               {
                                   Freq = ChangeFreq.None,
                                   lastmod = x.DateCrete,
                                   Navigation = x._slug,
                                   Priority = 1,
                                   Writer = null

                               })
                               ;
            Print(listNews.ToList());
            siteMapService.Generate(listNews.ToList(), (list.Count() / pageSize), i, SiteMapType.Post);
        }
        // siteMapService.Generate(listNews.ToList(), skip, pageid, SiteMapType.Post);
        //siteMapService.Generate(listNews.ToList(), skip, pageid, SiteMapType.Catgory);
        //siteMapService.Generate(listNews.ToList(), skip, pageid, SiteMapType.Tag);
        siteMapService.AddOrUpdateSiteMapIndex();
        return View();
    }

    private void Print(List<SiteMapProperty> sites)
    {
        sites.ForEach((x) =>
        {
            Console.WriteLine(x.Navigation);
        });
    }

}


