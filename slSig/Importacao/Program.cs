using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Importacao
{
    class Program
    {
        static void Main(string[] args)
        {
            var adpPlanoCliente = new DAL.DsPlanoTableAdapters.PlanoClienteTableAdapter();
            var adpPlano = new DAL.DsPlanoTableAdapters.PlanoTableAdapter();
            var adp = new DAL.DsImportacaoTableAdapters.ImportacaoTableAdapter();


            DAL.DsPlano.PlanoDataTable dtPlano = null;
            DAL.DsPlano.PlanoClienteDataTable dtPlanoCliente = null;

            var trn = ConnectionFramework.SqlAdapterHelper.BeginTransaction(adp);


            ConnectionFramework.SqlAdapterHelper.SetTransaction(adp, trn);
            ConnectionFramework.SqlAdapterHelper.SetTransaction(adpPlano, trn);
            ConnectionFramework.SqlAdapterHelper.SetTransaction(adpPlanoCliente, trn);


            try
            {

                GerenciadorDiretorio.SelecionarArquivosTxt(@"C:\inetpub\wwwroot\projetos_git\sig\slSig\Importacao\arquivos-importar\");

                foreach (var item in GerenciadorDiretorio.Arquivos)
                {
                    var lines = File.ReadAllLines(item.FullName);

                    for (var i = 0; i < lines.Length; i += 1)
                    {

                        Console.WriteLine(lines[i]);

                        var dados = lines[i].Split(';');
                        var titular = new CartaoDadosTitular(dados);
                        var idplano = 0;
                        var idplanocliente = 0;
                        var idImportacao = 0;

                        dtPlano = adpPlano.Listar(titular.Plano);

                        dtPlanoCliente = adpPlanoCliente.SelecionarPorNome(titular.PlanoCliente);

                        idplano = !dtPlano.Any() ? Convert.ToInt32(adpPlano.Inserir(titular.Plano)) : dtPlano[0].idplano;
                        idplanocliente = !dtPlanoCliente.Any() ? Convert.ToInt32(adpPlanoCliente.Inserir(titular.PlanoCliente)) : dtPlanoCliente[0].idplanocliente;


                        idImportacao = Convert.ToInt32(adp.ImportacaoInserir(1, titular.Titular, titular.NumeroContrato, idplano, idplanocliente, titular.Quantidade));

                        foreach (var dep in titular.Dependentes)
                        {
                            if (!string.IsNullOrWhiteSpace(dep.Nome))
                            {
                                adp.ImportacaoDetInserir(idImportacao, dep.Nome);
                            }
                        }
                    }
                }

                trn.Commit();

                Console.WriteLine("Importacao realizada com sucesso!!");

            }
            catch (Exception ex)
            {
                if (trn != null)
                {
                    trn.Rollback();
                }
            }


            Console.ReadKey();
        }
    }
}
