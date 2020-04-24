using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public static class DALBase
    {
        public static string DBConnectionString => ConfigurationManager.ConnectionStrings["DAL.Properties.Settings.natalprinterConnectionString"].ToString();
    }
}
