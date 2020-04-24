using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importacao
{
    public class GerenciadorDiretorio
    {
        public static List<FileInfo> Arquivos = new List<FileInfo>();

        public GerenciadorDiretorio()
        {
         
        }
 
        public static void SelecionarArquivosTxt(string sDir)
        {

            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d))
                    {

                        if (Path.GetExtension(f).ToLowerInvariant() == ".txt")
                        {
                            Arquivos.Add(new FileInfo(f));
                        }             
                    }

                    SelecionarArquivosTxt(d);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }
    }
}
