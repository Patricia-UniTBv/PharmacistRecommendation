namespace Cnas.Siui.Cnp2Cid
{
    partial class Cnp2CidForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBoxCnp2Cid = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.labelCNP = new System.Windows.Forms.Label();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.textCid = new System.Windows.Forms.TextBox();
            this.textCnp = new System.Windows.Forms.TextBox();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.groupBoxCnp2Cid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxCnp2Cid
            // 
            this.groupBoxCnp2Cid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxCnp2Cid.Controls.Add(this.label2);
            this.groupBoxCnp2Cid.Controls.Add(this.labelCNP);
            this.groupBoxCnp2Cid.Controls.Add(this.btnGenerate);
            this.groupBoxCnp2Cid.Controls.Add(this.textCid);
            this.groupBoxCnp2Cid.Controls.Add(this.textCnp);
            this.groupBoxCnp2Cid.Location = new System.Drawing.Point(12, 6);
            this.groupBoxCnp2Cid.Name = "groupBoxCnp2Cid";
            this.groupBoxCnp2Cid.Size = new System.Drawing.Size(322, 84);
            this.groupBoxCnp2Cid.TabIndex = 0;
            this.groupBoxCnp2Cid.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(16, 53);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = "CID";
            // 
            // labelCNP
            // 
            this.labelCNP.AutoSize = true;
            this.labelCNP.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCNP.Location = new System.Drawing.Point(16, 22);
            this.labelCNP.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelCNP.Name = "labelCNP";
            this.labelCNP.Size = new System.Drawing.Size(36, 16);
            this.labelCNP.TabIndex = 3;
            this.labelCNP.Text = "CNP";
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGenerate.Location = new System.Drawing.Point(210, 17);
            this.btnGenerate.Margin = new System.Windows.Forms.Padding(4);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(102, 26);
            this.btnGenerate.TabIndex = 1;
            this.btnGenerate.Text = "Generează";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // textCid
            // 
            this.textCid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textCid.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textCid.Location = new System.Drawing.Point(60, 50);
            this.textCid.Margin = new System.Windows.Forms.Padding(4);
            this.textCid.Name = "textCid";
            this.textCid.ReadOnly = true;
            this.textCid.Size = new System.Drawing.Size(252, 22);
            this.textCid.TabIndex = 2;
            // 
            // textCnp
            // 
            this.textCnp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textCnp.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textCnp.Location = new System.Drawing.Point(60, 19);
            this.textCnp.Margin = new System.Windows.Forms.Padding(4);
            this.textCnp.MaxLength = 13;
            this.textCnp.Name = "textCnp";
            this.textCnp.Size = new System.Drawing.Size(131, 22);
            this.textCnp.TabIndex = 0;
            this.textCnp.KeyPress += new System.Windows.Forms.KeyPressEventHandler( this.textCnp_KeyPress );
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // Cnp2CidForm
            // 
            this.AcceptButton = this.btnGenerate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 102);
            this.Controls.Add(this.groupBoxCnp2Cid);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Cnp2CidForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Generator CID";
            this.groupBoxCnp2Cid.ResumeLayout(false);
            this.groupBoxCnp2Cid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxCnp2Cid;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelCNP;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.TextBox textCid;
        private System.Windows.Forms.TextBox textCnp;
        private System.Windows.Forms.ErrorProvider errorProvider;
    }
}

