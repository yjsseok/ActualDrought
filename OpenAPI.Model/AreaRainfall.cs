using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Model
{
    public class AreaRainfall
    {
        public string sggCode { get; set; }
        public List<tsTimeSeries> listAreaRainfall { get; set; }
        public List<PointRainfall> CollectionPointRainfall { get; set; }

        public AreaRainfall()
        {
            this.listAreaRainfall = new List<tsTimeSeries>();
            this.CollectionPointRainfall = new List<PointRainfall>();
        }
    }
}
