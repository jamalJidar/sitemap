using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace app.Models
{
    public enum ChangeFreq
    {
        [Display(Name = "none")]
        None = 0,
        [Display(Name = "always")]
        Always,
        [Display(Name = "hourly")]
        Hourly,
        [Display(Name = "daily")]
        Daily,
        [Display(Name = "weekly")]
        Weekly,
        [Display(Name = "monthly")]
        Monthly,
        [Display(Name = "yearly")]
        Yearly

    }


    public enum SiteMapType
    {
        [Display(Name = "post")]
        Post,
        [Display(Name = "image")]
        Image,
        [Display(Name = "tag")]
        Tag,
        [Display(Name = "catgory")]
        Catgory,
        [Display(Name = "sitemapIndex")]
        SiteMapIndex
    }

    public class SiteMapProperty
    {


        public ChangeFreq Freq { get; set; }
        public string _ferq
        {
            get
            {
                return Utility.EnumExtensions.GetDisplayName(Freq);
            }
        }
        public string Navigation { get; set; }
        public DateTime lastmod { get; set; }
        public XmlWriter Writer { get; set; }
        public int Priority { get; set; }
        public string _priority
        {
            get
            {
                return (new double[] { 1.0, 0.9, 0.8, 0.7, 0.6, 0.5, 0.4, 0.3, 0.2, 0.1, 0.0 }[Priority]).ToString();
            } 
        }
    }


}