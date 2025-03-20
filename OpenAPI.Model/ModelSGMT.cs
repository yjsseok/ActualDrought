using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Model
{
    public class ModelSGMT
    {
        public DateTime modelDate { get; set; }
        public string sgCode { get; set; }

        public double spi_7 { get; set; }
        public double spi_30 { get; set; }
        public double spi_90 { get; set; }
        public double spi_180 { get; set; }
        public double spi_270 { get; set; }
        public double spi_365 { get; set; }
        public int spi_7_dr { get; set; }
        public int spi_30_dr { get; set; }
        public int spi_90_dr { get; set; }
        public int spi_180_dr { get; set; }
        public int spi_270_dr { get; set; }
        public int spi_365_dr { get; set; }

    }
}
