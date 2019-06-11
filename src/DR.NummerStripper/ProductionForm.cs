using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using DR.NummerStripper.MU;
using DR.NummerStripper.Properties;

namespace DR.NummerStripper
{
    internal class ProductionForm : Form
    {
        private string _prdNbr;
        private FlowLayoutPanel flowPanel1;
        private Label helpText;
        private Label title;
        private Label lbPrdNbr;
        private Panel panel1;
        private Panel panel2;
        private FlowLayoutPanel flowPanel2;
        private PictureBox pictureBox;
        private Label dateLbl;
        private readonly ProductionService _productionService;
        private Label channel;
        private Button btnDRDK;

        private void RefreshData()
        {
            _prdNbr = _productionService.Current.ProductionNumber;
            lbPrdNbr.Text = _prdNbr;
            var pc = _productionService.Current.ProgramCard;
            if (pc == null)
            {
                panel2.Hide();
                return;
            }
            panel2.Show();
            title.Text = pc.Title;
            channel.Text = $"{pc.PrimaryChannel?.Split('/').Last()} ({pc.ChannelType})";
            dateLbl.Text = pc.PrimaryBroadcastStartTime?.ToString("g") ?? string.Empty;
            pictureBox.Image = _productionService.Current.Image;

            btnDRDK.Enabled = pc.PresentationUri != null;
        }

        private int index = 1;
        private Button AddButton(string name, string target, FlowLayoutPanel panel)
        {
            var btn = new Button()
            {
                Text = $"&{index:X} : {name}",
                Size = new System.Drawing.Size(286, 23),
                TabIndex = index + 1
            };
            btn.Click += (sender, args) =>
            {
                var temp = target;
                if (temp.Contains("{prdNbr}"))
                    temp = temp.Replace("{prdNbr}", _prdNbr);
                if (temp.Contains("{PresentationUri}"))
                    temp = temp.Replace("{PresentationUri}", _productionService.Current.ProgramCard.PresentationUri.ToString());
                var process = Process.Start(temp);
                this.Close();
            };
            panel.Controls.Add(btn);
            index++;
            return btn;
        }
        internal ProductionForm(ProductionService productionService)
        {
            _productionService = productionService;
            productionService.PropertyChanged += (sender, args) => RefreshData();
            InitializeComponent();
            
            foreach (var link in Settings.Default.ProductionNumberLinks.Cast<string>().Take(15)
                .Select(x =>
                {
                    var temp = x.Split(';');
                    return new
                    {
                        Name = temp[0],
                        Target = temp[1]
                    };
                }))
            {
                AddButton(link.Name,link.Target, this.flowPanel1);
            }
            btnDRDK = AddButton("DR.dk", "{PresentationUri}",flowPanel2);
            RefreshData();
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
            this.flowPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.helpText = new System.Windows.Forms.Label();
            this.title = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.channel = new System.Windows.Forms.Label();
            this.dateLbl = new System.Windows.Forms.Label();
            this.flowPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // lbPrdNbr
            // 
            this.lbPrdNbr.AutoSize = true;
            this.lbPrdNbr.Location = new System.Drawing.Point(3, 4);
            this.lbPrdNbr.Name = "lbPrdNbr";
            this.lbPrdNbr.Size = new System.Drawing.Size(39, 13);
            this.lbPrdNbr.TabIndex = 0;
            this.lbPrdNbr.Text = "prdNbr";
            // 
            // flowPanel1
            // 
            this.flowPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowPanel1.Location = new System.Drawing.Point(0, 20);
            this.flowPanel1.Name = "flowPanel1";
            this.flowPanel1.Size = new System.Drawing.Size(305, 486);
            this.flowPanel1.TabIndex = 1;
            // 
            // helpText
            // 
            this.helpText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.helpText.AutoSize = true;
            this.helpText.Location = new System.Drawing.Point(3, 515);
            this.helpText.Name = "helpText";
            this.helpText.Size = new System.Drawing.Size(260, 13);
            this.helpText.TabIndex = 2;
            this.helpText.Text = "Klik <nummer> for starte, eller Esc for at lukke vinduet";
            // 
            // title
            // 
            this.title.AutoSize = true;
            this.title.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.title.Location = new System.Drawing.Point(3, 3);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(32, 13);
            this.title.TabIndex = 3;
            this.title.Text = "title ";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel1.Controls.Add(this.flowPanel1);
            this.panel1.Controls.Add(this.helpText);
            this.panel1.Controls.Add(this.lbPrdNbr);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(305, 528);
            this.panel1.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.channel);
            this.panel2.Controls.Add(this.dateLbl);
            this.panel2.Controls.Add(this.flowPanel2);
            this.panel2.Controls.Add(this.title);
            this.panel2.Controls.Add(this.pictureBox);
            this.panel2.Location = new System.Drawing.Point(324, 13);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(390, 527);
            this.panel2.TabIndex = 5;
            // 
            // channel
            // 
            this.channel.AutoSize = true;
            this.channel.Location = new System.Drawing.Point(3, 16);
            this.channel.Name = "channel";
            this.channel.Size = new System.Drawing.Size(45, 13);
            this.channel.TabIndex = 4;
            this.channel.Text = "channel";
            // 
            // dateLbl
            // 
            this.dateLbl.AutoSize = true;
            this.dateLbl.Location = new System.Drawing.Point(3, 29);
            this.dateLbl.Name = "dateLbl";
            this.dateLbl.Size = new System.Drawing.Size(42, 13);
            this.dateLbl.TabIndex = 2;
            this.dateLbl.Text = "dateLbl";
            // 
            // flowPanel2
            // 
            this.flowPanel2.Location = new System.Drawing.Point(3, 99);
            this.flowPanel2.Name = "flowPanel2";
            this.flowPanel2.Size = new System.Drawing.Size(387, 425);
            this.flowPanel2.TabIndex = 1;
            // 
            // pictureBox
            // 
            this.pictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox.Location = new System.Drawing.Point(227, 3);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(160, 90);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            // 
            // ProductionForm
            // 
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(726, 552);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "ProductionForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "ProduktionsNummer værktøj";
            this.TopMost = true;
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);

        }

    }
}
