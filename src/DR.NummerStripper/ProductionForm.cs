using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DR.NummerStripper.Properties;

namespace DR.NummerStripper
{
    internal class ProductionForm : Form
    {
        private string _prdNbr;
        private FlowLayoutPanel flowPanel;
        private Label helpText;
        private Label lbPrdNbr;

        internal string PrdNbr
        {
            set
            {
                _prdNbr = value;
                lbPrdNbr.Text = _prdNbr;
            }
        }

        internal ProductionForm(string prdNbr)
        {
            _prdNbr = prdNbr;
            InitializeComponent();
            lbPrdNbr.Text = _prdNbr;
            var index = 1;
            foreach (var link in Settings.Default.ProductionNumberLinks.Cast<string>()
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
                    Text = $"&{link.Index} : {link.Name}",
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
            this.flowPanel.Location = new System.Drawing.Point(13, 30);
            this.flowPanel.Name = "flowPanel";
            this.flowPanel.Size = new System.Drawing.Size(289, 265);
            this.flowPanel.TabIndex = 1;
            // 
            // helpText
            // 
            this.helpText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.helpText.AutoSize = true;
            this.helpText.Location = new System.Drawing.Point(13, 299);
            this.helpText.Name = "helpText";
            this.helpText.Size = new System.Drawing.Size(260, 13);
            this.helpText.TabIndex = 2;
            this.helpText.Text = "Klik <nummer> for starte, eller Esc for at lukke vinduet";
            // 
            // ProductionForm
            // 
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(314, 323);
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
