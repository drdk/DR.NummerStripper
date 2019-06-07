using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using DR.NummerStripper.Properties;

namespace DR.NummerStripper
{
    internal class ProductionForm : Form
    {
        private string _prdNbr;
        private FlowLayoutPanel flowPanel;
        private Label helpText;
        private Label title;
        private Label lbPrdNbr;
        private readonly MuService _muService;

        internal string PrdNbr
        {
            set
            {
                _prdNbr = value;
                lbPrdNbr.Text = _prdNbr;
                var pc = _muService.GetByProductionNumber(_prdNbr);
                title.Text = pc?.Title ?? "[not found]";
            }
        }
        
        internal ProductionForm(string prdNbr)
        {
            _muService = new MuService();
            InitializeComponent();
            PrdNbr = prdNbr;
            var index = 1;
            foreach (var link in Settings.Default.ProductionNumberLinks.Cast<string>().Take(15)
                .Select(x =>
                {
                    var temp = x.Split(';');

                    return new
                    {
                        Index = index++,
                        Name = temp[0],
                        Target = temp[1]
                    };
                }))
            {
                var btn = new Button()
                {
                    Text = $"&{link.Index:X} : {link.Name}",
                    Size = new System.Drawing.Size(286, 23),
                    TabIndex = link.Index + 1
                };
                btn.Click += (sender, args) =>
                {
                    var process = Process.Start(link.Target.Replace("{prdNbr}", _prdNbr));
                    this.Close();
                };
                this.flowPanel.Controls.Add(btn);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void InitializeComponent()
        {
            this.lbPrdNbr = new System.Windows.Forms.Label();
            this.flowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.helpText = new System.Windows.Forms.Label();
            this.title = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbPrdNbr
            // 
            this.lbPrdNbr.AutoSize = true;
            this.lbPrdNbr.Location = new System.Drawing.Point(13, 13);
            this.lbPrdNbr.Name = "lbPrdNbr";
            this.lbPrdNbr.Size = new System.Drawing.Size(39, 13);
            this.lbPrdNbr.TabIndex = 0;
            this.lbPrdNbr.Text = "prdNbr";
            // 
            // flowPanel
            // 
            this.flowPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowPanel.Location = new System.Drawing.Point(13, 46);
            this.flowPanel.Name = "flowPanel";
            this.flowPanel.Size = new System.Drawing.Size(289, 391);
            this.flowPanel.TabIndex = 1;
            // 
            // helpText
            // 
            this.helpText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.helpText.AutoSize = true;
            this.helpText.Location = new System.Drawing.Point(13, 441);
            this.helpText.Name = "helpText";
            this.helpText.Size = new System.Drawing.Size(260, 13);
            this.helpText.TabIndex = 2;
            this.helpText.Text = "Klik <nummer> for starte, eller Esc for at lukke vinduet";
            // 
            // title
            // 
            this.title.AutoSize = true;
            this.title.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.title.Location = new System.Drawing.Point(13, 30);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(32, 13);
            this.title.TabIndex = 3;
            this.title.Text = "title ";
            // 
            // ProductionForm
            // 
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(314, 465);
            this.Controls.Add(this.title);
            this.Controls.Add(this.helpText);
            this.Controls.Add(this.flowPanel);
            this.Controls.Add(this.lbPrdNbr);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "ProductionForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "ProduktionsNummer værktøj";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

    }
}
