namespace DIS_Semestralka_S3_Nemocnica
{
    partial class KonfigForm
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
            grpPoradie = new System.Windows.Forms.GroupBox();
            rbOsetreniePred = new System.Windows.Forms.RadioButton();
            rbVVPred = new System.Windows.Forms.RadioButton();
            grpLekari = new System.Windows.Forms.GroupBox();
            cbRezervaLekarPreA = new System.Windows.Forms.CheckBox();
            cbRezervaSestraPreVV = new System.Windows.Forms.CheckBox();
            cbMinPohybPersonalu = new System.Windows.Forms.CheckBox();
            cbRadABPreferA = new System.Windows.Forms.CheckBox();
            cbPrefRadB = new System.Windows.Forms.CheckBox();
            nudPrefRadBPrah = new System.Windows.Forms.NumericUpDown();
            lblPrefRadBPrah = new System.Windows.Forms.Label();
            grpCsv = new System.Windows.Forms.GroupBox();
            cbCsvLog = new System.Windows.Forms.CheckBox();
            cbCsvFinal = new System.Windows.Forms.CheckBox();
            btnCsvFinalAppend = new System.Windows.Forms.Button();
            btnZavriet = new System.Windows.Forms.Button();

            grpPoradie.SuspendLayout();
            grpCsv.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudPrefRadBPrah).BeginInit();
            SuspendLayout();

            // grpPoradie
            grpPoradie.Controls.Add(rbOsetreniePred);
            grpPoradie.Controls.Add(rbVVPred);
            grpPoradie.Location = new System.Drawing.Point(12, 12);
            grpPoradie.Size = new System.Drawing.Size(370, 80);
            grpPoradie.Text = "Poradie obsluhy pri uvoľnení zdroja";

            // rbOsetreniePred
            rbOsetreniePred.AutoSize = true;
            rbOsetreniePred.Checked = true;
            rbOsetreniePred.Location = new System.Drawing.Point(15, 25);
            rbOsetreniePred.Text = "Ošetrenie pred VV  (predvolené)";

            // rbVVPred
            rbVVPred.AutoSize = true;
            rbVVPred.Location = new System.Drawing.Point(15, 50);
            rbVVPred.Text = "VV pred Ošetrením";

            // grpLekari
            grpLekari.Controls.Add(cbRezervaLekarPreA);
            grpLekari.Controls.Add(cbRezervaSestraPreVV);
            grpLekari.Controls.Add(cbMinPohybPersonalu);
            grpLekari.Controls.Add(cbRadABPreferA);
            grpLekari.Controls.Add(cbPrefRadB);
            grpLekari.Controls.Add(nudPrefRadBPrah);
            grpLekari.Controls.Add(lblPrefRadBPrah);
            grpLekari.Location = new System.Drawing.Point(12, 105);
            grpLekari.Size = new System.Drawing.Size(370, 192);
            grpLekari.Text = "Stratégia prideľovania zdrojov";

            // cbRezervaLekarPreA
            cbRezervaLekarPreA.AutoSize = true;
            cbRezervaLekarPreA.Location = new System.Drawing.Point(15, 22);
            cbRezervaLekarPreA.Text = "Rezervovať 1 lekára pre ambulanciu A";

            // cbRezervaSestraPreVV
            cbRezervaSestraPreVV.AutoSize = true;
            cbRezervaSestraPreVV.Location = new System.Drawing.Point(15, 50);
            cbRezervaSestraPreVV.Text = "Rezervovať 1 sestru pre vstupné vyšetrenie";

            // cbMinPohybPersonalu
            cbMinPohybPersonalu.AutoSize = true;
            cbMinPohybPersonalu.Location = new System.Drawing.Point(15, 78);
            cbMinPohybPersonalu.Text = "Minimalizovať pohyb personálu (posielať pacienta k personálu)";

            // cbRadABPreferA
            cbRadABPreferA.AutoSize = true;
            cbRadABPreferA.Location = new System.Drawing.Point(15, 108);
            cbRadABPreferA.Text = "RadAB preferuje miestnosť A  (predvolene: B)";

            // cbPrefRadB
            cbPrefRadB.AutoSize = true;
            cbPrefRadB.Location = new System.Drawing.Point(15, 136);
            cbPrefRadB.Text = "Preferovať rad B, keď jeho dĺžka presiahne:";

            // nudPrefRadBPrah
            nudPrefRadBPrah.Location = new System.Drawing.Point(285, 134);
            nudPrefRadBPrah.Size = new System.Drawing.Size(60, 23);
            nudPrefRadBPrah.Minimum = 1;
            nudPrefRadBPrah.Maximum = 999;
            nudPrefRadBPrah.Value = 5;

            // lblPrefRadBPrah
            lblPrefRadBPrah.AutoSize = true;
            lblPrefRadBPrah.Location = new System.Drawing.Point(285, 160);
            lblPrefRadBPrah.Text = "pacientov";

            // grpCsv
            grpCsv.Controls.Add(cbCsvLog);
            grpCsv.Controls.Add(cbCsvFinal);
            grpCsv.Controls.Add(btnCsvFinalAppend);
            grpCsv.Location = new System.Drawing.Point(12, 309);
            grpCsv.Size = new System.Drawing.Size(370, 100);
            grpCsv.Text = "CSV export";

            // cbCsvLog
            cbCsvLog.AutoSize = true;
            cbCsvLog.Location = new System.Drawing.Point(15, 28);
            cbCsvLog.Text = "CSV log  (spomalenie)";

            // cbCsvFinal
            cbCsvFinal.AutoSize = true;
            cbCsvFinal.Location = new System.Drawing.Point(15, 62);
            cbCsvFinal.Text = "CSV výsledky  (max speed)";

            // btnCsvFinalAppend
            btnCsvFinalAppend.Location = new System.Drawing.Point(225, 58);
            btnCsvFinalAppend.Size = new System.Drawing.Size(125, 24);
            btnCsvFinalAppend.Text = "📂 Pripojiť...";
            btnCsvFinalAppend.UseVisualStyleBackColor = true;
            btnCsvFinalAppend.Click += BtnCsvFinalAppend_Click;

            // btnZavriet
            btnZavriet.Location = new System.Drawing.Point(282, 433);
            btnZavriet.Size = new System.Drawing.Size(100, 28);
            btnZavriet.Text = "Zavrieť";
            btnZavriet.UseVisualStyleBackColor = true;
            btnZavriet.Click += BtnZavriet_Click;

            // Form
            Controls.Add(grpPoradie);
            Controls.Add(grpLekari);
            Controls.Add(grpCsv);
            Controls.Add(btnZavriet);
            ClientSize = new System.Drawing.Size(396, 477);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Konfigurácia simulácie";

            grpPoradie.ResumeLayout(false);
            grpPoradie.PerformLayout();
            grpCsv.ResumeLayout(false);
            grpCsv.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudPrefRadBPrah).EndInit();
            ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox grpPoradie;
        private System.Windows.Forms.RadioButton rbOsetreniePred;
        private System.Windows.Forms.RadioButton rbVVPred;
        private System.Windows.Forms.GroupBox grpLekari;
        private System.Windows.Forms.CheckBox cbRezervaLekarPreA;
        private System.Windows.Forms.CheckBox cbRezervaSestraPreVV;
        private System.Windows.Forms.CheckBox cbMinPohybPersonalu;
        private System.Windows.Forms.CheckBox cbRadABPreferA;
        private System.Windows.Forms.CheckBox cbPrefRadB;
        private System.Windows.Forms.NumericUpDown nudPrefRadBPrah;
        private System.Windows.Forms.Label lblPrefRadBPrah;
        private System.Windows.Forms.GroupBox grpCsv;
        private System.Windows.Forms.CheckBox cbCsvLog;
        private System.Windows.Forms.CheckBox cbCsvFinal;
        private System.Windows.Forms.Button btnCsvFinalAppend;
        private System.Windows.Forms.Button btnZavriet;
    }
}
