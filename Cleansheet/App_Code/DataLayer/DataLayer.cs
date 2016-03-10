using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Xml.Linq;

using MySql.Data.MySqlClient;  

/// <summary>
/// Summary description for DataLayer
/// </summary>
/// 
namespace CleansheetAPI.DataLayer
{
    public class BaseDataLayer
    {
        public static string connectionString;

        public BaseDataLayer()
        {
            connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["WCFonnectionString"].ConnectionString;
        }

        public static MySqlParameter CreateParameters(DbType dbType, object value, string name, ParameterDirection pDirection)
        {
            MySqlParameter mySqlParam = new MySqlParameter();
            mySqlParam.DbType = dbType;
            mySqlParam.Value = value;
            mySqlParam.ParameterName = name;
            mySqlParam.Direction = pDirection;  
            return mySqlParam;
        }

        public enum EventFlag
        {
            Insert = 0,
            Update = 1
        }
    }
}
