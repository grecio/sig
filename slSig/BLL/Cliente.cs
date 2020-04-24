using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BLL
{
    /// <summary>
    /// 
    /// </summary>
    public static class Cliente
    {

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static DAL.DsCliente.ClienteDataTable Listar()
        {

            using (var adp = new DAL.DsClienteTableAdapters.ClienteTableAdapter())
            {
                return adp.Listar();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static DAL.DsCliente.ClienteDataTable Selecionar(global::System.Nullable<int> ID)
        {

            using (var adp = new DAL.DsClienteTableAdapters.ClienteTableAdapter())
            {
                return adp.Selecionar(ID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nome"></param>
        /// <param name="Email"></param>
        /// <param name="CPF"></param>
        /// <param name="FixoDDD"></param>
        /// <param name="FixoNumero"></param>
        /// <param name="CelularDDD"></param>
        /// <param name="CelularNumero"></param>
        /// <param name="ID"></param>
        public static void Salvar(string cpfcnpj, string nome, string email, string telefone,  global::System.Nullable<int> IdCliente)
        {

            Validador.Validar(!string.IsNullOrWhiteSpace(nome), "Informe o nome.");
            Validador.Validar(!string.IsNullOrWhiteSpace(email), "Informe o e-mail.");

            using (var adp = new DAL.DsClienteTableAdapters.ClienteTableAdapter())
            {
                var dt = adp.SelecionarPorCnpj(cpfcnpj);

                Validador.Validar(dt.Count == 0, "Já existe um cliente cadatrado com o e-mail informado.");

                if (IdCliente == 0)
                {
                    adp.Inserir(cpfcnpj, nome, email, telefone);
                }
                else
                {
                    adp.Atualizar(cpfcnpj, nome, email, telefone, IdCliente);
                }
            }
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        public static void Excluir(int? ID)
        {

            using (var adp = new DAL.DsClienteTableAdapters.ClienteTableAdapter())
            {
                adp.Excluir(ID);
            }

        }



    }
}
