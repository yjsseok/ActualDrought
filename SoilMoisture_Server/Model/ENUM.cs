using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoilMoisture_Server
{
    public enum DATA_TYPE
    {
        /// <summary>
        /// 수위
        /// </summary>
        WL,
        /// <summary>
        /// 유속
        /// </summary>
        SPEED,
        /// <summary>
        /// 증발
        /// </summary>
        EVAPORATION,
        /// <summary>
        /// 침투
        /// </summary>
        INFIL,
        /// <summary>
        /// 없음
        /// </summary>
        NONE
    }
}
