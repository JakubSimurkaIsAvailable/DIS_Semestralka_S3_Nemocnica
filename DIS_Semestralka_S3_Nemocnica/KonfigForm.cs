using System.IO;
using System.Windows.Forms;

namespace DIS_Semestralka_S3_Nemocnica
{
    public partial class KonfigForm : Form
    {
        public bool PreferVV => rbVVPred.Checked;
        public bool RezervaLekarPreA => cbRezervaLekarPreA.Checked;
        public bool RezervaSestraPreVV => cbRezervaSestraPreVV.Checked;
        public bool MinPohybPersonalu => cbMinPohybPersonalu.Checked;
        public bool RadABPreferA    => cbRadABPreferA.Checked;
        public bool PrefRadBEnabled => cbPrefRadB.Checked;
        public int  PrefRadBPrah    => (int)nudPrefRadBPrah.Value;
        public bool CsvLogEnabled => cbCsvLog.Checked;
        public bool CsvFinalEnabled => cbCsvFinal.Checked;
        public string? AppendFinalCsvPath { get; private set; }

        public KonfigForm()
        {
            InitializeComponent();
        }

        private void BtnCsvFinalAppend_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Vybrať existujúci CSV súbor na pripojenie",
                Filter = "CSV súbory (*.csv)|*.csv|Všetky súbory (*.*)|*.*",
                InitialDirectory = Path.Combine(AppContext.BaseDirectory, "csv"),
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            AppendFinalCsvPath = dlg.FileName;
            string shortName = Path.GetFileName(AppendFinalCsvPath);
            btnCsvFinalAppend.Text = $"📎 {shortName}";
            btnCsvFinalAppend.BackColor = System.Drawing.Color.LightYellow;
            var tt = new ToolTip();
            tt.SetToolTip(btnCsvFinalAppend, AppendFinalCsvPath);
        }

        private void BtnZavriet_Click(object sender, EventArgs e) => Hide();

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                base.OnFormClosing(e);
            }
        }
    }
}
