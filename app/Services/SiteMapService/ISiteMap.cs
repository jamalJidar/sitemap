using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public DirectoryInfo CreateRootFolder(string RootFolder, out bool exit);
        public Task<(int, int, bool)> Config(SiteMapType type);

        public void AddOrUpdateSiteMapIndex();

        public Task ListData(List<SiteMapProperty> list, SiteMapType type, int index, int Page);
        public void WriteTag(SiteMapProperty t);
        public void WriteTag(SiteMapProperty t, SiteMapType type);
        public void AppendTag(SiteMapProperty t, string Url, SiteMapType type);
        public void AppendTag(SiteMapProperty t, string Url);
        public bool CheckExistsJsonFile();
        public Task<List<JsonList>> WriteJsonFile();
        public Task<List<JsonList>> WriteJsonFile(List<JsonList> list);
        public Task<(List<JsonList>, bool)> ReadJsonFile();
        public void GeneratePostSiteMap(List<SiteMapProperty> _news, int pageSize, int page);
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
        public void GeneratePostSiteMap(List<SiteMapProperty> _news, int pageSize, int page)
        {

            XmlWriter writer = XmlWriter.Create($"{env.WebRootPath}/sitemap/SiteMap{page}.xml");

            writer.WriteStartDocument();
            //http://www.sitemaps.org/schemas/sitemap/0.9
            writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
            if (page == 1)
            {
                WriteTag("1", "Daily", LinkSite, DateTime.Now, writer);

                WriteTag("1", "Daily", LinkSite + "/Home/Index", DateTime.Now, writer);
            }
            WriteJsonFile(new List<JsonList?>()
            {
                new JsonList()
                {
                    Count = pageSize,
                    Page = page,
                    Type= SiteMapType.Post
                }
            });
            foreach (var item in _news)
            {//"0.9", "Daily", string.Format("{0}s/{1}", LinkSite, item.Navigation), item.lastmod,
                item.Writer = writer;
                WriteTag(item);

            }

            writer.WriteEndDocument();

            writer.Close();
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
        public bool CheckHasFile(string FilenName) => File.Exists($"{env.WebRootPath}/sitemap/{FilenName}");
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
            foreach (var item in listfile.Where(x => x != "SiteMap_index.xml"))
            {
                string url =  item.Remove(0, item.IndexOf("/sitemap"));
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




        public bool CheckExistsJsonFile()
         => System.IO.File.Exists($"{env.WebRootPath}/JsonFile.json");
        public async Task<List<JsonList>> WriteJsonFile()
        {
            List<JsonList> list = new List<JsonList>();
            foreach (SiteMapType item in (SiteMapType[])Enum.GetValues(typeof(SiteMapType)))
            {
                list.Add(new JsonList()
                {
                    Count = 11,
                    Page = 1,
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

        public Task ListData(List<SiteMapProperty> list, SiteMapType type, int index, int Page)
        {
            throw new NotImplementedException();
        }

      
    }
}
