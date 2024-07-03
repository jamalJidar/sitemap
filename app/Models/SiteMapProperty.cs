using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace app.Models
{
    public enum ChangeFreq
	{
		None = 0,
		Always,
		Hourly,
		Daily,
		Weekly,
		Monthly,
		Yearly

	}
    public class SiteMapProperty
    {
        public double[] Priority = { 1.0, 0.9, 0.8, 0.7, 0.6, 0.5, 0.4, 0.3, 0.2, 0.1, 0.0 };
		public ChangeFreq changeFreq { get; set; }
		public string Navigation { get; set; }
		public DateTime lastmod { get; set; }
		public XmlWriter Writer { get; set; }
    }
}