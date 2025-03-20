using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Model
{
    public class rcvKMAASOSData
    {
        /// <summary>
        /// 관측일(KST) YYYMMDD
        /// </summary>
        public string TM { get; set; }

        /// <summary>
        /// 국내 지점번호
        /// </summary>
        public int STN { get; set; }

        /// <summary>
        /// 일 평균 풍속 (m/s)
        /// </summary>
        public double WS_AVG { get; set; }

        /// <summary>
        /// 일 풍정 (m)
        /// </summary>
        public double WR_DAY { get; set; }

        /// <summary>
        /// 최대풍향
        /// </summary>
        public double WD_MAX { get; set; }

        /// <summary>
        /// 최대풍속 (m/s)
        /// </summary>
        public double WS_MAX { get; set; }

        /// <summary>
        /// 최대풍속 시각 (시분)
        /// </summary>
        public double WS_MAX_TM { get; set; }

        /// <summary>
        /// 최대순간풍향
        /// </summary>
        public double WD_INS { get; set; }

        /// <summary>
        /// 최대순간풍속 (m/s)
        /// </summary>
        public double WS_INS { get; set; }

        /// <summary>
        /// 최대순간풍속 시각 (시분)
        /// </summary>
        public double WS_INS_TM { get; set; }

        /// <summary>
        /// 일 평균기온 (C)
        /// </summary>
        public double TA_AVG { get; set; }

        /// <summary>
        /// 최고기온 (C)
        /// </summary>
        public double TA_MAX { get; set; }

        /// <summary>
        /// 최고기온 시가 (시분)
        /// </summary>
        public double TA_MAX_TM { get; set; }

        /// <summary>
        /// 최저기온 (C)
        /// </summary>
        public double TA_MIN { get; set; }

        /// <summary>
        /// 최저기온 시각 (시분)
        /// </summary>
        public double TA_MIN_TM { get; set; }

        /// <summary>
        /// 일 평균 이슬점온도 (C)
        /// </summary>
        public double TD_AVG { get; set; }

        /// <summary>
        /// 일 평균 지면온도 (C)
        /// </summary>
        public double TS_AVG { get; set; }

        /// <summary>
        /// 일 최저 초상온도 (C)
        /// </summary>
        public double TG_MIN { get; set; }

        /// <summary>
        /// 일 평균 상대습도 (%)
        /// </summary>
        public double HM_AVG { get; set; }

        /// <summary>
        /// 최저습도 (%)
        /// </summary>
        public double HM_MIN { get; set; }

        /// <summary>
        /// 최저습도 시각 (시분)
        /// </summary>
        public double HM_MIN_TM { get; set; }

        /// <summary>
        /// 일 평균 수증기압 (hPa)
        /// </summary>
        public double PV_AVG { get; set; }

        /// <summary>
        /// 소형 증발량 (mm)
        /// </summary>
        public double EV_S { get; set; }

        /// <summary>
        /// 대형 증발량 (mm)
        /// </summary>
        public double EV_L { get; set; }

        /// <summary>
        /// 안개계속시간 (hr)
        /// </summary>
        public double FG_DUR { get; set; }

        /// <summary>
        /// 일 평균 현지기압 (hPa)
        /// </summary>
        public double PA_AVG { get; set; }

        /// <summary>
        /// 일 평균 해면기압 (hPa)
        /// </summary>
        public double PS_AVG { get; set; }

        /// <summary>
        /// 최고 해면기압 (hPa)
        /// </summary>
        public double PS_MAX { get; set; }

        /// <summary>
        /// 최고 해면기압 시각 (시분)
        /// </summary>
        public double PS_MAX_TM { get; set; }

        /// <summary>
        /// 최저 해면기압 (hPa)
        /// </summary>
        public double PS_MIN { get; set; }

        /// <summary>
        /// 최저 해면기압 시각 (시분)
        /// </summary>
        public double PS_MIN_TM { get; set; }

        /// <summary>
        /// 일 평균 전운량 (1/10)
        /// </summary>
        public double CA_TOT { get; set; }

        /// <summary>
        /// 일조합 (hr)
        /// </summary>
        public double SS_DAY { get; set; }

        /// <summary>
        /// 가조시간 (hr)
        /// </summary>
        public double SS_DUR { get; set; }

        /// <summary>
        /// 캄벨 일조 (hr)
        /// </summary>
        public double SS_CMB { get; set; }

        /// <summary>
        /// 일사합 (MJ/m2)
        /// </summary>
        public double SI_DAY { get; set; }

        /// <summary>
        /// 최대 1시간일사 (MJ/m2)
        /// </summary>
        public double SI_60M_MAX { get; set; }

        /// <summary>
        /// 최대 1시간일사 시각 (시분)
        /// </summary>
        public double SI_60M_MAX_TM { get; set; }

        /// <summary>
        /// 일 강수량 (mm)
        /// </summary>
        public double RN_DAY { get; set; }

        /// <summary>
        /// 9-9 강수량 (mm)
        /// </summary>
        public double RN_D99 { get; set; }

        /// <summary>
        /// 강수계속시간 (hr)
        /// </summary>
        public double RN_DUR { get; set; }

        /// <summary>
        /// 1시간 최다강수량 (mm)
        /// </summary>
        public double RN_60M_MAX { get; set; }

        /// <summary>
        /// 1시간 최다강수량 시각 (시분)
        /// </summary>
        public double RN_60M_MAX_TM { get; set; }

        /// <summary>
        /// 10분간 최다강수량 (mm)
        /// </summary>
        public double RN_10M_MAX { get; set; }

        /// <summary>
        /// 10분간 최다강수량 시각 (시분)
        /// </summary>
        public double RN_10M_MAX_TM { get; set; }

        /// <summary>
        /// 최대 강우강도 (mm/h)
        /// </summary>
        public double RN_POW_MAX { get; set; }

        /// <summary>
        /// 최대 강우강도 시각 (시분)
        /// </summary>
        public double RN_POW_MAX_TM { get; set; }

        /// <summary>
        /// 최심 신적설 (cm)
        /// </summary>
        public double SD_NEW { get; set; }

        /// <summary>
        /// 최심 신적설 시각 (시분)
        /// </summary>
        public double SD_NEW_TM { get; set; }

        /// <summary>
        /// 최심 적설 (cm)
        /// </summary>
        public double SD_MAX { get; set; }

        /// <summary>
        /// 최심 적설 시각 (시분)
        /// </summary>
        public double SD_MAX_TM { get; set; }

        /// <summary>
        /// 0.5m 지중온도 (C) 
        /// </summary>
        public double TE_05 { get; set; }

        /// <summary>
        /// 1.0m 지중온도 (C)
        /// </summary>
        public double TE_10 { get; set; }

        /// <summary>
        /// 1.5m 지중온도 (C)
        /// </summary>
        public double TE_15 { get; set; }

        /// <summary>
        /// 3.0m 지중온도 (C)
        /// </summary>
        public double TE_30 { get; set; }

        /// <summary>
        /// 5.0m 지중온도 (C)
        /// </summary>
        public double TE_50 { get; set; }

    }
}
