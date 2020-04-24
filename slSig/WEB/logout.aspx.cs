using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Web
{
    public partial class logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            Session.Abandon();
            Response.Clear();
            Response.Write(@"<script language=""javascript"">top.location='login.aspx?logoff=false';</script>");
            Response.End();

        }
    }
}