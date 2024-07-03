using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using app.Models;

namespace app.Services.SiteMapService
{
    public enum SiteMapType
    {
        Post,
        Image,
        Tag,
        Catgory
    }
    public interface ISiteMap
    {
        public bool CheckHasFile(string FilenName);
        public DirectoryInfo CreateRootFolder(string RootFolder, out bool exit);
        public void AddOrUpdateSiteMapIndex();
        public string[] ListSiteMap(string path);
        public void WriteTagIndex(string Navigation, DateTime lastmod, XmlWriter MyWriter);
        public void WriteTagIndex(string Priority, string freq, string Navigation, DateTime lastmod, XmlWriter MyWriter);
        public void GenerateSiteMapPost();
        public void GenerateSiteMapImage();
        public void GenerateSiteMapTag();
        public void WriteTag();
        public void AppendTag(string url, string Navigation, DateTime lastmod);
        public void AppendTag(string url, string Priority, string freq, string Navigation, DateTime lastmod);

    }
    public class SiteMap : ISiteMap
    {
        private readonly IWebHostEnvironment env;
        const string LinkSite = "";
        public SiteMap(IWebHostEnvironment env)
        {
            this.env = env;
        }
        public bool CheckHasFile(string FilenName) =>
        File.Exists($"{env.WebRootPath}/sitemap/{FilenName}");
        public void AddOrUpdateSiteMapIndex()
        {
            XmlWriter writer = XmlWriter.Create($"{env.WebRootPath}/sitemap/SiteMap_index.xml");
            writer.WriteStartDocument();
            writer.WriteStartElement("sitemapindex", "http://www.sitemaps.org/schemas/sitemap/0.9");
            var listfile = ListSiteMap();
            foreach (var item in listfile.Where(x => x != "SiteMap_index.xml"))
            {
                string url = item.Remove(0, item.IndexOf("/sitemap"));
                WriteTagIndex($"{LinkSite}{url}", DateTime.Now, writer);
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
            throw new NotImplementedException();
        }

        public void GenerateSiteMapImage()
        {
            throw new NotImplementedException();
        }

        public void GenerateSiteMapPost()
        {
            throw new NotImplementedException();
        }

        public void GenerateSiteMapTag()
        {
            throw new NotImplementedException();
        }

        public void WriteTag()
        {
            throw new NotImplementedException();
        }

        public void WriteTagIndex(string Navigation, DateTime lastmod, XmlWriter MyWriter)
        {
            MyWriter.WriteStartElement("sitemap");

            MyWriter.WriteStartElement("loc");
            MyWriter.WriteValue(Navigation);
            MyWriter.WriteEndElement();

            MyWriter.WriteStartElement("lastmod");
            MyWriter.WriteValue(lastmod);
            MyWriter.WriteEndElement();

            MyWriter.WriteEndElement();
        }

        public void WriteTagIndex(string Priority, string freq, string Navigation, DateTime lastmod, XmlWriter MyWriter)
        {
            throw new NotImplementedException();
        }

        public void AppendTag(string url, string Priority, string freq, string Navigation, DateTime lastmod)
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

        public void AppendTag(string url, string Navigation, DateTime lastmod)
        {
            throw new NotImplementedException();
        }
    }


}