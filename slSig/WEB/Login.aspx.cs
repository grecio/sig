using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Web
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Login : System.Web.UI.Page
    {
        #region Métodos Privados

        /// <summary>
        /// 
        /// </summary>
        private void Entrar()
        {
            var dt = BLL.Usuario.EfetuarLogon(txtLogin.Text, txtSenha.Text);

            Validador.Validar(dt.Count > 0, "Nenhum usuário encontrado para o login e senha informados.");

            if (dt.Count > 0)
            {

                Session["IdUsuarioLogado"] = dt[0].idusuario;
                Session["NomeUsuarioLogado"] = dt[0].nome;

                Response.Redirect("painel.asp");
            }
        } 

        #endregion
        
        #region Eventos

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Request["logoff"]))
            {
                lblMensagem.Text = "Sessão expirou. por favor, efetue login novamente.";
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnEntrar_Click(object sender, EventArgs e)
        {
            try
            {

                Entrar();

            }
            catch (Exception ex)
            {
                lblMensagem.Text = ex.Message;
            }
        }

        protected void btnColaborador_Click(object sender, EventArgs e)
        {
            Response.Redirect("colaborador.aspx");
        } 

        #endregion

       
    }
}