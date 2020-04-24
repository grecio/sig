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
    public static class Usuario
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static DAL.DsUsuario.UsuarioDataTable Listar()
        {
            using (var adp = new DAL.DsUsuarioTableAdapters.UsuarioTableAdapter())
            {
                return adp.Listar();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static DAL.DsUsuario.UsuarioDataTable Selecionar(int ID)
        {
            Validador.Validar(ID > 0, "Nenhum usuário encontrado. Autentique-se novamente no sistema.");

            using (var adp = new DAL.DsUsuarioTableAdapters.UsuarioTableAdapter())
            {
                return adp.Selecionar(ID);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <param name="senha"></param>
        /// <returns></returns>
        public static DAL.DsUsuario.UsuarioDataTable EfetuarLogon(string login, string senha)
        {
            Validador.Validar(!string.IsNullOrWhiteSpace(login), "Informe o login.");

            Validador.Validar(!string.IsNullOrWhiteSpace(senha), "Informe a senha.");

            using (var adp = new DAL.DsUsuarioTableAdapters.UsuarioTableAdapter())
            {
                return adp.EfetuarLogin(login, senha);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="senhaatual"></param>
        /// <param name="senha1"></param>
        /// <param name="senha2"></param>
        /// <param name="ID"></param>
        public static void AtualizarSenha(string senhaatual, string senha1, string senha2, global::System.Nullable<int> ID)
        {
            Validador.Validar(!string.IsNullOrWhiteSpace(senhaatual), "Informe a senha atual.");
            Validador.Validar(!string.IsNullOrWhiteSpace(senha1), "Informe a nova senha.");
            Validador.Validar(!string.IsNullOrWhiteSpace(senha2), "Repita a senha");

            using (var adp = new DAL.DsUsuarioTableAdapters.UsuarioTableAdapter())
            {

                var dt = adp.Selecionar(ID);

                Validador.Validar(dt.Count > 0, "Não foi possível obter o usuário. autentique-se no sistema novamente.");

                Validador.Validar(senhaatual.ToLower().Equals(dt[0].senha.ToLower()), "Nenhum usuário encontrado para a senha atual informada.");

                Validador.Validar(senha1.ToLower().Equals(senha2.ToLower()), "As novas senhas informadas não conferem.");


                adp.AtualizarSenha(senha1, ID);
            }

        }

    }
}
