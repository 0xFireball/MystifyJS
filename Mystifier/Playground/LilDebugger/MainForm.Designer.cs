namespace LilDebugger
{
    partial class MainForm
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
            this.codeTb = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.codeTb.Location = new System.Drawing.Point(13, 13);
            this.codeTb.Name = "richTextBox1";
            this.codeTb.Size = new System.Drawing.Size(943, 631);
            this.codeTb.TabIndex = 0;
            this.codeTb.Text = "";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(13, 651);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(143, 45);
            this.button1.TabIndex = 1;
            this.button1.Text = "Debug";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(968, 708);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.codeTb);
            this.Name = "MainForm";
            this.Text = "Li\'l Debugger";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox codeTb;
        private System.Windows.Forms.Button button1;
    }
}

