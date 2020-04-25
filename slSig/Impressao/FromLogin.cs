using Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Impressao
{
    public partial class FromLogin : Form
    {
        public FromLogin()
        {
            InitializeComponent();
        }

        private void btnEntrar_Click(object sender, EventArgs e)
        {
            try
            {
                var dt = BLL.Usuario.EfetuarLogon(txtLogin.Text, txtSenha.Text);

                Validador.Validar(dt.Any(), "Nenhum usuário encontrado para email/senha informados.");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Invoke(Action p)
        {
            throw new NotImplementedException();
        }
    }
}
