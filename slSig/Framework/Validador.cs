using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;

namespace Framework
{
    public class Validador
    {

        #region Constantes
        
        public const string REGULAR_EXPRESSION_DATA = "(((0[1-9]|[12][0-9]|3[01])([/])(0[13578]|10|12)([/])(\\d{4}))|(([0][1-9]|[12][0-9]|30)([/])(0[469]|11)([/])(\\d{4}))|((0[1-9]|1[0-9]|2[0-8])([/])(02)([/])(\\d{4}))|((29)(\\.|-|\\/)(02)([/])([02468][048]00))|((29)([/])(02)([/])([13579][26]00))|((29)([/])(02)([/])([0-9][0-9][0][48]))|((29)([/])(02)([/])([0-9][0-9][2468][048]))|((29)([/])(02)([/])([0-9][0-9][13579][26])))";        
        public const string REGULAR_EXPRESSION_EMAIL = @"^(([^<>()[\]\\.,;:\s@\""]+"
                        + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"
                        + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
                        + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
                        + @"[a-zA-Z]{2,}))$";

        #endregion


        public static void Validar(bool condicao, string mensagem)
        {
            if (!condicao)
            {
                throw new ApplicationException(mensagem);
            }
        }

        public static bool ValidarData(string strData)
        {         
          
            return Regex.IsMatch(strData, REGULAR_EXPRESSION_DATA);

        }

        public static bool ValidarEmail(string strEmail)
        {
        
            return Regex.IsMatch(strEmail, REGULAR_EXPRESSION_EMAIL);

        }

        public static bool ValidaCPF(string CPF)
        {

            int i = 0;
            int x = 0;
            int n1 = 0;
            int n2 = 0;

            CPF = CPF.Trim();


            for (i = 0; i <= CPF.Length - 1; i++)
            {

                if (CPF.Length != 11 || CPF[i].Equals(CPF))
                {
                    return false;

                }

            }


            for (x = 0; x <= 1; x++)
            {
                n1 = 0;


                for (i = 0; i <= 8 + x; i++)
                {
                    
                    n1 = n1 + Convert.ToInt32(Conversion.Val(CPF.Substring(i, 1)) * (10 + x - i));
                }

                n2 = 11 - (n1 - (Conversion.Int(n1 / 11) * 11));

                if (n2 == 10 | n2 == 11)
                    n2 = 0;


                if (n2 != Conversion.Val(CPF.Substring(9 + x, 1)))
                {
                    return false;

                }
            }

            return true;

        }



        public static Boolean ValidarCNPJ(String cnpj)
        {

            if (Regex.IsMatch(cnpj, @"(^(\d{2}.\d{3}.\d{3}/\d{4}-\d{2})|(\d{14})$)"))
            {

                return validaCnpj(cnpj);

            }

            else
            {

                return false;

            }

        }



        private static Boolean validaCnpj(String cnpj)
        {

            Int32[] digitos, soma, resultado;

            Int32 nrDig;

            String ftmt;

            Boolean[] cnpjOk;

            cnpj = cnpj.Replace("/", "");

            cnpj = cnpj.Replace(".", "");

            cnpj = cnpj.Replace("-", "");

            if (cnpj == "00000000000000")
            {

                return false;

            }

            ftmt = "6543298765432";

            digitos = new Int32[14];

            soma = new Int32[2];

            soma[0] = 0;

            soma[1] = 0;

            resultado = new Int32[2];

            resultado[0] = 0;

            resultado[1] = 0;

            cnpjOk = new Boolean[2];

            cnpjOk[0] = false;

            cnpjOk[1] = false;

            try
            {

                for (nrDig = 0; nrDig < 14; nrDig++)
                {

                    digitos[nrDig] = int.Parse(cnpj.Substring(nrDig, 1));

                    if (nrDig <= 11)

                        soma[0] += (digitos[nrDig] *

                        int.Parse(ftmt.Substring(nrDig + 1, 1)));

                    if (nrDig <= 12)

                        soma[1] += (digitos[nrDig] *

                        int.Parse(ftmt.Substring(nrDig, 1)));

                }

                for (nrDig = 0; nrDig < 2; nrDig++)
                {

                    resultado[nrDig] = (soma[nrDig] % 11);

                    if ((resultado[nrDig] == 0) || (resultado[nrDig] == 1))

                        cnpjOk[nrDig] = (digitos[12 + nrDig] == 0);

                    else

                        cnpjOk[nrDig] = (digitos[12 + nrDig] == (

                        11 - resultado[nrDig]));

                }

                return (cnpjOk[0] && cnpjOk[1]);

            }

            catch
            {

                return false;

            }

        }

    }
}
