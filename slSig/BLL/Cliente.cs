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
        public static DAL.DsClientes.ClienteDataTable Listar()
        {

            using (var adp = new DAL.DsClientesTableAdapters.ClienteTableAdapter())
            {
                return adp.Listar();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static DAL.DsClientes.ClienteDataTable Selecionar(global::System.Nullable<int> ID)
        {

            using (var adp = new DAL.DsClientesTableAdapters.ClienteTableAdapter())
            {
                return adp.Selecionar(ID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Email"></param>
        /// <returns></returns>
        public static DAL.DsClientes.ClienteDataTable SelecionarPorEmail(string Email)
        {

            using (var adp = new DAL.DsClientesTableAdapters.ClienteTableAdapter())
            {
                return adp.SelecionarPorEmail(Email);
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
        public static void Salvar(string Nome, string Email, string CPF, string FixoDDD, string FixoNumero, string CelularDDD, string CelularNumero, global::System.Nullable<int> ID)
        {

            Validador.Validar(!string.IsNullOrWhiteSpace(Nome), "Informe o nome.");
            Validador.Validar(!string.IsNullOrWhiteSpace(Email), "Informe o e-mail.");

            using (var adp = new DAL.DsClientesTableAdapters.ClienteTableAdapter())
            {
                var dt = adp.ExistePorEmail(ID, Email);

                Validador.Validar(dt.Count == 0, "Já existe um cliente cadatrado com o e-mail informado.");

                if (ID == 0)
                {
                    adp.Inserir(Nome, Email, CPF, FixoDDD, FixoNumero, CelularDDD, CelularNumero);
                }
                else
                {
                    adp.Atualizar(Nome, Email, CPF, FixoDDD, FixoNumero, CelularDDD, CelularNumero, ID);
                }
            }
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        public static void Excluir(int? ID)
        {

            using (var adp = new DAL.DsClientesTableAdapters.ClienteTableAdapter())
            {
                adp.Excluir(ID);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public static class Interesse
        {

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static DAL.DsClientes.ClienteInteresseDataTable Listar()
            {
                using (var adp = new DAL.DsClientesTableAdapters.ClienteInteresseTableAdapter())
                {
                    return adp.Listar();
                }

            }


            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static DAL.DsClientes.ClienteInteresseDataTable Selecionar(global::System.Nullable<int> ID)
            {
                using (var adp = new DAL.DsClientesTableAdapters.ClienteInteresseTableAdapter())
                {
                    return adp.Selecionar(ID);
                }

            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="CotaID"></param>
            /// <param name="ClienteID"></param>
            public static void Inserir(global::System.Nullable<int> CotaID, global::System.Nullable<int> ClienteID)
            {

                using (var adp = new DAL.DsClientesTableAdapters.ClienteInteresseTableAdapter())
                {
                    adp.Inserir(CotaID, ClienteID);
                }

            }

            public static void Excluir(global::System.Nullable<int> ID)
            {

                Validador.Validar(ID > 0, "Selecione um registro.");

                using (var adp = new DAL.DsClientesTableAdapters.ClienteInteresseTableAdapter())
                {
                    adp.Excluir(ID);
                }

            }
        }

    }
}
