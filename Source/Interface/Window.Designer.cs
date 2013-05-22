namespace AbsoluteZero {
    partial class Window {
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
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveOuputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.savePGNToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.copyFENToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enterFENToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.offerDrawToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.restartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoMoveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.engineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.depthToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hashSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rotateBoardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.animationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.BackColor = System.Drawing.SystemColors.ControlText;
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.gameToolStripMenuItem,
            this.engineToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(384, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.savePGNToolStripMenuItem,
            this.saveOuputToolStripMenuItem,
            this.toolStripSeparator2,
            this.enterFENToolStripMenuItem,
            this.copyFENToolStripMenuItem});
            this.fileToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveOuputToolStripMenuItem
            // 
            this.saveOuputToolStripMenuItem.Name = "saveOuputToolStripMenuItem";
            this.saveOuputToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveOuputToolStripMenuItem.Text = "Save Output";
            this.saveOuputToolStripMenuItem.Click += new System.EventHandler(this.SaveOutputClick);
            // 
            // savePGNToolStripMenuItem
            // 
            this.savePGNToolStripMenuItem.Name = "savePGNToolStripMenuItem";
            this.savePGNToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.savePGNToolStripMenuItem.Text = "Save PGN";
            this.savePGNToolStripMenuItem.Click += new System.EventHandler(this.SavePGNClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(149, 6);
            // 
            // copyFENToolStripMenuItem
            // 
            this.copyFENToolStripMenuItem.Name = "copyFENToolStripMenuItem";
            this.copyFENToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.copyFENToolStripMenuItem.Text = "Copy FEN";
            this.copyFENToolStripMenuItem.Click += new System.EventHandler(this.CopyFENClick);
            // 
            // enterFENToolStripMenuItem
            // 
            this.enterFENToolStripMenuItem.Name = "enterFENToolStripMenuItem";
            this.enterFENToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.enterFENToolStripMenuItem.Text = "Enter FEN";
            this.enterFENToolStripMenuItem.Click += new System.EventHandler(this.EnterFENClick);
            // 
            // gameToolStripMenuItem
            // 
            this.gameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.offerDrawToolStripMenuItem,
            this.toolStripSeparator3,
            this.restartToolStripMenuItem,
            this.undoMoveToolStripMenuItem});
            this.gameToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.gameToolStripMenuItem.Name = "gameToolStripMenuItem";
            this.gameToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.gameToolStripMenuItem.Text = "Game";
            // 
            // offerDrawToolStripMenuItem
            // 
            this.offerDrawToolStripMenuItem.Name = "offerDrawToolStripMenuItem";
            this.offerDrawToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.offerDrawToolStripMenuItem.Text = "Offer Draw";
            this.offerDrawToolStripMenuItem.Click += new System.EventHandler(this.OfferDrawClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(133, 6);
            // 
            // restartToolStripMenuItem
            // 
            this.restartToolStripMenuItem.Name = "restartToolStripMenuItem";
            this.restartToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.restartToolStripMenuItem.Text = "Restart";
            this.restartToolStripMenuItem.Click += new System.EventHandler(this.RestartClick);
            // 
            // undoMoveToolStripMenuItem
            // 
            this.undoMoveToolStripMenuItem.Name = "undoMoveToolStripMenuItem";
            this.undoMoveToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.undoMoveToolStripMenuItem.Text = "Undo Move";
            this.undoMoveToolStripMenuItem.Click += new System.EventHandler(this.UndoMoveClick);
            // 
            // engineToolStripMenuItem
            // 
            this.engineToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.searchToolStripMenuItem,
            this.hashSizeToolStripMenuItem});
            this.engineToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.engineToolStripMenuItem.Name = "engineToolStripMenuItem";
            this.engineToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            this.engineToolStripMenuItem.Text = "Engine";
            // 
            // searchToolStripMenuItem
            // 
            this.searchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.timeToolStripMenuItem,
            this.depthToolStripMenuItem,
            this.nodesToolStripMenuItem});
            this.searchToolStripMenuItem.Name = "searchToolStripMenuItem";
            this.searchToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.searchToolStripMenuItem.Text = "Search";
            // 
            // timeToolStripMenuItem
            // 
            this.timeToolStripMenuItem.Name = "timeToolStripMenuItem";
            this.timeToolStripMenuItem.Size = new System.Drawing.Size(108, 22);
            this.timeToolStripMenuItem.Text = "Time";
            this.timeToolStripMenuItem.Click += new System.EventHandler(this.SearchTimeClick);
            // 
            // depthToolStripMenuItem
            // 
            this.depthToolStripMenuItem.Name = "depthToolStripMenuItem";
            this.depthToolStripMenuItem.Size = new System.Drawing.Size(108, 22);
            this.depthToolStripMenuItem.Text = "Depth";
            this.depthToolStripMenuItem.Click += new System.EventHandler(this.SearchDepthClick);
            // 
            // nodesToolStripMenuItem
            // 
            this.nodesToolStripMenuItem.Name = "nodesToolStripMenuItem";
            this.nodesToolStripMenuItem.Size = new System.Drawing.Size(108, 22);
            this.nodesToolStripMenuItem.Text = "Nodes";
            this.nodesToolStripMenuItem.Click += new System.EventHandler(this.SearchNodesClick);
            // 
            // hashSizeToolStripMenuItem
            // 
            this.hashSizeToolStripMenuItem.Name = "hashSizeToolStripMenuItem";
            this.hashSizeToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.hashSizeToolStripMenuItem.Text = "Hash Size";
            this.hashSizeToolStripMenuItem.Click += new System.EventHandler(this.HashSizeClick);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rotateBoardToolStripMenuItem,
            this.animationsToolStripMenuItem});
            this.optionsToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.optionsToolStripMenuItem.Text = "Display";
            // 
            // rotateBoardToolStripMenuItem
            // 
            this.rotateBoardToolStripMenuItem.Name = "rotateBoardToolStripMenuItem";
            this.rotateBoardToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.rotateBoardToolStripMenuItem.Text = "Rotate Board";
            this.rotateBoardToolStripMenuItem.Click += new System.EventHandler(this.RotateBoardClick);
            // 
            // animationsToolStripMenuItem
            // 
            this.animationsToolStripMenuItem.Name = "animationsToolStripMenuItem";
            this.animationsToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.animationsToolStripMenuItem.Text = "Animations";
            this.animationsToolStripMenuItem.Click += new System.EventHandler(this.AnimationsClick);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutClick);
            // 
            // Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 386);
            this.Controls.Add(this.menuStrip);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip;
            this.MaximizeBox = false;
            this.Name = "Window";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Absolute Zero";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem savePGNToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveOuputToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem animationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rotateBoardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restartToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem engineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyFENToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enterFENToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem undoMoveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem offerDrawToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem timeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem depthToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nodesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hashSizeToolStripMenuItem;
    }
}