using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public static class Briefing
    {

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static DAL.DsBriefing.BriefingDataTable Listar(int IdBriefing, int IdCliente)
        {

            using (var adp = new DAL.DsBriefingTableAdapters.BriefingTableAdapter())
            {
                return adp.Listar(IdBriefing, IdCliente);
            }

        }

        public static void Salvar(global::System.Nullable<int> idcliente, string titulo, string descricao, global::System.Nullable<int> idbriefing)
        {

            Validador.Validar(!string.IsNullOrWhiteSpace(titulo), "Informe o titulo");
            Validador.Validar(!string.IsNullOrWhiteSpace(descricao), "Informe a descricao");

            using (var adp = new DAL.DsBriefingTableAdapters.BriefingTableAdapter())
            {
                if (idbriefing == 0)
                {
                    adp.Inserir(idcliente, titulo, descricao);
                }
                else
                {
                    adp.Atualizar(idcliente, titulo, descricao, idbriefing);
                }
                
            }

        }

        public static void Excluir(global::System.Nullable<int> idbriefing)
        {
            using (var adp = new DAL.DsBriefingTableAdapters.BriefingTableAdapter())
            {
                adp.Excluir(idbriefing);
            }
        }

    }
}
