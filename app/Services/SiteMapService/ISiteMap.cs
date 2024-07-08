using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public DirectoryInfo CreateRootFolder(string RootFolder, out bool exit);
        public  (int , int)  Config( SiteMapType type);

        public void AddOrUpdateSiteMapIndex();
        public string[] ListSiteMap(string path);
        public Task ListData(List<SiteMapProperty> list, SiteMapType type, int index, int Page);
        public void WriteTag(SiteMapProperty t);
        public void WriteTag(SiteMapProperty t, SiteMapType type);
        public void AppendTag(SiteMapProperty t, string Url, SiteMapType type);
        public void AppendTag(SiteMapProperty t, string Url);
        public bool CheckExistsJsonFile();
        public Task<List<JsonList>> WriteJsonFile();
        public Task<List<JsonList>> WriteJsonFile(List<JsonList> list);
        public Task<List<JsonList>> ReadJsonFile();
    }
    public class SiteMapService : ISiteMapService
    {
        private const int padinate = 20;
        private readonly IWebHostEnvironment env;
        private readonly IConfiguration configuration;
        const string LinkSite = "";
        public SiteMapService(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.env = env;
            this.configuration = configuration;
        }
        public bool CheckHasFile(string FilenName) =>
        File.Exists($"{env.WebRootPath}/sitemap/{FilenName}");
        public (int , int) Config(SiteMapType type )
        {
            var json = ReadJsonFile().Result;
            var item = json.Where(x => x.Type == type).FirstOrDefault();
             return  (item.Page, item.Count);
        }
        public void AddOrUpdateSiteMapIndex()
        {
            XmlWriter writer = XmlWriter.Create($"{env.WebRootPath}/sitemap/SiteMap_index.xml");
            writer.WriteStartDocument();
            writer.WriteStartElement("sitemapindex", "http://www.sitemaps.org/schemas/sitemap/0.9");
            var listfile = ListSiteMap();
            foreach (var item in listfile.Where(x => x != "SiteMap_index.xml"))
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
            writer = null;
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
        public void WriteTag(SiteMapProperty _siteMap, SiteMapType type)
        {
            _siteMap.Writer.WriteStartElement("loc");
            _siteMap.Writer.WriteValue(_siteMap.Navigation);
            _siteMap.Writer.WriteEndElement();
            _siteMap.Writer.WriteStartElement("lastmod");
            _siteMap.Writer.WriteValue(_siteMap.lastmod);
            _siteMap.Writer.WriteEndElement();

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
            _siteMap.Writer.WriteValue(_siteMap.Freq);
            _siteMap.Writer.WriteEndElement();
            _siteMap.Writer.WriteStartElement("priority");
            _siteMap.Writer.WriteValue(_siteMap.Priority);
            _siteMap.Writer.WriteEndElement();
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
        private string sitemapLocation() => configuration.GetSection("sitemapLocation").Value;
        public async Task ListData(List<SiteMapProperty> list, SiteMapType type, int index, int Page)
        {
            if (type != SiteMapType.Post)
            {
                XNamespace nsSitemap = "http://www.sitemaps.org/schemas/sitemap/0.9";
                XNamespace nsType = $"http://www.google.com/schemas/sitemap-{Utility.EnumExtensions.GetDisplayName(type)}/1.1";
                var ImageSiteMap = new XDocument(new XDeclaration("1.0", "UTF-8", ""));
                var
                 tag = new XElement(nsSitemap + "urlset",
                 new XAttribute("xmlns", nsSitemap),
                 new XAttribute(
                   $"{XNamespace.Xmlns}{Utility.EnumExtensions.GetDisplayName(type)}", nsType));
                foreach (var item in list)
                {
                    WriteTag(new SiteMapProperty()
                    {
                        Navigation = item.Navigation,
                        lastmod = item.lastmod
                    });
                }
                ImageSiteMap.Add(tag);
                ImageSiteMap.Save($"{env.WebRootPath}/sitemap/{Utility.EnumExtensions.GetDisplayName(type)}/{index}.xml");
            }
            else
            {
                XmlWriter writer = XmlWriter.Create($"{env.WebRootPath}/sitemap/{Utility.EnumExtensions.GetDisplayName(type)}{index}.xml");
                writer.WriteStartDocument();
                writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
                if (1 == 1)
                {
                    WriteTag(new SiteMapProperty()
                    {
                        Freq = ChangeFreq.None,
                        Navigation = LinkSite,
                        lastmod = DateTime.Now,
                        Writer = writer
                    });
                }
                foreach (var item in list)
                {
                    WriteTag(
                        new SiteMapProperty()
                        {
                            Freq = item.Freq,
                            lastmod = item.lastmod,
                            Navigation = item.Navigation,
                            Priority = item.Priority,
                            Writer = writer
                        });
                }
                writer.WriteEndDocument();
                writer.Close();
            }
        }
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
                    Page = 0,
                    Type = item
                });
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
        public async Task<List<JsonList>> WriteJsonFile(List<JsonList?> list)
        {
            var temp = await ReadJsonFile();
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
        public async Task<List<JsonList>> ReadJsonFile()
        {
            if (!CheckExistsJsonFile()) { return await WriteJsonFile(); }
            List<JsonList> list = new List<JsonList>();
            using (FileStream fs = new FileStream($"{env.WebRootPath}/JsonFile.json", FileMode.Open))
            {
                byte[] readArr = new byte[fs.Length];
                int count;
                while ((count = fs.Read(readArr, 0, readArr.Length)) > 0)
                {
                    var item = JsonConvert.DeserializeObject<JsonList>(Encoding.UTF8.GetString(readArr, 0, count));
                    list.Add(item);
                }
                fs.Close();
                Console.ReadKey();
            }
            return list;
        }

        
    }
}
