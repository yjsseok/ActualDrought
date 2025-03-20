using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoilMoisture_Server
{
    public class ReceiveData
    {
        public string deviceid { get; set; }
        public string measureDT { get; set; }
        public int WC10 { get; set; }
        public int WC20 { get; set; }
        public int WC30 { get; set; }
        public int WC40 { get; set; }
        public int WC50 { get; set; }
    }
}
