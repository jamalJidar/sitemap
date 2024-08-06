using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using app.Models;
using Newtonsoft.Json;
namespace app.Services.SiteMapService
{
    public interface ISiteMapService
    {
        public bool CheckHasFile(string FilenName);
        public bool CheckHasFolder(string FolderName);
        public DirectoryInfo CreateFolder(string FolderName);
        public DirectoryInfo CreateRootFolder(string RootFolder, out bool exit);
        public Task<(int, int, bool)> Config(SiteMapType type);

        public void AddOrUpdateSiteMapIndex();
        public void WriteTag(SiteMapProperty t);
        public void WriteTag(SiteMapProperty t, SiteMapType type);
        public void AppendTag(SiteMapProperty t, string Url, SiteMapType type);
        public void AppendTag(SiteMapProperty t, string Url);
        public bool CheckExistsJsonFile();
        public Task<List<JsonList>> WriteJsonFile();
        public Task<List<JsonList>> WriteJsonFile(List<JsonList> list);
        public Task<(List<JsonList>, bool)> ReadJsonFile();
        public Task<(List<JsonList>, bool)> UpdateJsonFile(JsonList json);
        public void Generate(List<SiteMapProperty> _news, int pageSize, int page, SiteMapType type);
        public void GenerateSiteMap(List<SiteMapProperty> _news, int pageSize, int page, SiteMapType type);
        public void GenerateSiteMapImage(List<SiteMapProperty> _news, int pageSize, int page);
        public void GenerateSiteMapOther(List<SiteMapProperty> listNews, int index, SiteMapType type);

    }
    public class SiteMapService : ISiteMapService
    {
        private const int padinate = 20;
        private readonly IWebHostEnvironment env;
        private readonly IConfiguration configuration;
        const string LinkSite = "https://localhost:7260";
        public SiteMapService(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.env = env;
            this.configuration = configuration;
        }
        public async void Generate(List<SiteMapProperty> _news, int pageSize, int page, SiteMapType type)
        {
            CreateFolder(Utility.EnumExtensions.GetDisplayName(type));
            switch (type)
            {
                case SiteMapType.Post:
                    GenerateSiteMap(_news, pageSize, page, type);
                    break;
                case SiteMapType.Image:
                    GenerateSiteMapImage(_news, pageSize, page);
                    break;
                case SiteMapType.Tag:
                    GenerateSiteMapOther(_news, pageSize, type);
                    break;
                case SiteMapType.Catgory:
                    GenerateSiteMapOther(_news, pageSize, type);
                    break;
                case SiteMapType.SiteMapIndex:
                    AddOrUpdateSiteMapIndex();
                    break;
                default:
                    break;
            }


           await UpdateJsonFile(
                new JsonList()
                {
                    Count = pageSize,
                    Page = page,
                    Type=  type
                }
            );
        }
        public void GenerateSiteMapOther(List<SiteMapProperty> list, int index, SiteMapType type)
        {
            XmlWriter writer = XmlWriter.Create($"{env.WebRootPath}/sitemap/{Utility.EnumExtensions.GetDisplayName(type)}/{index}.xml");
            writer.WriteStartDocument();
            writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
            foreach (var item in list)
            {
                WriteTag("0.9", "Daily", $"{LinkSite}/tag/{item}", DateTime.Now, writer);
            }
            writer.WriteEndDocument();
            writer.Close();
        }
        public void GenerateSiteMap(List<SiteMapProperty> _news, int pageSize, int page, SiteMapType type)
        {

            XmlWriter writer = XmlWriter.Create($"{env.WebRootPath}/sitemap/{Utility.EnumExtensions.GetDisplayName(type)}/{page}.xml");
            writer.WriteStartDocument();
            writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
            if (page == 1)
            {
                WriteTag("1", "Daily", LinkSite, DateTime.Now, writer);
                WriteTag("1", "Daily", LinkSite + "/Home/Index", DateTime.Now, writer);
            }
            foreach (var item in _news)
            {//"0.9", "Daily", string.Format("{0}s/{1}", LinkSite, item.Navigation), item.lastmod,
                item.Writer = writer;
                WriteTag(item);
            }
            writer.WriteEndDocument();
            writer.Close();
        }
        public void GenerateSiteMapImage(List<SiteMapProperty> listNews, int pageSize, int page)
        {

            XNamespace nsSitemap = "http://www.sitemaps.org/schemas/sitemap/0.9";
            XNamespace nsImage = "http://www.google.com/schemas/sitemap-image/1.1";
            var ImageSiteMap = new XDocument(new XDeclaration("1.0", "UTF-8", ""));

            var
              Image = new XElement(nsSitemap + "urlset",
              new XAttribute("xmlns", nsSitemap),
              new XAttribute(XNamespace.Xmlns + "image", nsImage));

            foreach (var item in listNews)
            {
                //string.Format("{0}/s/{1}/{2}", LinkSite, ">>>>>", item.ShortLink)
                //<loc>https://filetap.ir/s/m7nkr</loc>
                Image.Add(new XElement(nsSitemap + "url",
                 new XElement(nsSitemap + "loc", $"{LinkSite}/s/{item.Navigation}"),
                 new XElement(nsImage + "image",
                 new XElement(nsImage + "loc", string.Format("{0}{1}", LinkSite, item.Navigation)))));
            }

            ImageSiteMap.Add(Image);
            ImageSiteMap.Save($"{env.WebRootPath}/sitemap/{Utility.EnumExtensions.GetDisplayName(SiteMapType.Image)}/{page}.xml");
        }
        public void WriteTag(SiteMapProperty _siteMap)
        {


            _siteMap.Writer.WriteStartElement("url");

            _siteMap.Writer.WriteStartElement("loc");
            _siteMap.Writer.WriteValue(_siteMap.Navigation);
            _siteMap.Writer.WriteEndElement();

            _siteMap.Writer.WriteStartElement("lastmod");
            _siteMap.Writer.WriteValue(_siteMap.lastmod);
            _siteMap.Writer.WriteEndElement();

            _siteMap.Writer.WriteStartElement("changefreq");
            _siteMap.Writer.WriteValue(_siteMap._ferq);
            _siteMap.Writer.WriteEndElement();

            _siteMap.Writer.WriteStartElement("priority");
            _siteMap.Writer.WriteValue(_siteMap._priority);
            _siteMap.Writer.WriteEndElement();
            _siteMap.Writer.WriteEndElement();
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
        public void AppendTag(SiteMapProperty _siteMapProperty, string url)
        {
            XDocument doc = XDocument.Load(url);
            XElement demoNode = new XElement("url");
            demoNode.Add(new XElement("loc", _siteMapProperty.Navigation));
            demoNode.Add(new XElement("lastmod", _siteMapProperty.lastmod));
            demoNode.Add(new XElement("changefreq", _siteMapProperty.Freq));
            demoNode.Add(new XElement("priority", _siteMapProperty.Priority));
            doc.Document.Root.Add(demoNode);//(demoNode);
            doc.Save($"{env.WebRootPath}/sitemap/post{10}.xml");
        }
        public void AppendTag(SiteMapProperty _siteMapProperty, string url, SiteMapType type)
        {
            XDocument doc = XDocument.Load(url);
            XElement demoNode = new XElement("url");
            demoNode.Add(new XElement("loc", _siteMapProperty.Navigation));
            demoNode.Add(new XElement("lastmod", _siteMapProperty.lastmod));
            doc.Document.Root.Add(demoNode);//(demoNode);
            doc.Save($"{env.WebRootPath}/sitemap/{Utility.EnumExtensions.GetDisplayName(type)}.xml");
        }
        public bool CheckHasFile(string FilenName) => File.Exists($"{env.WebRootPath}/sitemap/{FilenName}");
        public bool CheckHasFolder(string FolderName) => File.Exists($"{env.WebRootPath}/sitemap/{FolderName}");
        public DirectoryInfo CreateFolder(string FolderName) => !CheckHasFolder(FolderName) ?
            System.IO.Directory.CreateDirectory($"{env.WebRootPath}/sitemap/{FolderName}") : null;
        public async Task<(int, int, bool)> Config(SiteMapType type)
        {
            var Result = await ReadJsonFile();
            var json = Result.Item1;
            var item = json.Where(x => x.Type == type).FirstOrDefault();
            return (item.Count, item.Page, Result.Item2);
        }
        public async Task<(List<JsonList>, bool)> ReadJsonFile()
        {
            if (!CheckExistsJsonFile()) { return (await WriteJsonFile(), false); }
            List<JsonList> list = new List<JsonList>();
            var _f = System.IO.File.ReadAllLines($"{env.WebRootPath}/JsonFile.json");
            var item = JsonConvert.DeserializeObject<List<JsonList>>(_f[0]);
            list.AddRange(item);
            return (list, true);
        }
        public void AddOrUpdateSiteMapIndex()
        {
            XmlWriter writer = XmlWriter.Create($"{env.WebRootPath}/sitemap/SiteMap_index.xml");
            writer.WriteStartDocument();
            writer.WriteStartElement("sitemapindex", "http://www.sitemaps.org/schemas/sitemap/0.9");
            var listfile = ListSiteMap();
            Console.WriteLine(" count site map : " + listfile.Count());
            foreach (var item in listfile.Where(x => !x.Contains("SiteMap_index.xml")))
            {
                string url = item.Remove(0, item.IndexOf("/sitemap"));
                WriteTag(new SiteMapProperty()
                {
                    Navigation = $"{LinkSite}{url}",
                    lastmod = DateTime.Now,
                    Writer = writer
                }, SiteMapType.SiteMapIndex);
            }
            writer.WriteEndDocument();
            writer.Close();
            writer.Dispose();

        }
        public void WriteTag(SiteMapProperty t, SiteMapType type)
        {
            t.Writer.WriteStartElement("sitemap");

            t.Writer.WriteStartElement("loc");
            t.Writer.WriteValue(t.Navigation);
            t.Writer.WriteEndElement();

            t.Writer.WriteStartElement("lastmod");
            t.Writer.WriteValue(t.lastmod);
            t.Writer.WriteEndElement();

            t.Writer.WriteEndElement();
        }
        public string[] ListSiteMap(string path = @"/sitemap")
          => Directory.GetFiles($"{env.WebRootPath}{path}", "*.xml", SearchOption.AllDirectories);
        public DirectoryInfo CreateRootFolder(string RootFolder, out bool exit)
        {
            exit = true;
            if (!System.IO.Directory.Exists($"{env.WebRootPath}/{RootFolder}"))
            {
                exit = false;
                return System.IO.Directory.CreateDirectory($"{env.WebRootPath}/{RootFolder}");
            }
            return new DirectoryInfo($"{env.WebRootPath}/{RootFolder}");
        }
        private string sitemapLocation() => configuration.GetSection("sitemapLocation").Value;
        public bool CheckExistsJsonFile()
         => System.IO.File.Exists($"{env.WebRootPath}/JsonFile.json");
        public async Task<List<JsonList>> WriteJsonFile()
        {
            List<JsonList> list = new List<JsonList>();
            foreach (SiteMapType item in (SiteMapType[])Enum.GetValues(typeof(SiteMapType)))
            {
                list.Add(new JsonList()
                {
                    Count = 0,
                    Page = 1,
                    Type = item
                });

                Console.WriteLine(item);
            }
            string output = JsonConvert.SerializeObject(list);
            using (FileStream fs = new FileStream($"{env.WebRootPath}/JsonFile.json", FileMode.Create))
            {
                byte[] writeArr = Encoding.UTF8.GetBytes(output);
                await fs.WriteAsync(writeArr, 0, output.Length);
                fs.Close();
            }
            return list;
        }
        public async Task<List<JsonList>> WriteJsonFile(List<JsonList> list)
        {
            var Result = await ReadJsonFile();
            var temp = Result.Item1;
            if (!CheckExistsJsonFile())
            { await WriteJsonFile(); }
            else
            {
                foreach (var item in list)
                {
                    var _jsonItem = temp.Where(x => x.Type == item.Type).FirstOrDefault();
                    _jsonItem.Count += item.Count;
                    _jsonItem.Page = item.Page;
                }
                string output = JsonConvert.SerializeObject(list);
                using (FileStream fs = new FileStream($"{env.WebRootPath}/JsonFile.json", FileMode.Create))
                {
                    byte[] writeArr = Encoding.UTF8.GetBytes(output);
                    await fs.WriteAsync(writeArr, 0, output.Length);
                    fs.Close();
                }
            }
            return temp;
        }

        public async Task<(List<JsonList>, bool)> UpdateJsonFile(JsonList json)
        {

            var all = await ReadJsonFile();
            var single = all.Item1.Where(x => x.Type == json.Type).FirstOrDefault();
            all.Item1.Remove(single);

            single.Count = json.Count;
            single.Page = json.Page;
            single.Type = json.Type;

            all.Item1.Add(single);
            await WriteJsonFile(all.Item1);
            return (all.Item1 , true);

        }
    }
}
