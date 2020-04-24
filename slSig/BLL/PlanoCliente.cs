using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public static class PlanoCliente
    {
        public static DAL.DsPlanoCliente.PlanoClienteDataTable SelecionarPorNome(string plano)
        {

            using (var adp = new DAL.DsPlanoClienteTableAdapters.PlanoClienteTableAdapter())
            {
                return adp.Selecionar(plano);
            }

        }

        public static void Inserir(string planocliente)
        {
            using (var adp = new DAL.DsPlanoClienteTableAdapters.PlanoClienteTableAdapter())
            {
                adp.Inserir(planocliente);

            }
        }
    }
}
