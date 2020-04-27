namespace Impressao
{
    partial class FrmImpressaoContratos
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.crView = new CrystalDecisions.Windows.Forms.CrystalReportViewer();
            this.SuspendLayout();
            // 
            // crView
            // 
            this.crView.ActiveViewIndex = -1;
            this.crView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.crView.Cursor = System.Windows.Forms.Cursors.Default;
            this.crView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.crView.Location = new System.Drawing.Point(0, 0);
            this.crView.Name = "crView";
            this.crView.ShowLogo = false;
            this.crView.Size = new System.Drawing.Size(514, 397);
            this.crView.TabIndex = 0;
            this.crView.ToolPanelView = CrystalDecisions.Windows.Forms.ToolPanelViewType.None;
            // 
            // FrmImpressaoContratos
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 397);
            this.Controls.Add(this.crView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FrmImpressaoContratos";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Natal Printer - Contratos";
            this.ResumeLayout(false);

        }

        #endregion

        private CrystalDecisions.Windows.Forms.CrystalReportViewer crView;
    }
}