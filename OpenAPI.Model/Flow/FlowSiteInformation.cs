using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Model
{
    public class FlowSiteInformation
    {
        public string bbsnnm { get; set; }
        public string obscd { get; set; }
        public string obsnm { get; set; }
        public string sbsncd { get; set; }
        public string mngorg { get; set; }
        public int minYear { get; set; }
        public int maxYear { get; set; }
    }
}
