/////////////////////////////////////////////////////////////////////////////////////
/// ◑ Solution 		: UFRI
/// ◑ Project			: UFRI.FrameWork
/// ◑ Class Name		: DBUtil
/// ◑ Description		: DB Connection Manager
/// 
/// ◑ Revision History
/////////////////////////////////////////////////////////////////////////////////////
/// Date			Author		    Description
/////////////////////////////////////////////////////////////////////////////////////
/// 2017/12/28      GiMoon     First Draft
/////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFRI.FrameWork
{
    public class DBUtil
    {
        /// <summary>
        /// SQL Parameter생성
        /// </summary>
        /// <param name="paramName">Param이름</param>
        /// <param name="type">SQL DB Type</param>
        /// <param name="size">사이즈</param>
        /// <param name="paramValue">값</param>
        /// <returns></returns>
        public static SqlParameter CreateInParam(string paramName, SqlDbType type, int size, object paramValue)
        {
            try
            {
                SqlParameter param = new SqlParameter(paramName, type, size);
                param.Value = paramValue;
                return param;
            }
            catch (Exception)
            {
                throw new Exception("SQL Parameter 생성에 오류가 있습니다.");
            }
        }

        public static SqlParameter CreateOutParam(string paramName, SqlDbType type, int size)
        {
            try
            {
                SqlParameter param = new SqlParameter(paramName, type, size);
                param.Direction = ParameterDirection.Output;
                return param;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static SqlParameter CreateReturnParam()
        {
            try
            {
                SqlParameter param = new SqlParameter("@RETURN_VALUE", SqlDbType.Int, 4);
                param.Direction = ParameterDirection.ReturnValue;
                return param;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public SqlCommand CreateCommand(SqlConnection con, string command, CommandType type)
        {
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(command, con);
                cmd.CommandType = type;
                cmd.CommandTimeout = 15;

                return cmd;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public SqlCommand CreateCommand(SqlConnection con, string command, CommandType type, SqlParameter[] parameters)
        {
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(command, con);
                cmd.CommandType = type;
                cmd.CommandTimeout = 15;

                if (parameters != null)
                {
                    foreach (SqlParameter p in parameters)
                    {
                        cmd.Parameters.Add(p);
                    }
                }
                return cmd;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
