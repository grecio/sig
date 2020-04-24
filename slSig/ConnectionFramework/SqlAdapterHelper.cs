using System;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;

namespace ConnectionFramework
{
    public class SqlAdapterHelper
    {
        public static SqlTransaction BeginTransaction(object tableAdapter)
        {
            return BeginTransaction(tableAdapter, IsolationLevel.ReadUncommitted);
        }

        public static SqlTransaction BeginTransaction(object tableAdapter, IsolationLevel isolationLevel)
        {
            Type type = tableAdapter.GetType();
            SqlConnection connection = GetConnection(tableAdapter);

            if (connection.State == ConnectionState.Closed)
                connection.Open();

            SqlTransaction transaction = connection.BeginTransaction(isolationLevel);

            SetTransaction(tableAdapter, transaction);

            return transaction;
        }

        private static SqlConnection GetConnection(object tableAdapter)
        {
            Type type = tableAdapter.GetType();
            PropertyInfo connectionProperty = type.GetProperty("Connection", BindingFlags.NonPublic | BindingFlags.Instance);
            SqlConnection connection = (SqlConnection)connectionProperty.GetValue(tableAdapter, null);

            return connection;
        }

        private static void SetConnection(object tableAdapter, SqlConnection connection)
        {
            Type type = tableAdapter.GetType();
            PropertyInfo connectionProperty = type.GetProperty("Connection", BindingFlags.NonPublic | BindingFlags.Instance);

            connectionProperty.SetValue(tableAdapter, connection, null);
        }

        public static void SetTransaction(object tableAdapter, SqlTransaction transaction)
        {
            Type type = tableAdapter.GetType();
            PropertyInfo commandsProperty = type.GetProperty("CommandCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            SqlCommand[] commands = (SqlCommand[])commandsProperty.GetValue(tableAdapter, null);

            foreach (SqlCommand command in commands)
                command.Transaction = transaction;

            SetConnection(tableAdapter, transaction.Connection);
        }
    }

}
