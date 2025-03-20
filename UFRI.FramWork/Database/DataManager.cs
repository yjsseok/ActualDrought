/////////////////////////////////////////////////////////////////////////////////////
/// ◑ Solution 		: UFRI
/// ◑ Project			: UFRI.FrameWork
/// ◑ Class Name		: DataManager
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
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UFRI.FrameWork
{
    public class DataManager : IDisposable
    {
        private SqlConnection _con;
        private string _connectKey;

        /// <summary>
        /// DB 접속 키
        /// </summary>
        public string ConnectKey
        {
            get { return _connectKey; }
            set { _connectKey = value; }
        }

        /// <summary>
        /// DBManager의 기본생성자1
        /// </summary>
        public DataManager()
        {
        }

        /// <summary>
        /// DBManager의 기본 생성자2
        /// </summary>
        /// <param name="connectKey">DB 접속키</param>
        public DataManager(string connectKey)
        {
            ConnectKey = connectKey;
            if (_con == null) CreateConnection();
        }

        public void CreateConnection()
        {
            if (true == string.IsNullOrEmpty(ConnectKey))
            {
                throw new Exception("DB Connection에 필요한 키값이 없습니다.");
            }
            _con = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectKey].ConnectionString);
        }

        public SqlDataReader ExecuteReader(string command, CommandType type)
        {
            DBUtil db = new DBUtil();
            SqlCommand cmd = db.CreateCommand(_con, command, type);
            SqlDataReader dr = null;

            try
            {
                dr = cmd.ExecuteReader();
                return dr;
            }
            catch (Exception ex)
            {
                if (dr != null)
                {
                    dr.Close();
                    dr = null;
                }
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }

        public SqlDataReader ExecuteReader(string command, CommandType type, SqlParameter[] parameters)
        {
            DBUtil db = new DBUtil();
            SqlCommand cmd = db.CreateCommand(_con, command, type, parameters);
            SqlDataReader dr = null;
            try
            {
                dr = cmd.ExecuteReader();
                return dr;
            }
            catch (Exception ex)
            {
                if (dr != null)
                {
                    dr.Close();
                    dr = null;
                }
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }

        public DataTable ExecuteDataTable(string command, CommandType type)
        {
            DBUtil db = new DBUtil();
            SqlCommand cmd = db.CreateCommand(_con, command, type);
            SqlDataAdapter adp = new SqlDataAdapter(cmd);

            try
            {
                DataSet ds = new DataSet();
                adp.Fill(ds);

                if (ds.Tables.Count > 0)
                    return ds.Tables[0];
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                adp.Dispose();
                cmd.Dispose();
            }
        }

        public DataTable ExecuteDataTable(string command, CommandType type, SqlParameter[] parameters)
        {
            DBUtil db = new DBUtil();
            SqlCommand cmd = db.CreateCommand(_con, command, type, parameters);
            SqlDataAdapter adp = new SqlDataAdapter(cmd);

            try
            {
                DataSet ds = new DataSet();
                adp.Fill(ds);

                if (ds.Tables.Count > 0)
                    return ds.Tables[0];
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                adp.Dispose();
                cmd.Dispose();
            }
        }

        public XmlReader ExecuteXmlReader(string command, CommandType type)
        {
            DBUtil db = new DBUtil();
            SqlCommand cmd = db.CreateCommand(_con, command, type);
            XmlReader reader = null;

            try
            {
                reader = cmd.ExecuteXmlReader();
                return reader;
            }
            catch (Exception ex)
            {
                if (reader != null)
                    reader.Close();
                reader = null;
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }


        public XmlReader ExecuteXmlReader(string command, CommandType type, SqlParameter[] parameters)
        {
            DBUtil db = new DBUtil();
            SqlCommand cmd = db.CreateCommand(_con, command, type, parameters);
            XmlReader reader = null;

            try
            {
                reader = cmd.ExecuteXmlReader();
                return reader;
            }
            catch (Exception ex)
            {
                if (reader != null)
                    reader.Close();
                reader = null;
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }

        public object ExecuteScalar(string command, CommandType type)
        {
            DBUtil db = new DBUtil();
            SqlCommand cmd = db.CreateCommand(_con, command, type);
            try
            {
                return cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }

        public object ExecuteScalar(string command, CommandType type, SqlParameter[] parameters)
        {
            DBUtil db = new DBUtil();
            SqlCommand cmd = db.CreateCommand(_con, command, type, parameters);
            try
            {
                return cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }

        public int ExecuteNonQuery(string command, CommandType type)
        {
            DBUtil db = new DBUtil();
            SqlCommand cmd = db.CreateCommand(_con, command, type);
            try
            {
                int result = cmd.ExecuteNonQuery();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }

        public int ExecuteNonQuery(string command, CommandType type, SqlParameter[] parameters)
        {
            DBUtil db = new DBUtil();
            SqlCommand cmd = db.CreateCommand(_con, command, type, parameters);

            try
            {
                int result = cmd.ExecuteNonQuery();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }

        //public void Disconnection()
        //{
        //    if (con.State == ConnectionState.Open)
        //    {
        //        con.Close();
        //        con.Dispose();
        //        con = null;
        //    }
        //}

        #region IDisposable 멤버
        /// <summary>
        /// Dispose구현
        /// </summary>
        public void Dispose()
        {
            if (_con.State == ConnectionState.Open)
            {
                _con.Close();
                _con.Dispose();
                _con = null;
            }
        }

        #endregion
    }
}
