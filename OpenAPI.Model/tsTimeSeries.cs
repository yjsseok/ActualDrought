using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Model
{
    public class tsTimeSeries
    {
        public string tm { get; set; }
        public DateTime tmdt { get; set; }
        //public int DayOfYear { get; set; }
        public double rainfall { get; set; }
    }
}
