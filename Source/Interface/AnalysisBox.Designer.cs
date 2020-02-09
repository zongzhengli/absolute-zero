namespace AbsoluteZero {
    partial class AnalysisBox {
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
            this.searchButton = new System.Windows.Forms.Button();
            this.trackBar = new System.Windows.Forms.TrackBar();
            this.whitePanel = new System.Windows.Forms.GroupBox();
            this.blackRadio = new System.Windows.Forms.RadioButton();
            this.whiteRadio = new System.Windows.Forms.RadioButton();
            this.clearBoardButton = new System.Windows.Forms.Button();
            this.fenTextBox = new System.Windows.Forms.TextBox();
            this.pvsTextBox = new System.Windows.Forms.TextBox();
            this.pvsLabel = new System.Windows.Forms.Label();
            this.nextButton = new System.Windows.Forms.Button();
            this.resetBoardButton = new System.Windows.Forms.Button();
            this.previousButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar)).BeginInit();
            this.whitePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // searchButton
            // 
            this.searchButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.searchButton.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchButton.Location = new System.Drawing.Point(369, 80);
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(94, 27);
            this.searchButton.TabIndex = 1;
            this.searchButton.Text = "Search";
            this.searchButton.UseVisualStyleBackColor = true;
            this.searchButton.Click += new System.EventHandler(this.SearchClick);
            // 
            // trackBar
            // 
            this.trackBar.Location = new System.Drawing.Point(582, 42);
            this.trackBar.Maximum = 7;
            this.trackBar.Name = "trackBar";
            this.trackBar.Size = new System.Drawing.Size(189, 45);
            this.trackBar.TabIndex = 2;
            this.trackBar.Value = 4;
            this.trackBar.Scroll += new System.EventHandler(this.TrackBarScroll);
            // 
            // whitePanel
            // 
            this.whitePanel.Controls.Add(this.blackRadio);
            this.whitePanel.Controls.Add(this.whiteRadio);
            this.whitePanel.Location = new System.Drawing.Point(370, 34);
            this.whitePanel.Name = "whitePanel";
            this.whitePanel.Padding = new System.Windows.Forms.Padding(0);
            this.whitePanel.Size = new System.Drawing.Size(138, 39);
            this.whitePanel.TabIndex = 5;
            this.whitePanel.TabStop = false;
            // 
            // blackRadio
            // 
            this.blackRadio.AutoSize = true;
            this.blackRadio.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.blackRadio.Location = new System.Drawing.Point(73, 14);
            this.blackRadio.Name = "blackRadio";
            this.blackRadio.Size = new System.Drawing.Size(60, 19);
            this.blackRadio.TabIndex = 1;
            this.blackRadio.Text = "Black";
            this.blackRadio.UseVisualStyleBackColor = true;
            this.blackRadio.CheckedChanged += new System.EventHandler(this.BlackRadioChecked);
            // 
            // whiteRadio
            // 
            this.whiteRadio.AutoSize = true;
            this.whiteRadio.Checked = true;
            this.whiteRadio.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.whiteRadio.Location = new System.Drawing.Point(10, 14);
            this.whiteRadio.Name = "whiteRadio";
            this.whiteRadio.Size = new System.Drawing.Size(60, 19);
            this.whiteRadio.TabIndex = 0;
            this.whiteRadio.TabStop = true;
            this.whiteRadio.Text = "White";
            this.whiteRadio.UseVisualStyleBackColor = true;
            this.whiteRadio.CheckedChanged += new System.EventHandler(this.WhiteRadioChecked);
            // 
            // clearBoardButton
            // 
            this.clearBoardButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.clearBoardButton.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clearBoardButton.Location = new System.Drawing.Point(671, 80);
            this.clearBoardButton.Name = "clearBoardButton";
            this.clearBoardButton.Size = new System.Drawing.Size(94, 27);
            this.clearBoardButton.TabIndex = 6;
            this.clearBoardButton.Text = "Clear Board";
            this.clearBoardButton.UseVisualStyleBackColor = true;
            this.clearBoardButton.Click += new System.EventHandler(this.ClearBoardClick);
            // 
            // fenTextBox
            // 
            this.fenTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fenTextBox.Location = new System.Drawing.Point(370, 11);
            this.fenTextBox.Name = "fenTextBox";
            this.fenTextBox.Size = new System.Drawing.Size(394, 20);
            this.fenTextBox.TabIndex = 7;
            this.fenTextBox.Text = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            this.fenTextBox.TextChanged += new System.EventHandler(this.FenTextBoxChanged);
            // 
            // pvsTextBox
            // 
            this.pvsTextBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pvsTextBox.Location = new System.Drawing.Point(546, 45);
            this.pvsTextBox.Name = "pvsTextBox";
            this.pvsTextBox.ReadOnly = true;
            this.pvsTextBox.Size = new System.Drawing.Size(34, 23);
            this.pvsTextBox.TabIndex = 4;
            this.pvsTextBox.Text = "16";
            this.pvsTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // pvsLabel
            // 
            this.pvsLabel.AutoSize = true;
            this.pvsLabel.Location = new System.Drawing.Point(514, 49);
            this.pvsLabel.Name = "pvsLabel";
            this.pvsLabel.Size = new System.Drawing.Size(28, 15);
            this.pvsLabel.TabIndex = 8;
            this.pvsLabel.Text = "PVs";
            // 
            // nextButton
            // 
            this.nextButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.nextButton.Enabled = false;
            this.nextButton.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nextButton.Location = new System.Drawing.Point(520, 80);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(45, 27);
            this.nextButton.TabIndex = 9;
            this.nextButton.Text = ">>";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.NextClick);
            // 
            // resetBoardButton
            // 
            this.resetBoardButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.resetBoardButton.Enabled = false;
            this.resetBoardButton.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resetBoardButton.Location = new System.Drawing.Point(571, 80);
            this.resetBoardButton.Name = "resetBoardButton";
            this.resetBoardButton.Size = new System.Drawing.Size(94, 27);
            this.resetBoardButton.TabIndex = 10;
            this.resetBoardButton.Text = "Reset Board";
            this.resetBoardButton.UseVisualStyleBackColor = true;
            this.resetBoardButton.Click += new System.EventHandler(this.ResetBoardClick);
            // 
            // previousButton
            // 
            this.previousButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.previousButton.Enabled = false;
            this.previousButton.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.previousButton.Location = new System.Drawing.Point(469, 80);
            this.previousButton.Name = "previousButton";
            this.previousButton.Size = new System.Drawing.Size(45, 27);
            this.previousButton.TabIndex = 11;
            this.previousButton.Text = "<<";
            this.previousButton.UseVisualStyleBackColor = true;
            this.previousButton.Click += new System.EventHandler(this.PreviousClick);
            // 
            // AnalysisBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(774, 117);
            this.Controls.Add(this.previousButton);
            this.Controls.Add(this.resetBoardButton);
            this.Controls.Add(this.nextButton);
            this.Controls.Add(this.pvsLabel);
            this.Controls.Add(this.fenTextBox);
            this.Controls.Add(this.clearBoardButton);
            this.Controls.Add(this.pvsTextBox);
            this.Controls.Add(this.searchButton);
            this.Controls.Add(this.trackBar);
            this.Controls.Add(this.whitePanel);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AnalysisBox";
            this.ShowIcon = false;
            this.Text = "Control Panel";
            ((System.ComponentModel.ISupportInitialize)(this.trackBar)).EndInit();
            this.whitePanel.ResumeLayout(false);
            this.whitePanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.TrackBar trackBar;
        private System.Windows.Forms.GroupBox whitePanel;
        private System.Windows.Forms.RadioButton blackRadio;
        private System.Windows.Forms.RadioButton whiteRadio;
        private System.Windows.Forms.Button clearBoardButton;
        private System.Windows.Forms.TextBox fenTextBox;
        private System.Windows.Forms.TextBox pvsTextBox;
        private System.Windows.Forms.Label pvsLabel;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Button resetBoardButton;
        private System.Windows.Forms.Button previousButton;
    }
}