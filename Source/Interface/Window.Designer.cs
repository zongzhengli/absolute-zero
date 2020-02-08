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
            this.fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.savePGNMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveOuputMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.enterFENMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyFENMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.offerDrawMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.restartMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoMoveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.engineMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.searchTimeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchDepthMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchNodesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hashSizeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.multiPVMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.rotateBoardMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.animationsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.arrowsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.BackColor = System.Drawing.SystemColors.ControlText;
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenu,
            this.gameMenu,
            this.engineMenu,
            this.displayMenu,
            this.aboutMenu});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(384, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileMenu
            // 
            this.fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.savePGNMenuItem,
            this.saveOuputMenuItem,
            this.toolStripSeparator2,
            this.enterFENMenuItem,
            this.copyFENMenuItem});
            this.fileMenu.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.fileMenu.Name = "fileMenu";
            this.fileMenu.Size = new System.Drawing.Size(37, 20);
            this.fileMenu.Text = "File";
            // 
            // savePGNMenuItem
            // 
            this.savePGNMenuItem.Name = "savePGNMenuItem";
            this.savePGNMenuItem.Size = new System.Drawing.Size(139, 22);
            this.savePGNMenuItem.Text = "Save PGN";
            this.savePGNMenuItem.Click += new System.EventHandler(this.SavePGNClick);
            // 
            // saveOuputMenuItem
            // 
            this.saveOuputMenuItem.Name = "saveOuputMenuItem";
            this.saveOuputMenuItem.Size = new System.Drawing.Size(139, 22);
            this.saveOuputMenuItem.Text = "Save Output";
            this.saveOuputMenuItem.Click += new System.EventHandler(this.SaveOutputClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(136, 6);
            // 
            // enterFENMenuItem
            // 
            this.enterFENMenuItem.Name = "enterFENMenuItem";
            this.enterFENMenuItem.Size = new System.Drawing.Size(139, 22);
            this.enterFENMenuItem.Text = "Enter FEN";
            this.enterFENMenuItem.Click += new System.EventHandler(this.EnterFENClick);
            // 
            // copyFENMenuItem
            // 
            this.copyFENMenuItem.Name = "copyFENMenuItem";
            this.copyFENMenuItem.Size = new System.Drawing.Size(139, 22);
            this.copyFENMenuItem.Text = "Copy FEN";
            this.copyFENMenuItem.Click += new System.EventHandler(this.CopyFENClick);
            // 
            // gameMenu
            // 
            this.gameMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.offerDrawMenuItem,
            this.toolStripSeparator3,
            this.restartMenuItem,
            this.undoMoveMenuItem});
            this.gameMenu.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.gameMenu.Name = "gameMenu";
            this.gameMenu.Size = new System.Drawing.Size(50, 20);
            this.gameMenu.Text = "Game";
            // 
            // offerDrawMenuItem
            // 
            this.offerDrawMenuItem.Name = "offerDrawMenuItem";
            this.offerDrawMenuItem.Size = new System.Drawing.Size(136, 22);
            this.offerDrawMenuItem.Text = "Offer Draw";
            this.offerDrawMenuItem.Click += new System.EventHandler(this.OfferDrawClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(133, 6);
            // 
            // restartMenuItem
            // 
            this.restartMenuItem.Name = "restartMenuItem";
            this.restartMenuItem.Size = new System.Drawing.Size(136, 22);
            this.restartMenuItem.Text = "Restart";
            this.restartMenuItem.Click += new System.EventHandler(this.RestartClick);
            // 
            // undoMoveMenuItem
            // 
            this.undoMoveMenuItem.Name = "undoMoveMenuItem";
            this.undoMoveMenuItem.Size = new System.Drawing.Size(136, 22);
            this.undoMoveMenuItem.Text = "Undo Move";
            this.undoMoveMenuItem.Click += new System.EventHandler(this.UndoMoveClick);
            // 
            // engineMenu
            // 
            this.engineMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.searchTimeMenuItem,
            this.searchDepthMenuItem,
            this.searchNodesMenuItem,
            this.hashSizeMenuItem,
            this.multiPVMenuItem});
            this.engineMenu.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.engineMenu.Name = "engineMenu";
            this.engineMenu.Size = new System.Drawing.Size(55, 20);
            this.engineMenu.Text = "Engine";
            // 
            // searchTimeMenuItem
            // 
            this.searchTimeMenuItem.Name = "searchTimeMenuItem";
            this.searchTimeMenuItem.Size = new System.Drawing.Size(180, 22);
            this.searchTimeMenuItem.Text = "Time";
            this.searchTimeMenuItem.Click += new System.EventHandler(this.SearchTimeClick);
            // 
            // searchDepthMenuItem
            // 
            this.searchDepthMenuItem.Name = "searchDepthMenuItem";
            this.searchDepthMenuItem.Size = new System.Drawing.Size(180, 22);
            this.searchDepthMenuItem.Text = "Depth";
            this.searchDepthMenuItem.Click += new System.EventHandler(this.SearchDepthClick);
            // 
            // searchNodesMenuItem
            // 
            this.searchNodesMenuItem.Name = "searchNodesMenuItem";
            this.searchNodesMenuItem.Size = new System.Drawing.Size(180, 22);
            this.searchNodesMenuItem.Text = "Nodes";
            this.searchNodesMenuItem.Click += new System.EventHandler(this.SearchNodesClick);
            // 
            // hashSizeMenuItem
            // 
            this.hashSizeMenuItem.Name = "hashSizeMenuItem";
            this.hashSizeMenuItem.Size = new System.Drawing.Size(180, 22);
            this.hashSizeMenuItem.Text = "Hash Size";
            this.hashSizeMenuItem.Click += new System.EventHandler(this.HashSizeClick);
            // 
            // multiPVMenuItem
            // 
            this.multiPVMenuItem.Name = "multiPVMenuItem";
            this.multiPVMenuItem.Size = new System.Drawing.Size(180, 22);
            this.multiPVMenuItem.Text = "Multi PV";
            this.multiPVMenuItem.Click += new System.EventHandler(this.MultiPVClick);
            // 
            // displayMenu
            // 
            this.displayMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rotateBoardMenuItem,
            this.animationsMenuItem,
            this.arrowsMenuItem});
            this.displayMenu.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.displayMenu.Name = "displayMenu";
            this.displayMenu.Size = new System.Drawing.Size(57, 20);
            this.displayMenu.Text = "Display";
            // 
            // rotateBoardMenuItem
            // 
            this.rotateBoardMenuItem.Name = "rotateBoardMenuItem";
            this.rotateBoardMenuItem.Size = new System.Drawing.Size(142, 22);
            this.rotateBoardMenuItem.Text = "Rotate Board";
            this.rotateBoardMenuItem.Click += new System.EventHandler(this.RotateBoardClick);
            // 
            // animationsMenuItem
            // 
            this.animationsMenuItem.Name = "animationsMenuItem";
            this.animationsMenuItem.Size = new System.Drawing.Size(142, 22);
            this.animationsMenuItem.Text = "Animations";
            this.animationsMenuItem.Click += new System.EventHandler(this.AnimationsClick);
            // 
            // arrowsMenuItem
            // 
            this.arrowsMenuItem.Name = "arrowsMenuItem";
            this.arrowsMenuItem.Size = new System.Drawing.Size(142, 22);
            this.arrowsMenuItem.Text = "Arrows";
            this.arrowsMenuItem.Click += new System.EventHandler(this.ArrowsClick);
            // 
            // aboutMenu
            // 
            this.aboutMenu.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.aboutMenu.Name = "aboutMenu";
            this.aboutMenu.Size = new System.Drawing.Size(52, 20);
            this.aboutMenu.Text = "About";
            this.aboutMenu.Click += new System.EventHandler(this.AboutClick);
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
        private System.Windows.Forms.ToolStripMenuItem fileMenu;
        private System.Windows.Forms.ToolStripMenuItem savePGNMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayMenu;
        private System.Windows.Forms.ToolStripMenuItem saveOuputMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutMenu;
        private System.Windows.Forms.ToolStripMenuItem animationsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rotateBoardMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gameMenu;
        private System.Windows.Forms.ToolStripMenuItem restartMenuItem;
        private System.Windows.Forms.ToolStripMenuItem engineMenu;
        private System.Windows.Forms.ToolStripMenuItem copyFENMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enterFENMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem undoMoveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem offerDrawMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem hashSizeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem arrowsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchTimeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchDepthMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchNodesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem multiPVMenuItem;
    }
}