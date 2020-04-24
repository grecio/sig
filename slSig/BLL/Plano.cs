using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public static class Plano
    {
        public static DAL.DsPlano.PlanoDataTable Listar(string plano)
        {

            using (var adp = new DAL.DsPlanoTableAdapters.PlanoTableAdapter())
            {
                return adp.Listar(plano);
            }

        }

        public static void Inserir(string plano)
        {
            using (var adp = new DAL.DsPlanoTableAdapters.PlanoTableAdapter())
            {
                adp.Inserir(plano);
            }
        }
    }
}
