using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importacao
{
    public class CartaoDadosTitular
    {
        public int Quantidade { get; private set; }
        public string NumeroContrato { get; private set; }
        public string PlanoCliente { get; private set; }
        public string Plano { get; set; }
        public string Titular { get; private set; }

        public List<CartaoDadosDependente> Dependentes { get;  private set; }

        public CartaoDadosTitular(string[] array)
        {
            Quantidade = Convert.ToInt32(array[0]);
            NumeroContrato = array[1].Trim();
            Plano = array[2].Trim();
            PlanoCliente = array[3].Trim();
            Titular = array[4].Trim();

            Dependentes = new List<CartaoDadosDependente>();

            for (int i = 0; i < array.Length; i++)
            {
                if (i > 4)
                {
                    Dependentes.Add(new CartaoDadosDependente() { Nome = array[i].Trim() });
                }
            }

        }
    }
}
