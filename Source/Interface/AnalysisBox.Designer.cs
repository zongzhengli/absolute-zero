﻿namespace AbsoluteZero {
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
            this.clearBoard = new System.Windows.Forms.Button();
            this.fenTextBox = new System.Windows.Forms.TextBox();
            this.pvsTextBox = new System.Windows.Forms.TextBox();
            this.pvsLabel = new System.Windows.Forms.Label();
            this.playPVButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar)).BeginInit();
            this.whitePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // searchButton
            // 
            this.searchButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.searchButton.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchButton.Location = new System.Drawing.Point(461, 80);
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(92, 27);
            this.searchButton.TabIndex = 1;
            this.searchButton.Text = "Search";
            this.searchButton.UseVisualStyleBackColor = true;
            this.searchButton.Click += new System.EventHandler(this.SearchClick);
            // 
            // trackBar
            // 
            this.trackBar.Location = new System.Drawing.Point(531, 42);
            this.trackBar.Maximum = 7;
            this.trackBar.Name = "trackBar";
            this.trackBar.Size = new System.Drawing.Size(236, 45);
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
            this.whitePanel.Size = new System.Drawing.Size(78, 74);
            this.whitePanel.TabIndex = 5;
            this.whitePanel.TabStop = false;
            // 
            // blackRadio
            // 
            this.blackRadio.AutoSize = true;
            this.blackRadio.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.blackRadio.Location = new System.Drawing.Point(10, 45);
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
            this.whiteRadio.Location = new System.Drawing.Point(10, 17);
            this.whiteRadio.Name = "whiteRadio";
            this.whiteRadio.Size = new System.Drawing.Size(60, 19);
            this.whiteRadio.TabIndex = 0;
            this.whiteRadio.TabStop = true;
            this.whiteRadio.Text = "White";
            this.whiteRadio.UseVisualStyleBackColor = true;
            this.whiteRadio.CheckedChanged += new System.EventHandler(this.WhiteRadioChecked);
            // 
            // clearBoard
            // 
            this.clearBoard.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.clearBoard.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clearBoard.Location = new System.Drawing.Point(665, 80);
            this.clearBoard.Name = "clearBoard";
            this.clearBoard.Size = new System.Drawing.Size(96, 27);
            this.clearBoard.TabIndex = 6;
            this.clearBoard.Text = "Clear Board";
            this.clearBoard.UseVisualStyleBackColor = true;
            this.clearBoard.Click += new System.EventHandler(this.ClearBoardClick);
            // 
            // fenTextBox
            // 
            this.fenTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fenTextBox.Location = new System.Drawing.Point(370, 11);
            this.fenTextBox.Name = "fenTextBox";
            this.fenTextBox.Size = new System.Drawing.Size(392, 20);
            this.fenTextBox.TabIndex = 7;
            this.fenTextBox.Text = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            this.fenTextBox.TextChanged += new System.EventHandler(this.FenTextBoxChanged);
            // 
            // pvsTextBox
            // 
            this.pvsTextBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pvsTextBox.Location = new System.Drawing.Point(491, 45);
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
            this.pvsLabel.Location = new System.Drawing.Point(458, 49);
            this.pvsLabel.Name = "pvsLabel";
            this.pvsLabel.Size = new System.Drawing.Size(28, 15);
            this.pvsLabel.TabIndex = 8;
            this.pvsLabel.Text = "PVs";
            // 
            // playPVButton
            // 
            this.playPVButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.playPVButton.Enabled = false;
            this.playPVButton.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playPVButton.Location = new System.Drawing.Point(561, 80);
            this.playPVButton.Name = "playPVButton";
            this.playPVButton.Size = new System.Drawing.Size(96, 27);
            this.playPVButton.TabIndex = 9;
            this.playPVButton.Text = "Play PV";
            this.playPVButton.UseVisualStyleBackColor = true;
            this.playPVButton.Click += new System.EventHandler(this.PlayPVClick);
            // 
            // AnalysisBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(771, 117);
            this.Controls.Add(this.playPVButton);
            this.Controls.Add(this.pvsLabel);
            this.Controls.Add(this.fenTextBox);
            this.Controls.Add(this.clearBoard);
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
        private System.Windows.Forms.Button clearBoard;
        private System.Windows.Forms.TextBox fenTextBox;
        private System.Windows.Forms.TextBox pvsTextBox;
        private System.Windows.Forms.Label pvsLabel;
        private System.Windows.Forms.Button playPVButton;
    }
}