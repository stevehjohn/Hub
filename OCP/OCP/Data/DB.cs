using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace OCP.Data
{
    public class DB : IDisposable
    {
        private SqlConnection m_Cnn = null;
        private SqlCommand m_Cmd = null;
        private SqlTransaction m_Trn = null;

        public DB()
        {
            string sCnn;
            // This try/catch allows testing on standard IIS rather than Azure emulator.
            //try
            //{
            //    sCnn = RoleEnvironment.GetConfigurationSettingValue("OCPDB");
            //}
            //catch
            //{
                sCnn = ConfigurationManager.ConnectionStrings["OCPDB"].ConnectionString;
            //}

            m_Cnn = new SqlConnection(sCnn);
            m_Cnn.Open();
        }

        public void CreateCommand(string procName)
        {
            m_Cmd = new SqlCommand(procName, m_Cnn);
            m_Cmd.CommandType = CommandType.StoredProcedure;
            if (m_Trn != null)
                m_Cmd.Transaction = m_Trn;
        }

        public void AddParameter(string paramName, SqlDbType paramType, object paramValue)
        {
            SqlParameter prm = new SqlParameter(paramName, paramType);
            prm.Direction = ParameterDirection.Input;
            prm.Value = paramValue;
            m_Cmd.Parameters.Add(prm);
        }

        public void AlterParameterValue(string paramName, object paramValue)
        {
            m_Cmd.Parameters[paramName].Value = paramValue;
        }

        public void StartTransaction()
        {
            m_Trn = m_Cnn.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (m_Trn != null)
            {
                m_Trn.Commit();
                m_Trn = null;
            }
        }

        public void RollbackTransaction()
        {
            if (m_Trn != null)
            {
                m_Trn.Rollback();
                m_Trn = null;
            }
        }

        public SqlDataReader Execute()
        {
            SqlDataReader rdr = m_Cmd.ExecuteReader();

            return rdr;
        }

        public void ExecuteNonReader()
        {
            m_Cmd.ExecuteNonQuery();
        }

        public void Close()
        {
            if (m_Cnn != null)
            {
                if (m_Cnn.State != ConnectionState.Closed)
                {
                    if (m_Trn != null)
                    {
                        m_Trn.Rollback();
                    }

                    m_Cnn.Close();
                }
                m_Cnn = null;
            }
        }

        public static object IsNull(object val)
        {
            if (val == DBNull.Value)
                return null;
            else
                return val;
        }

        public static object IsNull(object val, object defaultVal)
        {
            if (val == DBNull.Value)
                return defaultVal;
            else
                return val;
        }

        public void Dispose()
        {
            Close();
        }
    }
}
