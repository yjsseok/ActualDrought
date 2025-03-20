using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Model
{
    public class SoilMoisture
    {
        public DateTime measureDT { get; set; }
        public string SiteCode { get; set; }
        public double wc10 { get; set; }
        public double wc20 { get; set; }
        public double wc30 { get; set; }
        public double wc40 { get; set; }
        public double wc50 { get; set; }
        public double bat { get; set; }

    }
}
