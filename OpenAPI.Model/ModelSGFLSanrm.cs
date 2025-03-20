using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Model
{
    public class ModelSGFLSanrm
    {
        public DateTime modelDate { get; set; }
        public string sgCode { get; set; }
        public double precipitation { get; set; }
        public double evaporation { get; set; }
        public double soilMoisture { get; set; }
    }
}
