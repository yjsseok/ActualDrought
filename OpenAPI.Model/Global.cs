using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Model
{
    public class Global
    {
        private static Global instance = null;

        static Global()
        {
            instance = new Global();
        }

        /// <summary>
        /// 데이터베이스 접속객체
        /// </summary>
        public NpgsqlConnection NpgSQLconn { get; set; }

        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public bool RealTimeUse { get; set; }
        public bool PeriodUse { get; set; }

        public List<FlowSiteInformation> listFlowOBS { get; set; }
        public List<DamSiteInformation> listDams { get; set; }
        private Global()
        {
            this.RealTimeUse = true;
            this.PeriodUse = false;

            this.listFlowOBS = new List<FlowSiteInformation>();
            this.listDams = new List<DamSiteInformation>();
        }

        public static Global GetInstance()
        {
            return instance;
        }
    }
}
