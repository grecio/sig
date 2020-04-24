using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public static class Importacao
    {
        public static void Inserir(global::System.Nullable<int> idimportacao, string nome, string contrato, global::System.Nullable<int> idplano, 
            global::System.Nullable<int> idplanocliente, global::System.Nullable<int> qtd)
        {
            using (var adp = new DAL.DsImportacaoDadosTableAdapters.ImportacaoDadosTableAdapter())
            {
                adp.Inserir(idimportacao, nome, contrato, idplano, idplanocliente,qtd);
            }
        }

        public static void InserirDependentes(global::System.Nullable<int> idimportacaodados, string dependentes)
        {
            using (var adp = new DAL.DsImportacaoDadosTableAdapters.ImportacaoDadosTableAdapter())
            {
                adp.DependentesInserir(idimportacaodados, dependentes);
            }
        }
    }
}
