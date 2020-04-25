using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public static class PlanoCliente
    {
        public static DAL.DsPlano.PlanoClienteDataTable SelecionarPorNome(string plano)
        {

            using (var adp = new DAL.DsPlanoTableAdapters.PlanoClienteTableAdapter())
            {
                return adp.SelecionarPorNome(plano);
            }

        }

        public static void Inserir(string planocliente)
        {
            using (var adp = new DAL.DsPlanoTableAdapters.PlanoClienteTableAdapter())
            {
                adp.Inserir(planocliente);

            }
        }
    }
}
