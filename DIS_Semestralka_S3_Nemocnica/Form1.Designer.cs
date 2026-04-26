namespace DIS_Semestralka_S3_Nemocnica
{
    partial class Form1
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
            this.SuspendLayout();
            this.Text = "Simulácia nemocnice";
            this.ClientSize = new System.Drawing.Size(900, 650);
            this.MinimumSize = new System.Drawing.Size(700, 500);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
        }
    }
}
