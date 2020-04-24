using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Globalization;

namespace Framework
{
    public class Formatador : IFormattable
    {

        #region Atributos


        private string _Texto;

        #endregion

        #region Construtores


        public Formatador(string StrTexto)
        {
            _Texto = SoNumero(StrTexto);

        }

        #endregion

        #region "Métodos"

        public static string SoNumero(string strTexto, string strExcessao = "")
        {

            string strAux = null;
            string strFrag = null;

            int intCont = 0;

            strAux = string.Empty;


            for (intCont = 0; intCont <= strTexto.Length - 1; intCont++)
            {
                strFrag = strTexto.Substring(intCont, 1);


                if (strExcessao == "X")
                {

                    if (strFrag != strExcessao)
                    {

                        if (Information.IsNumeric(strFrag))
                        {
                            strAux = strAux + strFrag;

                        }

                    }
                    else
                    {
                        strAux = strAux + strFrag;

                    }


                }
                else
                {
                    if (Information.IsNumeric(strFrag))
                    {
                        strAux = strAux + strFrag;

                    }

                }
            }

            return strAux;

        }

        public static string SoLetras(string strTexto)
        {

            byte vPos = 0;

            const string vComAcento = "ÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÒÓÔÕÖÙÚÛÜàáâãäåçèéêëìíîïòóôõöùúûü";
            const string vSemAcento = "AAAAAACEEEEIIIIOOOOOUUUUaaaaaaceeeeiiiiooooouuuu";


            for (int i = 1; i <= Strings.Len(strTexto); i++)
            {
                vPos = (byte)Strings.InStr(1, vComAcento, strTexto.Substring(i, 1));


                if (vPos > 0)
                {
                    var x = vSemAcento.Substring(vPos, 1);

                    strTexto = x.Substring(i, 1);

                }
            }

            return strTexto;

        }

        public static string CPF(string strTexto)
        {
            if (strTexto == "")
            {
                return "";
            }
            else
            {
                return String.Format(@"{0:000\.000\.000\-00}", long.Parse(strTexto));
            }

        }

        public static string CEP(string strTexto)
        {
            if (strTexto == "")
            {
                return "";
            }
            else
            {
                return String.Format(@"{0:00\.000\-000}", long.Parse(strTexto));
            }

        }

        public static string CNPJ(string strTexto)
        {
            return String.Format(@"{0:00\.000\.000\/0000\-00}", long.Parse(strTexto));
        }

        public static string Telefone(string strTexto)
        {
            strTexto = SoNumero(strTexto);
            if (strTexto != "")
                strTexto = string.Format("{0:(###) ####-####}", long.Parse(strTexto));

            return strTexto;
        }


        public static string ObterMesExtenso(byte intMes, bool boolAbreviar = false)
        {

            string strAux = string.Empty;

            switch (intMes)
            {

                case 1:

                    strAux = (boolAbreviar ? "JAN" : "Janeiro");

                    break;
                case 2:

                    strAux = (boolAbreviar ? "FEV" : "Fevereiro");

                    break;
                case 3:

                    strAux = (boolAbreviar ? "MAR" : "Março");

                    break;
                case 4:

                    strAux = (boolAbreviar ? "ABR" : "Abril");

                    break;
                case 5:

                    strAux = (boolAbreviar ? "MAI" : "Maio");

                    break;
                case 6:

                    strAux = (boolAbreviar ? "JUN" : "Junho");

                    break;
                case 7:

                    strAux = (boolAbreviar ? "JUL" : "Julho");

                    break;
                case 8:

                    strAux = (boolAbreviar ? "AGO" : "Agosto");

                    break;
                case 9:

                    strAux = (boolAbreviar ? "SET" : "Setembro");

                    break;
                case 10:

                    strAux = (boolAbreviar ? "OUT" : "Outubro");

                    break;
                case 11:

                    strAux = (boolAbreviar ? "NOV" : "Novembro");

                    break;
                case 12:

                    strAux = (boolAbreviar ? "DEZ" : "Dezembro");

                    break;

            }

            return strAux;

        }

        public static string SemEspacos(string strTexto)
        {

            string strAux = null;
            int intCont = 0;
            string strFrag = null;

            strAux = string.Empty;


            for (intCont = 1; intCont <= Strings.Len(strTexto); intCont++)
            {
                strFrag = Strings.Mid(strTexto, intCont, 1);

                if (strFrag != " ")
                {
                    strAux = strAux + strFrag;
                }

            }

            return strAux;

        }

        public static string ExtensaoArquivo(string strArquivo)
        {

            string strAux = "";
            int lngPos = 0;

            strAux = Strings.StrReverse(strArquivo);

            lngPos = Strings.InStr(strAux, ".");

            if (lngPos == -1 || lngPos == 0)
                return strArquivo;

            strAux = strAux.Substring(1, lngPos);

            return Strings.StrReverse(strAux);

        }

        public static string NomeArquivo(string strArquivo)
        {

            string strAux = null;
            int lngPos = 0;

            strAux = Strings.StrReverse(strArquivo);

            lngPos = Strings.InStr(strAux, "\\");

            if (lngPos == -1 || lngPos == 0)
                return strArquivo;

            strAux = Strings.Mid(strAux, 1, lngPos - 1);

            return Strings.StrReverse(strAux);

        }

        public static byte[] ConverteBinario(string str)
        {

            return System.Text.Encoding.Unicode.GetBytes(str);

        }

        public static bool IsDate(string date, string format)
        {
            DateTime parsedDate;

            bool isValidDate;

            isValidDate = DateTime.TryParseExact(

                date,

                format,

                CultureInfo.InvariantCulture,

                DateTimeStyles.None,

                out parsedDate);

            return isValidDate;
        }



        #endregion

        #region "Métodos Sobrecarregados"

        public virtual string ToString(string format, System.IFormatProvider formatProvider)
        {

            string aux = string.Empty;

            if (format == "CNPJ")
            {
                aux = string.Format("{0:00\\.000\\.000\\/0000\\-00}", Convert.ToDouble(_Texto));


            }
            else if (format == "CPF")
            {
                aux = string.Format("{0:000\\.000\\.000\\-00}", Convert.ToDouble(_Texto));

            }

            return aux;

        }

        #endregion

    }

}
