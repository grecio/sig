using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
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
    public partial class FrmImpressaoContratos : Form
    {
        public FrmImpressaoContratos()
        {
            InitializeComponent();
        }

        private void ExibirRelatorio()
        {

            ReportDocument cryRpt = new ReportDocument();
            string path_ = System.AppDomain.CurrentDomain.BaseDirectory;
            string caminho = path_ + "reports/rptListaContratos.rpt";
            cryRpt.Load(caminho);


            ConnectionInfo connInfo = new ConnectionInfo();
            connInfo.ServerName = "144.217.49.13";
            connInfo.DatabaseName = "natalprinter";
            connInfo.UserID = "sa";
            connInfo.Password = "buga@tec_buga1";

            TableLogOnInfo tableLogOnInfo = new TableLogOnInfo();
            tableLogOnInfo.ConnectionInfo = connInfo;


            foreach (Table table in cryRpt.Database.Tables)
            {
                table.ApplyLogOnInfo(tableLogOnInfo);
                table.LogOnInfo.ConnectionInfo.ServerName = connInfo.ServerName;
                table.LogOnInfo.ConnectionInfo.DatabaseName = connInfo.DatabaseName;
                table.LogOnInfo.ConnectionInfo.UserID = connInfo.UserID;
                table.LogOnInfo.ConnectionInfo.Password = connInfo.Password;

                // Apply the schema name to the table's location
                table.Location = "dbo." + table.Location;
            }

          
           
            crView.ReportSource = cryRpt;
            crView.Refresh();
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                ExibirRelatorio();
            }
            catch (Exception ex)
            {

                MessageBox.Show("Não foi possível obter os dados para exibição do relatório");
            }
        }

    }
}
