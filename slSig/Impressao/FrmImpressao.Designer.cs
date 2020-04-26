namespace Impressao
{
    partial class FrmImpressao
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnNovo = new System.Windows.Forms.Button();
            this.btnLerDadosContrato = new System.Windows.Forms.Button();
            this.txtNumeroContrato = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblNumeroContrato = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblPlano = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblTitular = new System.Windows.Forms.Label();
            this.ptbCartao = new System.Windows.Forms.PictureBox();
            this.btnImprimir = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lstDependentes = new System.Windows.Forms.ListBox();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ptbCartao)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(491, 98);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Dados do Cartão:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnNovo);
            this.panel1.Controls.Add(this.btnLerDadosContrato);
            this.panel1.Controls.Add(this.txtNumeroContrato);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(6, 20);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(479, 71);
            this.panel1.TabIndex = 3;
            // 
            // btnNovo
            // 
            this.btnNovo.Location = new System.Drawing.Point(234, 29);
            this.btnNovo.Name = "btnNovo";
            this.btnNovo.Size = new System.Drawing.Size(110, 23);
            this.btnNovo.TabIndex = 3;
            this.btnNovo.Text = "Limpar Campos";
            this.btnNovo.UseVisualStyleBackColor = true;
            this.btnNovo.Click += new System.EventHandler(this.btnNovo_Click);
            // 
            // btnLerDadosContrato
            // 
            this.btnLerDadosContrato.Location = new System.Drawing.Point(153, 29);
            this.btnLerDadosContrato.Name = "btnLerDadosContrato";
            this.btnLerDadosContrato.Size = new System.Drawing.Size(75, 23);
            this.btnLerDadosContrato.TabIndex = 2;
            this.btnLerDadosContrato.Text = "Pesquisar";
            this.btnLerDadosContrato.UseVisualStyleBackColor = true;
            this.btnLerDadosContrato.Click += new System.EventHandler(this.btnLerDadosContrato_Click);
            // 
            // txtNumeroContrato
            // 
            this.txtNumeroContrato.Location = new System.Drawing.Point(10, 31);
            this.txtNumeroContrato.Name = "txtNumeroContrato";
            this.txtNumeroContrato.Size = new System.Drawing.Size(137, 20);
            this.txtNumeroContrato.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Número do Contrato:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lstDependentes);
            this.groupBox2.Controls.Add(this.lblPlano);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.lblNumeroContrato);
            this.groupBox2.Controls.Add(this.pictureBox2);
            this.groupBox2.Controls.Add(this.pictureBox1);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.lblTitular);
            this.groupBox2.Controls.Add(this.ptbCartao);
            this.groupBox2.Location = new System.Drawing.Point(12, 117);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(491, 237);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Cartão";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(301, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "N do Contrato:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblNumeroContrato
            // 
            this.lblNumeroContrato.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNumeroContrato.AutoSize = true;
            this.lblNumeroContrato.BackColor = System.Drawing.Color.White;
            this.lblNumeroContrato.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNumeroContrato.ForeColor = System.Drawing.Color.Black;
            this.lblNumeroContrato.Location = new System.Drawing.Point(301, 83);
            this.lblNumeroContrato.MaximumSize = new System.Drawing.Size(270, 0);
            this.lblNumeroContrato.Name = "lblNumeroContrato";
            this.lblNumeroContrato.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblNumeroContrato.Size = new System.Drawing.Size(133, 13);
            this.lblNumeroContrato.TabIndex = 12;
            this.lblNumeroContrato.Text = "NUMERO CONTRATO";
            this.lblNumeroContrato.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(301, 118);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Plano:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPlano
            // 
            this.lblPlano.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPlano.AutoSize = true;
            this.lblPlano.BackColor = System.Drawing.Color.White;
            this.lblPlano.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlano.ForeColor = System.Drawing.Color.Black;
            this.lblPlano.Location = new System.Drawing.Point(301, 131);
            this.lblPlano.MaximumSize = new System.Drawing.Size(270, 0);
            this.lblPlano.Name = "lblPlano";
            this.lblPlano.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblPlano.Size = new System.Drawing.Size(142, 13);
            this.lblPlano.TabIndex = 9;
            this.lblPlano.Text = "PLANO DO CONTRATO";
            this.lblPlano.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::Impressao.Properties.Resources.ezgif_com_webp_to_png;
            this.pictureBox2.InitialImage = global::Impressao.Properties.Resources.logo_sempre;
            this.pictureBox2.Location = new System.Drawing.Point(442, 165);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(34, 37);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 11;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Impressao.Properties.Resources.logo_sempre;
            this.pictureBox1.InitialImage = global::Impressao.Properties.Resources.logo_sempre;
            this.pictureBox1.Location = new System.Drawing.Point(368, 165);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(63, 37);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 10;
            this.pictureBox1.TabStop = false;
            // 
            // lblTitular
            // 
            this.lblTitular.AutoSize = true;
            this.lblTitular.BackColor = System.Drawing.Color.White;
            this.lblTitular.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitular.ForeColor = System.Drawing.Color.Black;
            this.lblTitular.Location = new System.Drawing.Point(13, 28);
            this.lblTitular.MaximumSize = new System.Drawing.Size(270, 0);
            this.lblTitular.Name = "lblTitular";
            this.lblTitular.Size = new System.Drawing.Size(122, 13);
            this.lblTitular.TabIndex = 3;
            this.lblTitular.Text = "NOME DO TITULAR";
            // 
            // ptbCartao
            // 
            this.ptbCartao.BackColor = System.Drawing.Color.White;
            this.ptbCartao.Location = new System.Drawing.Point(6, 20);
            this.ptbCartao.Name = "ptbCartao";
            this.ptbCartao.Size = new System.Drawing.Size(479, 190);
            this.ptbCartao.TabIndex = 0;
            this.ptbCartao.TabStop = false;
            // 
            // btnImprimir
            // 
            this.btnImprimir.Location = new System.Drawing.Point(422, 360);
            this.btnImprimir.Name = "btnImprimir";
            this.btnImprimir.Size = new System.Drawing.Size(75, 23);
            this.btnImprimir.TabIndex = 3;
            this.btnImprimir.Text = "Imprimir";
            this.btnImprimir.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(13, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Dependentes";
            // 
            // lstDependentes
            // 
            this.lstDependentes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstDependentes.FormattingEnabled = true;
            this.lstDependentes.Location = new System.Drawing.Point(15, 70);
            this.lstDependentes.Name = "lstDependentes";
            this.lstDependentes.Size = new System.Drawing.Size(280, 130);
            this.lstDependentes.TabIndex = 13;
            // 
            // FrmImpressao
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 397);
            this.Controls.Add(this.btnImprimir);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmImpressao";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Natal Printer - Impressão";
            this.groupBox1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ptbCartao)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnLerDadosContrato;
        private System.Windows.Forms.TextBox txtNumeroContrato;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.PictureBox ptbCartao;
        private System.Windows.Forms.Button btnImprimir;
        private System.Windows.Forms.Label lblTitular;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnNovo;
        private System.Windows.Forms.Label lblPlano;
        private System.Windows.Forms.Label lblNumeroContrato;
        private System.Windows.Forms.ListBox lstDependentes;
        private System.Windows.Forms.Label label2;
    }
}