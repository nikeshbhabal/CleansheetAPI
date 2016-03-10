using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using CleansheetAPI.DataLayer;

namespace CleansheetAPI.BusinessLayer
{
    public class BLCleansheet: Params, IDisposable
    {
        DLCleansheet oDLCleansheet;

        public BLCleansheet()
        {
            oDLCleansheet = new DLCleansheet();
        }

        public DataSet GetCleanSheetData()
        {
            return oDLCleansheet.GetCleanSheetData(this);
        }

        public DataTable GetVehicleTypeByName(string VEHICLETYPENAME)
        {
            return oDLCleansheet.GetVehicleTypeByName(VEHICLETYPENAME);
        }

        public DataTable GetDistance(string _origin, string _destination)
        {
            return oDLCleansheet.GetDistance(_origin, _destination);
        }

        #region IDisposable Members

        public void Dispose()
        {
            oDLCleansheet = null;
        }

        #endregion
    }
}
