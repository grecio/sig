using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Web.UI;

namespace WebFramework
{
    public static class JavaScriptAJAX
    {


        public static void DocLocation(System.Web.UI.Page oPage, string strURL)
        {
            System.Text.StringBuilder strScript = new System.Text.StringBuilder();

            var _with1 = strScript;

            _with1.Append("<script language=\"javascript\">");
            _with1.Append("document.location.href='" + strURL + "';");
            _with1.Append("</script>");

            ScriptManager.RegisterStartupScript(oPage, typeof(string), new Guid().ToString(), _with1.ToString(), false);                        

        }

        public static void Retornar(System.Web.UI.Page oPage, string strURL)
        {
            System.Text.StringBuilder strScript = new System.Text.StringBuilder();

            var _with1 = strScript;

            _with1.Append("<script language=\"javascript\">");
            _with1.Append("top.location.href='" + strURL + "';");
            _with1.Append("</script>");

            ScriptManager.RegisterStartupScript(oPage, typeof(string), new Guid().ToString(), _with1.ToString(), false);

        }


        public static void SetFocus(System.Web.UI.Page oPage, System.Web.UI.Control objFocus, bool boolFiltro = false)
        {

            System.Text.StringBuilder strScript = new System.Text.StringBuilder();

            var _with2 = strScript;

            _with2.Append("<script language=\"javascript\">");
            _with2.Append("$(\"#" + objFocus.ClientID + "\").focus();");


            if (!boolFiltro)
            {
                _with2.Append("$(\"#" + objFocus.ClientID + "\").toggleClass('txt-focus');");

            }

            _with2.Append("</script>");

            ScriptManager.RegisterStartupScript(oPage, typeof(string), new Guid().ToString(), _with2.ToString(), false);

        }


        public static void SetClick(System.Web.UI.Page oPage, System.Web.UI.Control objControl)
        {
            System.Text.StringBuilder strScript = new System.Text.StringBuilder();

            var _with3 = strScript;

            _with3.Append("<script language=\"javascript\">");
            _with3.Append("$(\"#" + objControl.ClientID + "\").click();");
            _with3.Append("</script>");

            ScriptManager.RegisterStartupScript(oPage, typeof(string), new Guid().ToString(), _with3.ToString(), false);
            

        }


        public static void ShowMsg(System.Web.UI.Page oPage, string Message, System.Web.UI.Control objFocus = null, bool ispostback = false, string postbackpagina = "")
        {

            if (Strings.InStr(Message.ToUpper(), "DELETE statement conflicted".ToUpper()) != 0 )
            {
                Message = "O registro não pode ser excluído. Existem informações relacionadas a ele.";

            }

            if (Strings.InStr(Message.ToUpper(), "Violation of PRIMARY KEY constraint".ToUpper())!= 0)
            {
                Message = "Não permitido registros duplicados.";
            }

            if (Strings.InStr(Message.ToUpper(), "Violation of UNIQUE KEY constraint".ToUpper()) != 0)
            {
                Message = "Não permitido registros duplicados.";
            }

            System.Text.StringBuilder strScript = new System.Text.StringBuilder();

            var _with4 = strScript;

            _with4.Append("<script language=\"javascript\">");

            _with4.Append("alert('" + StringJava(Message) + "');");


            if (objFocus != null)
            {
                _with4.Append("$(\"#" + objFocus.ClientID + "\").focus();");                

            }

            if (ispostback)
            {

                if (postbackpagina.Equals(string.Empty))
                {

                    _with4.Append("document.location.href='" + oPage.Request.Url.AbsoluteUri + "';");

                }
                else
                {

                    _with4.Append("document.location.href='" + postbackpagina + "';");

                }                

            }

            _with4.Append("</script>");

            ScriptManager.RegisterStartupScript(oPage, typeof(string), new Guid().ToString(), _with4.ToString(), false);
                        
        }

        public static void ShowRPT(System.Web.UI.Page oPage, string strRpt, string strParam, string strTipo = "0")
        {

            ScriptManager.RegisterStartupScript(oPage, typeof(string), new Guid().ToString(), string.Format("showCrystalRPT('{0}', '{1}', '{2}');", strRpt, strParam, strTipo), true);            

        }

        private static string StringJava(string Texto)
        {

            Texto = Strings.Replace(Texto, "\\", "\\\\");
            Texto = Strings.Replace(Texto, Constants.vbCrLf, "\\n");
            Texto = Strings.Replace(Texto, "'", "\\'");
            Texto = Strings.Replace(Texto, "\"", "\\\"");

            return Texto;

        } 
    }

}
