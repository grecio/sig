using DAL;

namespace ConnectionFramework
{
    public abstract class ConnectionBase
    {
        private System.Data.SqlClient.SqlConnection _cnn;

        public System.Data.SqlClient.SqlConnection DBConnection
        {
            get
            {
                if (_cnn == null)
                    _cnn = new System.Data.SqlClient.SqlConnection(DAL.DALBase.DBConnectionString);

                return _cnn;
            }
        }
    }
}
