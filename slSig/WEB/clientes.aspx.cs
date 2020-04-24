using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebFramework;

namespace Web
{
    public partial class clientes : FormBase
    {

        #region Propriedades


        /// <summary>
        /// 
        /// </summary>
        private int IdCliente
        {
            get
            {
                return ViewState["IdCliente"] == null ? 0 : Convert.ToInt32(ViewState["IdCliente"]);
            }

            set
            {
                ViewState["IdCliente"] = value;
            }
        }

        #endregion

        #region Métodos Privados

        private void Listar()
        {
            grdList.DataSource = BLL.Cliente.Listar();
            grdList.DataBind();
        }

        private void Recuperar()
        {
            var dt = BLL.Cliente.Selecionar(IdCliente);

            Validador.Validar(dt.Count > 0, "Não foi possível recuperar o registro selecionado.");

            IdCliente = dt[0].idcliente;

            txtNome.Text = dt[0].nome;
            txtEmail.Text = dt[0].email;
            txtCpfCnpj.Text = dt[0].cpfcnpj;
            txtTelefone.Text = dt[0].telefone;
         }

        #endregion

        #region Eventos

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Listar();
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("clientes.aspx");
        }

       

        protected void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                BLL.Cliente.Salvar(txtNome.Text, txtEmail.Text,
                    txtCpfCnpj.Text, txtTelefone.Text, IdCliente);

                Listar();

            }
            catch (Exception ex)
            {

                JavaScript.ShowMsg(this.Page, ex.Message);
            }
        }

        protected void grdList_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {

                IdCliente = Convert.ToInt32(e.Keys[0]);

                BLL.Cliente.Excluir(IdCliente);

                Listar();

            }
            catch (Exception ex)
            {
                JavaScript.ShowMsg(this.Page, ex.Message);
            }
        }

        protected void grdList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                IdCliente = Convert.ToInt32(grdList.SelectedDataKey["ID"].ToString());

                Recuperar();

            }
            catch (Exception ex)
            {
                JavaScript.ShowMsg(this.Page, ex.Message);
            }

        }

        protected void grdList_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {

                grdList.PageIndex = e.NewPageIndex;
                Listar();

            }
            catch (Exception ex)
            {
                JavaScript.ShowMsg(this.Page, ex.Message);
            }
        }
        
        #endregion

    }
}