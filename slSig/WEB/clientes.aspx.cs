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

            IdCliente = dt[0].ID;

            txtNome.Text = dt[0].Nome;
            txtEmail.Text = dt[0].Email;
            txtCPF.Text = dt[0].CPF;
            txtFixoDDD.Text = dt[0].FixoDDD;
            txtFixoNumero.Text = dt[0].FixoNumero;
            txtCelularDDD.Text = dt[0].CelularDDD;
            txtCelularNumero.Text = dt[0].CelularNumero;

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

        protected void grdList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var lblTelefone = e.Row.FindControl("lblTelefone") as Label;

                
                if (lblTelefone != null)
                {
                    if (DataBinder.Eval(e.Row.DataItem, "CelularNUmero").ToString().Length > 7)
                    {
                        lblTelefone.Text = string.Format("{0} {1}", DataBinder.Eval(e.Row.DataItem, "CelularDDD"), DataBinder.Eval(e.Row.DataItem, "CelularNUmero"));                        
                    }
                    
                }
            }
        }

        protected void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                BLL.Cliente.Salvar(txtNome.Text, txtEmail.Text,
                    txtCPF.Text, txtFixoDDD.Text, txtFixoNumero.Text,
                    txtCelularDDD.Text, txtCelularNumero.Text, IdCliente);

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