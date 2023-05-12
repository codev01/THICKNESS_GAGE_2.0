using System;
using System.Data;

using Oracle.ManagedDataAccess.Client;

using ThicknessGage2_0.Contracts;
using ThicknessGage2_0.Properties;

namespace ThicknessGage2_0.Client
{
    public class DataBase : IDBContract
    {
        #region Congiguration FIELDS

#if DEBUG
        private const string DataSource = "ANGC_PROD_GTW";
        private const string User = "CGL";
        private const string Password = "cgl";
        private const string Package = "pcg_gauge_test";
#else
        private string DataSource => Configuration.OracleDataSource;
        private string User => Configuration.OracleUser;
        private string Password => Configuration.OraclePassword; 
        private string Package => Configuration.OraclePackage;
#endif

#if Redirector
        private bool isWrite = false;
#else
        private bool isWrite => Configuration.OracleWriteToDataBase;
#endif

        #endregion

        public bool IsWrite => isWrite;

        public OracleConnection OracleConnection { get; private set; }

        public event Action<bool> OnWriting;

        private string ConnectionString => $"Data Source={DataSource};User ID={User};Password={Password};pooling=false";

        public void Connect()
        {
            OracleConnection = new OracleConnection(ConnectionString);
            try
            {
                OracleConnection.Open();
            }
            catch (OracleException e)
            {
                throw new Exception("Ошибка подключения к БД", e);
            }
            catch (InvalidOperationException e)
            {
                throw new Exception("Oracle: Соединение не открыто", e);
            }
            finally
            {
                OnWriting?.Invoke(false);
            }
        }

        public void SaveTo(string value, DateTime dateTime)
        {
            if (IsWrite)
            {
                try
                {
                    OracleCommand cmd = new OracleCommand($"{User}.{Package}.ins_tel", OracleConnection);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("v_tel", OracleDbType.Varchar2).Value = value;
                    cmd.Parameters.Add("v_dt", OracleDbType.Date).Value = dateTime;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw new Exception("Ошибка при вызове процедуры ins_tel", e);
                }
                finally
                {
                    OnWriting?.Invoke(false);
                }
            }

            OnWriting?.Invoke(IsWrite);
        }

        public string GetCoilIdZone()
        {
            try
            {
                OracleCommand cmd = new OracleCommand($"{User}.{Package}.Get_COILID_Zone", OracleConnection);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("Result", OracleDbType.Varchar2, ParameterDirection.ReturnValue).Size = 10;
                cmd.ExecuteNonQuery();
                return cmd.Parameters["Result"].Value.ToString();
            }
            catch (Exception e)
            {
                throw new Exception("Ошибка при вызове функции Get_COILID_Zone"
                    + $"\n\t{e.Message}", e);
            }
        }
    }
}
