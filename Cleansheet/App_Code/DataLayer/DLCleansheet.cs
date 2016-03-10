using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using CleansheetAPI.BusinessLayer;

namespace CleansheetAPI.DataLayer
{
    public class DLCleansheet : BaseDataLayer
    {
        public DataSet GetCleanSheetData(BLCleansheet obj)
        {
            try
            {
                string Sql = null;
                Sql = "CALL SP_GET_CLEANSHEET_DATA(?VEHTYPEID)";
                MySqlParameter[] Params = new MySqlParameter[1];

                Params[0] = CreateParameters(DbType.Int64, obj._vehtypeid, "?VEHTYPEID", ParameterDirection.Input);

                return (DataSet)(MySqlHelper.ExecuteDataset(connectionString, Sql, Params));
            }
            finally
            {
                // Params = null;
            }
        }

        public DataTable GetVehicleTypeByName(string VEHICLETYPENAME)
        {
            string queryString = "CALL SP_GetVehicleTypeByName(?VEHICLETYPENAME)";
            MySqlParameter[] mySqlParam = new MySqlParameter[1];

            mySqlParam[0] = CreateParameters(DbType.String, VEHICLETYPENAME, "?VEHICLETYPENAME", ParameterDirection.Input);

            return (DataTable)MySqlHelper.ExecuteDataset(connectionString, queryString, mySqlParam).Tables[0];
        }

        public DataTable GetDistance(String _origin, String _destination)
        {
            try
            {
                string Sql = null;
                Sql = "CALL SP_GET_DISTANCE(?ORIGIN_CITY, ?DESTINATION_CITY)";
                MySqlParameter[] Params = new MySqlParameter[2];

                Params[0] = CreateParameters(DbType.String, _origin, "?ORIGIN_CITY", ParameterDirection.Input);
                Params[1] = CreateParameters(DbType.String, _destination, "?DESTINATION_CITY", ParameterDirection.Input);

                return (DataTable)(MySqlHelper.ExecuteDataset(connectionString, Sql, Params)).Tables[0];
            }
            finally
            {
                // Params = null;
            }
        }
    }
}
