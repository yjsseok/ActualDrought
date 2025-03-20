using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Model
{
    public class ModelSGFLS
    {
        public DateTime modelDate { get; set; }
        public string sgCode { get; set; }
        public double STVI { get; set; }
        public double EDDI_SPI { get; set; }
        public int FlashDroughtMonitor { get; set; }

    }
}
