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
            pnlTop = new System.Windows.Forms.FlowLayoutPanel();
            lblTrvanieLabel = new System.Windows.Forms.Label();
            nudTrvanie = new System.Windows.Forms.NumericUpDown();
            lblReplikacieLabel = new System.Windows.Forms.Label();
            nudReplikacie = new System.Windows.Forms.NumericUpDown();
            lblSeedLabel = new System.Windows.Forms.Label();
            nudSeed = new System.Windows.Forms.NumericUpDown();
            cbNahodny = new System.Windows.Forms.CheckBox();
            btnSpustit = new System.Windows.Forms.Button();
            btnZastavit = new System.Windows.Forms.Button();
            btnPauza = new System.Windows.Forms.Button();
            pnlKonfZdrojov = new System.Windows.Forms.FlowLayoutPanel();
            lblSestryKonfLabel = new System.Windows.Forms.Label();
            nudSestry = new System.Windows.Forms.NumericUpDown();
            lblLekariKonfLabel = new System.Windows.Forms.Label();
            nudLekari = new System.Windows.Forms.NumericUpDown();
            lblMiestnostiAKonfLabel = new System.Windows.Forms.Label();
            nudMiestnostiA = new System.Windows.Forms.NumericUpDown();
            lblMiestnostiBKonfLabel = new System.Windows.Forms.Label();
            nudMiestnostiB = new System.Windows.Forms.NumericUpDown();
            pnlSpeed = new System.Windows.Forms.Panel();
            lblDurationLabel = new System.Windows.Forms.Label();
            trkDuration = new System.Windows.Forms.TrackBar();
            lblDurationVal = new System.Windows.Forms.Label();
            lblIntervalLabel = new System.Windows.Forms.Label();
            trkInterval = new System.Windows.Forms.TrackBar();
            lblIntervalVal = new System.Windows.Forms.Label();
            pnlStatus = new System.Windows.Forms.Panel();
            lblSimCas = new System.Windows.Forms.Label();
            lblReplikacia = new System.Windows.Forms.Label();
            lblPocetPacientov = new System.Windows.Forms.Label();
            lblPocetVybavenych = new System.Windows.Forms.Label();
            tabControl = new System.Windows.Forms.TabControl();
            tpPacienti = new System.Windows.Forms.TabPage();
            dgvPacienti = new System.Windows.Forms.DataGridView();
            tpZdroje = new System.Windows.Forms.TabPage();
            tlpZdroje = new System.Windows.Forms.TableLayoutPanel();
            lblZdrojH = new System.Windows.Forms.Label();
            lblVolneH = new System.Windows.Forms.Label();
            lblVytazenieH = new System.Windows.Forms.Label();
            lblSestryRow = new System.Windows.Forms.Label();
            lblSestryVal = new System.Windows.Forms.Label();
            pbSestry = new System.Windows.Forms.ProgressBar();
            lblLekariRow = new System.Windows.Forms.Label();
            lblLekariVal = new System.Windows.Forms.Label();
            pbLekari = new System.Windows.Forms.ProgressBar();
            lblMiestnostiARow = new System.Windows.Forms.Label();
            lblMiestnostiAVal = new System.Windows.Forms.Label();
            pbMiestnostiA = new System.Windows.Forms.ProgressBar();
            lblMiestnostiBRow = new System.Windows.Forms.Label();
            lblMiestnostiBVal = new System.Windows.Forms.Label();
            pbMiestnostiB = new System.Windows.Forms.ProgressBar();
            tpRady = new System.Windows.Forms.TabPage();
            scRady = new System.Windows.Forms.SplitContainer();
            lblRadVVCount = new System.Windows.Forms.Label();
            lbRadVV = new System.Windows.Forms.ListBox();
            lblRadOsetrenieCount = new System.Windows.Forms.Label();
            lbRadOsetrenie = new System.Windows.Forms.ListBox();
            tpAnimator = new System.Windows.Forms.TabPage();
            pnlAnimator = new System.Windows.Forms.Panel();

            ((System.ComponentModel.ISupportInitialize)nudTrvanie).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudReplikacie).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSeed).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSestry).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudLekari).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudMiestnostiA).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudMiestnostiB).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkDuration).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkInterval).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvPacienti).BeginInit();
            pnlTop.SuspendLayout();
            pnlKonfZdrojov.SuspendLayout();
            pnlSpeed.SuspendLayout();
            pnlStatus.SuspendLayout();
            tabControl.SuspendLayout();
            tpPacienti.SuspendLayout();
            tpZdroje.SuspendLayout();
            tlpZdroje.SuspendLayout();
            tpRady.SuspendLayout();
            scRady.Panel1.SuspendLayout();
            scRady.Panel2.SuspendLayout();
            scRady.SuspendLayout();
            tpAnimator.SuspendLayout();
            SuspendLayout();

            // pnlTop
            pnlTop.Controls.Add(lblTrvanieLabel);
            pnlTop.Controls.Add(nudTrvanie);
            pnlTop.Controls.Add(lblReplikacieLabel);
            pnlTop.Controls.Add(nudReplikacie);
            pnlTop.Controls.Add(lblSeedLabel);
            pnlTop.Controls.Add(nudSeed);
            pnlTop.Controls.Add(cbNahodny);
            pnlTop.Controls.Add(btnSpustit);
            pnlTop.Controls.Add(btnZastavit);
            pnlTop.Controls.Add(btnPauza);
            pnlTop.BackColor = System.Drawing.SystemColors.ControlLight;
            pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            pnlTop.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            pnlTop.Height = 50;
            pnlTop.Padding = new System.Windows.Forms.Padding(6, 8, 6, 0);
            pnlTop.WrapContents = false;

            // lblTrvanieLabel
            lblTrvanieLabel.AutoSize = true;
            lblTrvanieLabel.Margin = new System.Windows.Forms.Padding(0, 7, 0, 0);
            lblTrvanieLabel.Text = "Trvanie (hod):";

            // nudTrvanie
            nudTrvanie.Margin = new System.Windows.Forms.Padding(2, 4, 10, 0);
            nudTrvanie.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudTrvanie.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudTrvanie.Value = new decimal(new int[] { 24, 0, 0, 0 });
            nudTrvanie.Width = 55;

            // lblReplikacieLabel
            lblReplikacieLabel.AutoSize = true;
            lblReplikacieLabel.Margin = new System.Windows.Forms.Padding(0, 7, 0, 0);
            lblReplikacieLabel.Text = "Replikácie:";

            // nudReplikacie
            nudReplikacie.Margin = new System.Windows.Forms.Padding(2, 4, 10, 0);
            nudReplikacie.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudReplikacie.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudReplikacie.Value = new decimal(new int[] { 1, 0, 0, 0 });
            nudReplikacie.Width = 70;

            // lblSeedLabel
            lblSeedLabel.AutoSize = true;
            lblSeedLabel.Margin = new System.Windows.Forms.Padding(0, 7, 0, 0);
            lblSeedLabel.Text = "Seed:";

            // nudSeed
            nudSeed.Margin = new System.Windows.Forms.Padding(2, 4, 10, 0);
            nudSeed.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            nudSeed.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            nudSeed.Value = new decimal(new int[] { 42, 0, 0, 0 });
            nudSeed.Width = 70;

            // cbNahodny
            cbNahodny.AutoSize = true;
            cbNahodny.Margin = new System.Windows.Forms.Padding(0, 6, 12, 0);
            cbNahodny.Text = "Náhodný";
            cbNahodny.CheckedChanged += CbNahodny_CheckedChanged;

            // btnSpustit
            btnSpustit.BackColor = System.Drawing.Color.LightGreen;
            btnSpustit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            btnSpustit.Size = new System.Drawing.Size(95, 28);
            btnSpustit.Text = "▶  Spustiť";
            btnSpustit.UseVisualStyleBackColor = false;
            btnSpustit.Click += BtnSpustit_Click;

            // btnZastavit
            btnZastavit.BackColor = System.Drawing.Color.Salmon;
            btnZastavit.Enabled = false;
            btnZastavit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            btnZastavit.Size = new System.Drawing.Size(95, 28);
            btnZastavit.Text = "⏹  Zastaviť";
            btnZastavit.UseVisualStyleBackColor = false;
            btnZastavit.Click += BtnZastavit_Click;

            // btnPauza
            btnPauza.BackColor = System.Drawing.Color.LightYellow;
            btnPauza.Enabled = false;
            btnPauza.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            btnPauza.Size = new System.Drawing.Size(120, 28);
            btnPauza.Text = "⏸  Pauza";
            btnPauza.UseVisualStyleBackColor = false;
            btnPauza.Click += BtnPauza_Click;

            // pnlKonfZdrojov
            pnlKonfZdrojov.Controls.Add(lblSestryKonfLabel);
            pnlKonfZdrojov.Controls.Add(nudSestry);
            pnlKonfZdrojov.Controls.Add(lblLekariKonfLabel);
            pnlKonfZdrojov.Controls.Add(nudLekari);
            pnlKonfZdrojov.Controls.Add(lblMiestnostiAKonfLabel);
            pnlKonfZdrojov.Controls.Add(nudMiestnostiA);
            pnlKonfZdrojov.Controls.Add(lblMiestnostiBKonfLabel);
            pnlKonfZdrojov.Controls.Add(nudMiestnostiB);
            pnlKonfZdrojov.BackColor = System.Drawing.SystemColors.ControlLight;
            pnlKonfZdrojov.Dock = System.Windows.Forms.DockStyle.Top;
            pnlKonfZdrojov.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            pnlKonfZdrojov.Height = 36;
            pnlKonfZdrojov.Padding = new System.Windows.Forms.Padding(6, 6, 6, 0);
            pnlKonfZdrojov.WrapContents = false;

            // lblSestryKonfLabel
            lblSestryKonfLabel.AutoSize = true;
            lblSestryKonfLabel.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            lblSestryKonfLabel.Text = "Sestry:";

            // nudSestry
            nudSestry.Margin = new System.Windows.Forms.Padding(2, 1, 14, 0);
            nudSestry.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudSestry.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            nudSestry.Value = new decimal(new int[] { 3, 0, 0, 0 });
            nudSestry.Width = 50;

            // lblLekariKonfLabel
            lblLekariKonfLabel.AutoSize = true;
            lblLekariKonfLabel.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            lblLekariKonfLabel.Text = "Lekári:";

            // nudLekari
            nudLekari.Margin = new System.Windows.Forms.Padding(2, 1, 14, 0);
            nudLekari.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudLekari.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            nudLekari.Value = new decimal(new int[] { 2, 0, 0, 0 });
            nudLekari.Width = 50;

            // lblMiestnostiAKonfLabel
            lblMiestnostiAKonfLabel.AutoSize = true;
            lblMiestnostiAKonfLabel.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            lblMiestnostiAKonfLabel.Text = "Miestnosti A:";

            // nudMiestnostiA
            nudMiestnostiA.Margin = new System.Windows.Forms.Padding(2, 1, 14, 0);
            nudMiestnostiA.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudMiestnostiA.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
            nudMiestnostiA.Value = new decimal(new int[] { 5, 0, 0, 0 });
            nudMiestnostiA.Width = 50;

            // lblMiestnostiBKonfLabel
            lblMiestnostiBKonfLabel.AutoSize = true;
            lblMiestnostiBKonfLabel.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            lblMiestnostiBKonfLabel.Text = "Miestnosti B:";

            // nudMiestnostiB
            nudMiestnostiB.Margin = new System.Windows.Forms.Padding(2, 1, 14, 0);
            nudMiestnostiB.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudMiestnostiB.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
            nudMiestnostiB.Value = new decimal(new int[] { 7, 0, 0, 0 });
            nudMiestnostiB.Width = 50;

            // pnlSpeed
            pnlSpeed.Controls.Add(lblDurationLabel);
            pnlSpeed.Controls.Add(trkDuration);
            pnlSpeed.Controls.Add(lblDurationVal);
            pnlSpeed.Controls.Add(lblIntervalLabel);
            pnlSpeed.Controls.Add(trkInterval);
            pnlSpeed.Controls.Add(lblIntervalVal);
            pnlSpeed.BackColor = System.Drawing.SystemColors.ControlLight;
            pnlSpeed.Dock = System.Windows.Forms.DockStyle.Top;
            pnlSpeed.Height = 55;
            pnlSpeed.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);

            // lblDurationLabel
            lblDurationLabel.AutoSize = true;
            lblDurationLabel.Location = new System.Drawing.Point(8, 10);
            lblDurationLabel.Text = "Trvanie pauzy:";

            // trkDuration
            trkDuration.LargeChange = 100;
            trkDuration.Location = new System.Drawing.Point(112, 4);
            trkDuration.Maximum = 2000;
            trkDuration.Minimum = 0;
            trkDuration.Size = new System.Drawing.Size(200, 45);
            trkDuration.SmallChange = 10;
            trkDuration.TickFrequency = 200;
            trkDuration.Value = 0;
            trkDuration.ValueChanged += SliderChanged;

            // lblDurationVal
            lblDurationVal.AutoSize = true;
            lblDurationVal.Location = new System.Drawing.Point(316, 10);
            lblDurationVal.Text = "0 ms  (max rýchlosť)";

            // lblIntervalLabel
            lblIntervalLabel.AutoSize = true;
            lblIntervalLabel.Location = new System.Drawing.Point(470, 10);
            lblIntervalLabel.Text = "Interval (sim. sek.):";

            // trkInterval
            trkInterval.LargeChange = 60;
            trkInterval.Location = new System.Drawing.Point(598, 4);
            trkInterval.Maximum = 3600;
            trkInterval.Minimum = 1;
            trkInterval.Size = new System.Drawing.Size(200, 45);
            trkInterval.SmallChange = 1;
            trkInterval.TickFrequency = 300;
            trkInterval.Value = 60;
            trkInterval.ValueChanged += SliderChanged;

            // lblIntervalVal
            lblIntervalVal.AutoSize = true;
            lblIntervalVal.Location = new System.Drawing.Point(802, 10);
            lblIntervalVal.Text = "60 s";

            // pnlStatus
            pnlStatus.Controls.Add(lblSimCas);
            pnlStatus.Controls.Add(lblReplikacia);
            pnlStatus.Controls.Add(lblPocetPacientov);
            pnlStatus.Controls.Add(lblPocetVybavenych);
            pnlStatus.BackColor = System.Drawing.SystemColors.Info;
            pnlStatus.Dock = System.Windows.Forms.DockStyle.Top;
            pnlStatus.Height = 26;

            // lblSimCas
            lblSimCas.AutoSize = true;
            lblSimCas.Location = new System.Drawing.Point(6, 5);
            lblSimCas.Text = "Čas: —";

            // lblReplikacia
            lblReplikacia.AutoSize = true;
            lblReplikacia.Location = new System.Drawing.Point(130, 5);
            lblReplikacia.Text = "Replikácia: —";

            // lblPocetPacientov
            lblPocetPacientov.AutoSize = true;
            lblPocetPacientov.Location = new System.Drawing.Point(320, 5);
            lblPocetPacientov.Text = "Pacienti v systéme: 0";

            // lblPocetVybavenych
            lblPocetVybavenych.AutoSize = true;
            lblPocetVybavenych.Location = new System.Drawing.Point(510, 5);
            lblPocetVybavenych.Text = "Vybavených: 0";

            // tabControl
            tabControl.Controls.Add(tpPacienti);
            tabControl.Controls.Add(tpZdroje);
            tabControl.Controls.Add(tpRady);
            tabControl.Controls.Add(tpAnimator);
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;

            // tpPacienti
            tpPacienti.Controls.Add(dgvPacienti);
            tpPacienti.Text = "Pacienti";

            // dgvPacienti
            dgvPacienti.AllowUserToAddRows = false;
            dgvPacienti.AllowUserToDeleteRows = false;
            dgvPacienti.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.None;
            dgvPacienti.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvPacienti.ReadOnly = true;
            dgvPacienti.RowHeadersVisible = false;
            dgvPacienti.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

            // tpZdroje
            tpZdroje.Controls.Add(tlpZdroje);
            tpZdroje.Text = "Zdroje";

            // tlpZdroje
            tlpZdroje.ColumnCount = 3;
            tlpZdroje.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            tlpZdroje.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 170F));
            tlpZdroje.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpZdroje.Controls.Add(lblZdrojH, 0, 0);
            tlpZdroje.Controls.Add(lblVolneH, 1, 0);
            tlpZdroje.Controls.Add(lblVytazenieH, 2, 0);
            tlpZdroje.Controls.Add(lblSestryRow, 0, 1);
            tlpZdroje.Controls.Add(lblSestryVal, 1, 1);
            tlpZdroje.Controls.Add(pbSestry, 2, 1);
            tlpZdroje.Controls.Add(lblLekariRow, 0, 2);
            tlpZdroje.Controls.Add(lblLekariVal, 1, 2);
            tlpZdroje.Controls.Add(pbLekari, 2, 2);
            tlpZdroje.Controls.Add(lblMiestnostiARow, 0, 3);
            tlpZdroje.Controls.Add(lblMiestnostiAVal, 1, 3);
            tlpZdroje.Controls.Add(pbMiestnostiA, 2, 3);
            tlpZdroje.Controls.Add(lblMiestnostiBRow, 0, 4);
            tlpZdroje.Controls.Add(lblMiestnostiBVal, 1, 4);
            tlpZdroje.Controls.Add(pbMiestnostiB, 2, 4);
            tlpZdroje.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpZdroje.Padding = new System.Windows.Forms.Padding(12);
            tlpZdroje.RowCount = 5;
            tlpZdroje.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            tlpZdroje.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            tlpZdroje.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            tlpZdroje.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            tlpZdroje.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));

            // header labels
            lblZdrojH.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblZdrojH.AutoSize = true;
            lblZdrojH.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblZdrojH.Text = "Zdroj";

            lblVolneH.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblVolneH.AutoSize = true;
            lblVolneH.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblVolneH.Text = "Voľné / Celkom";

            lblVytazenieH.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblVytazenieH.AutoSize = true;
            lblVytazenieH.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblVytazenieH.Text = "Vyťaženie";

            // sestry row
            lblSestryRow.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblSestryRow.AutoSize = true;
            lblSestryRow.Text = "Sestry";

            lblSestryVal.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblSestryVal.AutoSize = true;
            lblSestryVal.Text = "—";

            pbSestry.Dock = System.Windows.Forms.DockStyle.Fill;
            pbSestry.Maximum = 3;

            // lekari row
            lblLekariRow.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblLekariRow.AutoSize = true;
            lblLekariRow.Text = "Lekári";

            lblLekariVal.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblLekariVal.AutoSize = true;
            lblLekariVal.Text = "—";

            pbLekari.Dock = System.Windows.Forms.DockStyle.Fill;
            pbLekari.Maximum = 2;

            // miestnosti A row
            lblMiestnostiARow.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblMiestnostiARow.AutoSize = true;
            lblMiestnostiARow.Text = "Miestnosti A";

            lblMiestnostiAVal.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblMiestnostiAVal.AutoSize = true;
            lblMiestnostiAVal.Text = "—";

            pbMiestnostiA.Dock = System.Windows.Forms.DockStyle.Fill;
            pbMiestnostiA.Maximum = 5;

            // miestnosti B row
            lblMiestnostiBRow.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblMiestnostiBRow.AutoSize = true;
            lblMiestnostiBRow.Text = "Miestnosti B";

            lblMiestnostiBVal.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblMiestnostiBVal.AutoSize = true;
            lblMiestnostiBVal.Text = "—";

            pbMiestnostiB.Dock = System.Windows.Forms.DockStyle.Fill;
            pbMiestnostiB.Maximum = 7;

            // tpRady
            tpRady.Controls.Add(scRady);
            tpRady.Text = "Rady";

            // scRady
            scRady.Dock = System.Windows.Forms.DockStyle.Fill;
            scRady.Orientation = System.Windows.Forms.Orientation.Vertical;

            scRady.Panel1.Controls.Add(lbRadVV);
            scRady.Panel1.Controls.Add(lblRadVVCount);

            lblRadVVCount.Dock = System.Windows.Forms.DockStyle.Top;
            lblRadVVCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblRadVVCount.Height = 22;
            lblRadVVCount.Text = "Rad VV (0)";

            lbRadVV.Dock = System.Windows.Forms.DockStyle.Fill;

            scRady.Panel2.Controls.Add(lbRadOsetrenie);
            scRady.Panel2.Controls.Add(lblRadOsetrenieCount);

            lblRadOsetrenieCount.Dock = System.Windows.Forms.DockStyle.Top;
            lblRadOsetrenieCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblRadOsetrenieCount.Height = 22;
            lblRadOsetrenieCount.Text = "Rad ošetrenie (0)";

            lbRadOsetrenie.Dock = System.Windows.Forms.DockStyle.Fill;

            // tpAnimator
            tpAnimator.Controls.Add(pnlAnimator);
            tpAnimator.Text = "Animátor";

            // pnlAnimator
            pnlAnimator.BackColor = System.Drawing.Color.White;
            pnlAnimator.Dock = System.Windows.Forms.DockStyle.Fill;

            // Form
            Controls.Add(tabControl);
            Controls.Add(pnlStatus);
            Controls.Add(pnlSpeed);
            Controls.Add(pnlKonfZdrojov);
            Controls.Add(pnlTop);
            ClientSize = new System.Drawing.Size(900, 650);
            MinimumSize = new System.Drawing.Size(700, 500);
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Simulácia nemocnice";

            ((System.ComponentModel.ISupportInitialize)nudTrvanie).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudReplikacie).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudSeed).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudSestry).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudLekari).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudMiestnostiA).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudMiestnostiB).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkDuration).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkInterval).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvPacienti).EndInit();
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            pnlKonfZdrojov.ResumeLayout(false);
            pnlKonfZdrojov.PerformLayout();
            pnlSpeed.ResumeLayout(false);
            pnlSpeed.PerformLayout();
            pnlStatus.ResumeLayout(false);
            pnlStatus.PerformLayout();
            tpPacienti.ResumeLayout(false);
            tlpZdroje.ResumeLayout(false);
            tlpZdroje.PerformLayout();
            tpZdroje.ResumeLayout(false);
            scRady.Panel1.ResumeLayout(false);
            scRady.Panel2.ResumeLayout(false);
            scRady.ResumeLayout(false);
            tpRady.ResumeLayout(false);
            tpAnimator.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            ResumeLayout(false);
        }

        private System.Windows.Forms.FlowLayoutPanel pnlTop;
        private System.Windows.Forms.Label lblTrvanieLabel;
        private System.Windows.Forms.NumericUpDown nudTrvanie;
        private System.Windows.Forms.Label lblReplikacieLabel;
        private System.Windows.Forms.NumericUpDown nudReplikacie;
        private System.Windows.Forms.Label lblSeedLabel;
        private System.Windows.Forms.NumericUpDown nudSeed;
        private System.Windows.Forms.CheckBox cbNahodny;
        private System.Windows.Forms.Button btnSpustit;
        private System.Windows.Forms.Button btnZastavit;
        private System.Windows.Forms.Button btnPauza;
        private System.Windows.Forms.FlowLayoutPanel pnlKonfZdrojov;
        private System.Windows.Forms.Label lblSestryKonfLabel;
        private System.Windows.Forms.NumericUpDown nudSestry;
        private System.Windows.Forms.Label lblLekariKonfLabel;
        private System.Windows.Forms.NumericUpDown nudLekari;
        private System.Windows.Forms.Label lblMiestnostiAKonfLabel;
        private System.Windows.Forms.NumericUpDown nudMiestnostiA;
        private System.Windows.Forms.Label lblMiestnostiBKonfLabel;
        private System.Windows.Forms.NumericUpDown nudMiestnostiB;
        private System.Windows.Forms.Panel pnlSpeed;
        private System.Windows.Forms.Label lblDurationLabel;
        private System.Windows.Forms.TrackBar trkDuration;
        private System.Windows.Forms.Label lblDurationVal;
        private System.Windows.Forms.Label lblIntervalLabel;
        private System.Windows.Forms.TrackBar trkInterval;
        private System.Windows.Forms.Label lblIntervalVal;
        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.Label lblSimCas;
        private System.Windows.Forms.Label lblReplikacia;
        private System.Windows.Forms.Label lblPocetPacientov;
        private System.Windows.Forms.Label lblPocetVybavenych;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tpPacienti;
        private System.Windows.Forms.DataGridView dgvPacienti;
        private System.Windows.Forms.TabPage tpZdroje;
        private System.Windows.Forms.TableLayoutPanel tlpZdroje;
        private System.Windows.Forms.Label lblZdrojH;
        private System.Windows.Forms.Label lblVolneH;
        private System.Windows.Forms.Label lblVytazenieH;
        private System.Windows.Forms.Label lblSestryRow;
        private System.Windows.Forms.Label lblSestryVal;
        private System.Windows.Forms.ProgressBar pbSestry;
        private System.Windows.Forms.Label lblLekariRow;
        private System.Windows.Forms.Label lblLekariVal;
        private System.Windows.Forms.ProgressBar pbLekari;
        private System.Windows.Forms.Label lblMiestnostiARow;
        private System.Windows.Forms.Label lblMiestnostiAVal;
        private System.Windows.Forms.ProgressBar pbMiestnostiA;
        private System.Windows.Forms.Label lblMiestnostiBRow;
        private System.Windows.Forms.Label lblMiestnostiBVal;
        private System.Windows.Forms.ProgressBar pbMiestnostiB;
        private System.Windows.Forms.TabPage tpRady;
        private System.Windows.Forms.SplitContainer scRady;
        private System.Windows.Forms.Label lblRadVVCount;
        private System.Windows.Forms.ListBox lbRadVV;
        private System.Windows.Forms.Label lblRadOsetrenieCount;
        private System.Windows.Forms.ListBox lbRadOsetrenie;
        private System.Windows.Forms.TabPage tpAnimator;
        private System.Windows.Forms.Panel pnlAnimator;
    }
}
