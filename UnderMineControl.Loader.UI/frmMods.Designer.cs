namespace UnderMineControl.Loader.UI
{
    partial class frmMods
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ocMods = new UnderMineControl.Loader.UI.Controls.OrderedContainer();
            this.menuMain = new System.Windows.Forms.MenuStrip();
            this.SuspendLayout();
            // 
            // ocMods
            // 
            this.ocMods.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ocMods.AutoScroll = true;
            this.ocMods.Location = new System.Drawing.Point(1, 25);
            this.ocMods.Margin = new System.Windows.Forms.Padding(0);
            this.ocMods.Name = "ocMods";
            this.ocMods.ScrollToBottom = false;
            this.ocMods.Size = new System.Drawing.Size(290, 424);
            this.ocMods.TabIndex = 0;
            // 
            // menuMain
            // 
            this.menuMain.Location = new System.Drawing.Point(0, 0);
            this.menuMain.Name = "menuMain";
            this.menuMain.Size = new System.Drawing.Size(800, 24);
            this.menuMain.TabIndex = 1;
            this.menuMain.Text = "menuStrip1";
            // 
            // frmMods
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ocMods);
            this.Controls.Add(this.menuMain);
            this.MainMenuStrip = this.menuMain;
            this.Name = "frmMods";
            this.Text = "UnderMineControl";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.OrderedContainer ocMods;
        private System.Windows.Forms.MenuStrip menuMain;
    }
}

