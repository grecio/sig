using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web
{
    public class FormBase : System.Web.UI.Page
    {

        public string Hash
        {
            get
            {
                return Session["Hash"] == null ? string.Empty : Session["Hash"].ToString();
            }

            set
            {
                Session["Hash"] = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int IdUsuarioLogado
        {
            get
            {
                return Session["IdUsuarioLogado"] == null ? 0 : Convert.ToInt32(Session["IdUsuarioLogado"]);
            }

            set
            {
                Session["IdUsuarioLogado"] = value;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public string NomeUsuarioLogado
        {

            get
            {
                return Session["NomeUsuarioLogado"] == null ? string.Empty : Session["NomeUsuarioLogado"].ToString();
            }

            set
            {
                Session["NomeUsuarioLogado"] = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            if (IdUsuarioLogado == 0)
            {
                Response.Redirect("logout.aspx");
            }
        }
    }
}