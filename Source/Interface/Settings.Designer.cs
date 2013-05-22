namespace AbsoluteZero {
    partial class Settings {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
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
        private void InitializeComponent() {
            this.start = new System.Windows.Forms.Button();
            this.whitePanel = new System.Windows.Forms.GroupBox();
            this.whiteComputer = new System.Windows.Forms.RadioButton();
            this.whiteHuman = new System.Windows.Forms.RadioButton();
            this.blackPanel = new System.Windows.Forms.GroupBox();
            this.blackComputer = new System.Windows.Forms.RadioButton();
            this.blackHuman = new System.Windows.Forms.RadioButton();
            this.whitePanel.SuspendLayout();
            this.blackPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // start
            // 
            this.start.Location = new System.Drawing.Point(9, 79);
            this.start.Name = "start";
            this.start.Size = new System.Drawing.Size(200, 27);
            this.start.TabIndex = 0;
            this.start.Text = "Start";
            this.start.UseVisualStyleBackColor = true;
            this.start.Click += new System.EventHandler(this.StartClick);
            // 
            // whitePanel
            // 
            this.whitePanel.Controls.Add(this.whiteComputer);
            this.whitePanel.Controls.Add(this.whiteHuman);
            this.whitePanel.Location = new System.Drawing.Point(9, 8);
            this.whitePanel.Name = "whitePanel";
            this.whitePanel.Size = new System.Drawing.Size(97, 65);
            this.whitePanel.TabIndex = 2;
            this.whitePanel.TabStop = false;
            this.whitePanel.Text = "White";
            // 
            // whiteComputer
            // 
            this.whiteComputer.AutoSize = true;
            this.whiteComputer.Location = new System.Drawing.Point(10, 40);
            this.whiteComputer.Name = "whiteComputer";
            this.whiteComputer.Size = new System.Drawing.Size(70, 17);
            this.whiteComputer.TabIndex = 1;
            this.whiteComputer.TabStop = true;
            this.whiteComputer.Text = "Computer";
            this.whiteComputer.UseVisualStyleBackColor = true;
            // 
            // whiteHuman
            // 
            this.whiteHuman.AutoSize = true;
            this.whiteHuman.Location = new System.Drawing.Point(10, 17);
            this.whiteHuman.Name = "whiteHuman";
            this.whiteHuman.Size = new System.Drawing.Size(59, 17);
            this.whiteHuman.TabIndex = 0;
            this.whiteHuman.TabStop = true;
            this.whiteHuman.Text = "Human";
            this.whiteHuman.UseVisualStyleBackColor = true;
            // 
            // blackPanel
            // 
            this.blackPanel.Controls.Add(this.blackComputer);
            this.blackPanel.Controls.Add(this.blackHuman);
            this.blackPanel.Location = new System.Drawing.Point(112, 8);
            this.blackPanel.Name = "blackPanel";
            this.blackPanel.Size = new System.Drawing.Size(97, 65);
            this.blackPanel.TabIndex = 3;
            this.blackPanel.TabStop = false;
            this.blackPanel.Text = "Black";
            // 
            // blackComputer
            // 
            this.blackComputer.AutoSize = true;
            this.blackComputer.Location = new System.Drawing.Point(10, 40);
            this.blackComputer.Name = "blackComputer";
            this.blackComputer.Size = new System.Drawing.Size(70, 17);
            this.blackComputer.TabIndex = 1;
            this.blackComputer.TabStop = true;
            this.blackComputer.Text = "Computer";
            this.blackComputer.UseVisualStyleBackColor = true;
            // 
            // blackHuman
            // 
            this.blackHuman.AutoSize = true;
            this.blackHuman.Location = new System.Drawing.Point(10, 17);
            this.blackHuman.Name = "blackHuman";
            this.blackHuman.Size = new System.Drawing.Size(59, 17);
            this.blackHuman.TabIndex = 0;
            this.blackHuman.TabStop = true;
            this.blackHuman.Text = "Human";
            this.blackHuman.UseVisualStyleBackColor = true;
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(218, 114);
            this.Controls.Add(this.blackPanel);
            this.Controls.Add(this.whitePanel);
            this.Controls.Add(this.start);
            this.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Settings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.whitePanel.ResumeLayout(false);
            this.whitePanel.PerformLayout();
            this.blackPanel.ResumeLayout(false);
            this.blackPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button start;
        private System.Windows.Forms.GroupBox whitePanel;
        private System.Windows.Forms.RadioButton whiteComputer;
        private System.Windows.Forms.RadioButton whiteHuman;
        private System.Windows.Forms.GroupBox blackPanel;
        private System.Windows.Forms.RadioButton blackComputer;
        private System.Windows.Forms.RadioButton blackHuman;

    }
}