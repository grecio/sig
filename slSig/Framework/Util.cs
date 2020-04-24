using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Globalization;

namespace ScaWeb
{
    public class Util
    {

        #region Estuturas

        public class UF
        {

            private string ID;

            public string id
            {
                get { return ID; }
                set { ID = value; }
            }
            private string ESTADO;

            public string estado
            {
                get { return ESTADO; }
                set { ESTADO = value; }
            }

            public UF(string id, string estado)
            {

                this.ID = id;
                this.ESTADO = estado;

            }

        }

        #endregion

        #region Métodos Publicos

        public static int Val(object _value)
        {

            if (_value == null || _value == string.Empty)
            {
                return 0;
            }

            string value = Convert.ToString(_value);

            string returnVal = string.Empty;
            MatchCollection collection = Regex.Matches(value, "^[-]?[\\d]+");
            foreach (Match match in collection)
            {

                returnVal += match.ToString();
            }

            if (returnVal != value)
            {
                return 0;
            }
            else
            {
                return int.Parse(returnVal);
            }


        }

        public static long ValLong(object _value)
        {

            if (_value == null)
            {
                return 0;
            }

            string value = Convert.ToString(_value);

            string returnVal = string.Empty;

            MatchCollection collection = Regex.Matches(value, "^[-]?[\\d]+");
            
            foreach (Match match in collection)
            {

                returnVal += match.ToString();
            }

            if (returnVal != value)
            {
                return 0;
            }
            else
            {
                return long.Parse(returnVal);
            }
        }

        public static List<UF> ObterUF()
        {

            var obj = new List<UF>();


            obj.Add(new UF("",""));
            obj.Add(new UF("AC", "AC"));
            obj.Add(new UF("AL", "AL"));
            obj.Add(new UF("AP", "AP"));
            obj.Add(new UF("AM", "AM"));
            obj.Add(new UF("BA", "BA"));
            obj.Add(new UF("CE", "CE"));
            obj.Add(new UF("DF", "DF"));
            obj.Add(new UF("ES", "ES"));
            obj.Add(new UF("GO", "GO"));
            obj.Add(new UF("MA", "MA"));
            obj.Add(new UF("MT", "MT"));
            obj.Add(new UF("MS", "MS"));
            obj.Add(new UF("MG", "MG"));
            obj.Add(new UF("PA", "PA"));
            obj.Add(new UF("PB", "PB"));
            obj.Add(new UF("PR", "PR"));
            obj.Add(new UF("PE", "PE"));
            obj.Add(new UF("PI", "PI"));
            obj.Add(new UF("RJ", "RJ"));
            obj.Add(new UF("RN", "RN"));
            obj.Add(new UF("RS", "RS"));
            obj.Add(new UF("RO", "RO"));
            obj.Add(new UF("RR", "RR"));
            obj.Add(new UF("SC", "SC"));
            obj.Add(new UF("SP", "SP"));
            obj.Add(new UF("SE", "SE"));
            obj.Add(new UF("TO", "TO"));

            return obj;

        }

        public static string RemoverAcentos(string texto)
        {
            string s = texto.Normalize(NormalizationForm.FormD);

            StringBuilder sb = new StringBuilder();

            for (int k = 0; k < s.Length; k++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(s[k]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(s[k]);
                }
            }
            return sb.ToString();
        }

        #endregion                


    }
}
