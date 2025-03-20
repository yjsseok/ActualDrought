using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Model
{
    public class PointRainfall
    {
        public string stn { get; set; }
        public double ratio { get; set; }
        public List<tsTimeSeries> listRainfall { get; set; }

        public PointRainfall() 
        {
            this.listRainfall = new List<tsTimeSeries>();
        }
    }
}
