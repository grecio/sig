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
    public partial class FrmImpressao : Form
    {

   

        public FrmImpressao()
        {
            InitializeComponent();

           
        }

        private void LerCartao()
        {

            var dtCartao = BLL.Cartao.SelecionarTitularPorContrato(txtNumeroContrato.Text);

            Validador.Validar(dtCartao.Any(), "Nenhum registro encontrdo para o contrato informado.");


            lblTitular.Text = dtCartao[0].nome;
            lblNumeroContrato.Text = dtCartao[0].contrato;
            lblPlano.Text = dtCartao[0].planocliente;


            lblPlano.AutoSize = false;
            lblPlano.TextAlign = ContentAlignment.MiddleRight;
            lblPlano.RightToLeft = RightToLeft.Yes;
            lblPlano.TextAlign = ContentAlignment.MiddleRight;

            int widthLabel = 100;
            using (Graphics g = lblPlano.CreateGraphics())
            {
                SizeF size = g.MeasureString(dtCartao[0].planocliente, lblPlano.Font, widthLabel);
                lblPlano.Height = (int)Math.Ceiling(size.Height);
                lblPlano.Text = dtCartao[0].planocliente;
            }


            lblNumeroContrato.AutoSize = false;
            lblNumeroContrato.TextAlign = ContentAlignment.MiddleRight;
           
            using (Graphics g = lblNumeroContrato.CreateGraphics())
            {
                SizeF size = g.MeasureString(dtCartao[0].contrato, lblNumeroContrato.Font, widthLabel);
                lblNumeroContrato.Height = (int)Math.Ceiling(size.Height);
                lblNumeroContrato.Text = dtCartao[0].contrato;
            }

            var dtDep = BLL.Cartao.ListarDependentes(dtCartao[0].idimportacaodados).AsQueryable().ToArray();


            lstDependentes.Items.Clear();

            if (dtDep.Any())
            {
                
                var items = dtDep.AsQueryable().ToArray().Take(10);

                foreach (var item in items)
                {
                    lstDependentes.Items.Add(item.dependentes);
                }
            }

        
        }

        private void btnLerDadosContrato_Click(object sender, EventArgs e)
        {
            try
            {
                Validador.Validar(!string.IsNullOrWhiteSpace(txtNumeroContrato.Text), "Informe o Número do Contrato.");


                LerCartao();

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message); 
            }
        }

        private void btnNovo_Click(object sender, EventArgs e)
        {
            txtNumeroContrato.Text = string.Empty;

            lblTitular.Text = "NOME DO TITULAR";
            lblNumeroContrato.Text = "NUMERO DO CONTRATO";
            lblPlano.Text = "PLANO DO CONTRATO";

            lstDependentes.Items.Clear();
            txtNumeroContrato.Focus();
        }
    }
}
