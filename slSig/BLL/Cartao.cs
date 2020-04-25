using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public static class Cartao
    {
        public static DAL.DsCartao.CartaoDadostitularDataTable SelecionarTitularPorContrato(string numeroContrato)
        {

            Validador.Validar(!string.IsNullOrWhiteSpace(numeroContrato), "Informe o numero do contrato.");

            using (var adp = new DAL.DsCartaoTableAdapters.CartaoDadostitularTableAdapter())
            {
                return adp.Listar(numeroContrato);
            }

        }


        public static DAL.DsCartao.CartaoDadosDependenteDataTable ListarDependentes(int idImportacaoDados)
        {

            Validador.Validar(idImportacaoDados > 0, "Não foi possivel recuperar os dados dos dependentes");

            using (var adp = new DAL.DsCartaoTableAdapters.CartaoDadosDependenteTableAdapter())
            {
                return adp.Listar(idImportacaoDados);
            }

        }
    }
}
