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
            this.rdoAudioBoth = new System.Windows.Forms.RadioButton();
            this.button1 = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.rdo264 = new System.Windows.Forms.RadioButton();
            this.rdoHevc = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.rdoMkv = new System.Windows.Forms.RadioButton();
            this.rdoMp4 = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFps = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.chkChapter = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtAudioLanguage = new System.Windows.Forms.TextBox();
            this.chkSubtitle = new System.Windows.Forms.CheckBox();
            this.txtSubtitleLanguage = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel3.SuspendLayout();
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
            this.rdoM4a.Location = new System.Drawing.Point(4, 39);
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
            this.panel1.Location = new System.Drawing.Point(7, 48);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(69, 107);
            this.panel1.TabIndex = 2;
            // 
            // rdoAudioBoth
            // 
            this.rdoAudioBoth.AutoSize = true;
            this.rdoAudioBoth.Location = new System.Drawing.Point(4, 73);
            this.rdoAudioBoth.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoAudioBoth.Name = "rdoAudioBoth";
            this.rdoAudioBoth.Size = new System.Drawing.Size(57, 24);
            this.rdoAudioBoth.TabIndex = 2;
            this.rdoAudioBoth.TabStop = true;
            this.rdoAudioBoth.Text = "全部";
            this.rdoAudioBoth.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 242);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(418, 68);
            this.button1.TabIndex = 3;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.rdo264);
            this.panel2.Controls.Add(this.rdoHevc);
            this.panel2.Location = new System.Drawing.Point(84, 48);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(66, 74);
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
            this.rdoHevc.Location = new System.Drawing.Point(4, 39);
            this.rdoHevc.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rdoHevc.Name = "rdoHevc";
            this.rdoHevc.Size = new System.Drawing.Size(61, 24);
            this.rdoHevc.TabIndex = 1;
            this.rdoHevc.TabStop = true;
            this.rdoHevc.Text = "hevc";
            this.rdoHevc.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtSubtitleLanguage);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.chkSubtitle);
            this.groupBox1.Controls.Add(this.txtAudioLanguage);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.chkChapter);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.panel2);
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(294, 222);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "输入";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.txtFps);
            this.groupBox2.Controls.Add(this.panel3);
            this.groupBox2.Location = new System.Drawing.Point(312, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(118, 222);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "输出";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.rdoMp4);
            this.panel3.Controls.Add(this.rdoMkv);
            this.panel3.Location = new System.Drawing.Point(6, 27);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(73, 69);
            this.panel3.TabIndex = 0;
            // 
            // rdoMkv
            // 
            this.rdoMkv.AutoSize = true;
            this.rdoMkv.Checked = true;
            this.rdoMkv.Location = new System.Drawing.Point(3, 5);
            this.rdoMkv.Name = "rdoMkv";
            this.rdoMkv.Size = new System.Drawing.Size(57, 24);
            this.rdoMkv.TabIndex = 0;
            this.rdoMkv.TabStop = true;
            this.rdoMkv.Text = "mkv";
            this.rdoMkv.UseVisualStyleBackColor = true;
            // 
            // rdoMp4
            // 
            this.rdoMp4.AutoSize = true;
            this.rdoMp4.Location = new System.Drawing.Point(3, 39);
            this.rdoMp4.Name = "rdoMp4";
            this.rdoMp4.Size = new System.Drawing.Size(60, 24);
            this.rdoMp4.TabIndex = 1;
            this.rdoMp4.Text = "mp4";
            this.rdoMp4.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 99);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 20);
            this.label1.TabIndex = 4;
            this.label1.Text = "FPS";
            // 
            // txtFps
            // 
            this.txtFps.Location = new System.Drawing.Point(6, 122);
            this.txtFps.Name = "txtFps";
            this.txtFps.Size = new System.Drawing.Size(106, 27);
            this.txtFps.TabIndex = 5;
            this.txtFps.Text = "24000/1001";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "音频";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(84, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "视频";
            // 
            // chkChapter
            // 
            this.chkChapter.AutoSize = true;
            this.chkChapter.Checked = true;
            this.chkChapter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkChapter.Location = new System.Drawing.Point(157, 22);
            this.chkChapter.Name = "chkChapter";
            this.chkChapter.Size = new System.Drawing.Size(58, 24);
            this.chkChapter.TabIndex = 6;
            this.chkChapter.Text = "章节";
            this.chkChapter.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 160);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 20);
            this.label4.TabIndex = 7;
            this.label4.Text = "语言";
            // 
            // txtAudioLanguage
            // 
            this.txtAudioLanguage.Location = new System.Drawing.Point(11, 183);
            this.txtAudioLanguage.Name = "txtAudioLanguage";
            this.txtAudioLanguage.Size = new System.Drawing.Size(52, 27);
            this.txtAudioLanguage.TabIndex = 8;
            this.txtAudioLanguage.Text = "jpn";
            // 
            // chkSubtitle
            // 
            this.chkSubtitle.AutoSize = true;
            this.chkSubtitle.Checked = true;
            this.chkSubtitle.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSubtitle.Location = new System.Drawing.Point(221, 22);
            this.chkSubtitle.Name = "chkSubtitle";
            this.chkSubtitle.Size = new System.Drawing.Size(58, 24);
            this.chkSubtitle.TabIndex = 9;
            this.chkSubtitle.Text = "字幕";
            this.chkSubtitle.UseVisualStyleBackColor = true;
            // 
            // txtSubtitleLanguage
            // 
            this.txtSubtitleLanguage.Location = new System.Drawing.Point(221, 72);
            this.txtSubtitleLanguage.Name = "txtSubtitleLanguage";
            this.txtSubtitleLanguage.Size = new System.Drawing.Size(58, 27);
            this.txtSubtitleLanguage.TabIndex = 11;
            this.txtSubtitleLanguage.Text = "jpn";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(221, 49);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(39, 20);
            this.label5.TabIndex = 10;
            this.label5.Text = "语言";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(443, 319);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button1);
            this.Font = new System.Drawing.Font("微软雅黑", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Form1";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
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
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.RadioButton rdoMp4;
        private System.Windows.Forms.RadioButton rdoMkv;
        private System.Windows.Forms.TextBox txtFps;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSubtitleLanguage;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chkSubtitle;
        private System.Windows.Forms.TextBox txtAudioLanguage;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkChapter;
    }
}