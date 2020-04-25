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
    public partial class FrmImpressao : Form
    {
        public FrmImpressao()
        {
            InitializeComponent();
        }

        private void btnLerDadosContrato_Click(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message); 
            }
        }
    }
}
