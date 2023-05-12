using System;
using System.Data;

using Oracle.ManagedDataAccess.Client;

namespace ThicknessGage
{
    public class DataBase
    {
        public OracleConnection OracleConnection { get; private set; }

#if DEBUG
        private const string DataSource = "ANGC_PROD_GTW";
        private const string User = "CGL";
        private const string Password = "cgl";
        private const string Package = "pcg_gauge_test";
#else
        private string DataSource { get => Configuration.OracleDataSource; }
        private string User { get => Configuration.OracleUser; }
        private string Password { get => Configuration.OraclePassword; }
        private string Package { get => Configuration.OraclePackage; }
#endif
        private string ConnectionString
        {
            get => $"Data Source={DataSource};User ID={User};Password={Password};pooling=false";
        }

        public bool Connect()
        {
            OracleConnection = new OracleConnection(ConnectionString);
            try
            {
                OracleConnection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public ConnectionState GetConnectionState() => OracleConnection.State;

        public bool GetConnected()
        {
            if (OracleConnection.State == ConnectionState.Open)
                return true;
            else
                return false;
        }

        public void SaveTo(string value, DateTime dateTime)
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
