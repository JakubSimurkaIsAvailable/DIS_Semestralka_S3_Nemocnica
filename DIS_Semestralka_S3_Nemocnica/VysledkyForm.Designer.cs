namespace DIS_Semestralka_S3_Nemocnica
{
    partial class VysledkyForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            tabControl = new System.Windows.Forms.TabControl();
            SuspendLayout();

            // tabControl
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;

            // Form
            Controls.Add(tabControl);
            ClientSize = new System.Drawing.Size(1100, 720);
            MinimumSize = new System.Drawing.Size(800, 500);
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Výsledky simulácie";

            ResumeLayout(false);
        }

        private System.Windows.Forms.TabControl tabControl;
    }
}
