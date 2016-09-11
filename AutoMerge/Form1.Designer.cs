namespace AutoMerge
{
    partial class Form1
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
            if (disposing && (components != null)) {
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
            this.rdoFlac = new System.Windows.Forms.RadioButton();
            this.rdoM4a = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.rdo264 = new System.Windows.Forms.RadioButton();
            this.rdoHevc = new System.Windows.Forms.RadioButton();
            this.rdoAudioBoth = new System.Windows.Forms.RadioButton();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // rdoFlac
            // 
            this.rdoFlac.AutoSize = true;
            this.rdoFlac.Checked = true;
            this.rdoFlac.Location = new System.Drawing.Point(4, 5);
            this.rdoFlac.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoFlac.Name = "rdoFlac";
            this.rdoFlac.Size = new System.Drawing.Size(52, 24);
            this.rdoFlac.TabIndex = 0;
            this.rdoFlac.TabStop = true;
            this.rdoFlac.Text = "flac";
            this.rdoFlac.UseVisualStyleBackColor = true;
            // 
            // rdoM4a
            // 
            this.rdoM4a.AutoSize = true;
            this.rdoM4a.Location = new System.Drawing.Point(4, 42);
            this.rdoM4a.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoM4a.Name = "rdoM4a";
            this.rdoM4a.Size = new System.Drawing.Size(58, 24);
            this.rdoM4a.TabIndex = 1;
            this.rdoM4a.TabStop = true;
            this.rdoM4a.Text = "m4a";
            this.rdoM4a.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rdoAudioBoth);
            this.panel1.Controls.Add(this.rdoFlac);
            this.panel1.Controls.Add(this.rdoM4a);
            this.panel1.Location = new System.Drawing.Point(18, 20);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(92, 117);
            this.panel1.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(22, 147);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(310, 113);
            this.button1.TabIndex = 3;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.rdo264);
            this.panel2.Controls.Add(this.rdoHevc);
            this.panel2.Location = new System.Drawing.Point(118, 20);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(92, 80);
            this.panel2.TabIndex = 3;
            // 
            // rdo264
            // 
            this.rdo264.AutoSize = true;
            this.rdo264.Location = new System.Drawing.Point(4, 5);
            this.rdo264.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdo264.Name = "rdo264";
            this.rdo264.Size = new System.Drawing.Size(54, 24);
            this.rdo264.TabIndex = 0;
            this.rdo264.TabStop = true;
            this.rdo264.Text = "264";
            this.rdo264.UseVisualStyleBackColor = true;
            // 
            // rdoHevc
            // 
            this.rdoHevc.AutoSize = true;
            this.rdoHevc.Checked = true;
            this.rdoHevc.Location = new System.Drawing.Point(4, 42);
            this.rdoHevc.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoHevc.Name = "rdoHevc";
            this.rdoHevc.Size = new System.Drawing.Size(61, 24);
            this.rdoHevc.TabIndex = 1;
            this.rdoHevc.TabStop = true;
            this.rdoHevc.Text = "hevc";
            this.rdoHevc.UseVisualStyleBackColor = true;
            // 
            // rdoAudioBoth
            // 
            this.rdoAudioBoth.AutoSize = true;
            this.rdoAudioBoth.Location = new System.Drawing.Point(4, 75);
            this.rdoAudioBoth.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoAudioBoth.Name = "rdoAudioBoth";
            this.rdoAudioBoth.Size = new System.Drawing.Size(62, 24);
            this.rdoAudioBoth.TabIndex = 2;
            this.rdoAudioBoth.TabStop = true;
            this.rdoAudioBoth.Text = "both";
            this.rdoAudioBoth.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(345, 274);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("微软雅黑", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.Text = "Form1";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton rdoFlac;
        private System.Windows.Forms.RadioButton rdoM4a;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton rdo264;
        private System.Windows.Forms.RadioButton rdoHevc;
        private System.Windows.Forms.RadioButton rdoAudioBoth;
    }
}