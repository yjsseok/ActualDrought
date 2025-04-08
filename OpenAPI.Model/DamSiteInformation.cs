using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Model
{
    public class DamSiteInformation
    {
        /// <summary>
        /// 댐코드
        /// </summary>
        public string damcd { get; set; }

        /// <summary>
        /// 댐명
        /// </summary>
        public string damnm { get; set; }

        /// <summary>
        /// 권역코드
        /// </summary>
        public string bbsncd { get; set; }

         /// <summary>
        /// 표준유역코드
        /// </summary>
        public string sbsncd { get; set; }

        /// <summary>
        /// 대권역명
        /// </summary>
        public string bbsnnm { get; set; }

        /// <summary>
        /// 관할기관
        /// </summary>
        public string mggvnm { get; set; }

    }
}
