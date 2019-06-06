using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DR.NummerStripper
{
    internal class ProductionForm : Form
    {
        private readonly string _prdNmr;
        private Label lbPrdNmr;
        private Button btnMA;
        private Button btnODPubPlan;
        private Button btnPSDB;

        internal ProductionForm(string prdNmr)
        {
            InitializeComponent();
            lbPrdNmr.Text = _prdNmr = prdNmr;
        }
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProductionForm));
            this.lbPrdNmr = new System.Windows.Forms.Label();
            this.btnMA = new System.Windows.Forms.Button();
            this.btnPSDB = new System.Windows.Forms.Button();
            this.btnODPubPlan = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbPrdNmr
            // 
            this.lbPrdNmr.AutoSize = true;
            this.lbPrdNmr.Location = new System.Drawing.Point(13, 13);
            this.lbPrdNmr.Name = "lbPrdNmr";
            this.lbPrdNmr.Size = new System.Drawing.Size(41, 13);
            this.lbPrdNmr.TabIndex = 0;
            this.lbPrdNmr.Text = "prdNmr";
            // 
            // btnMA
            // 
            this.btnMA.Location = new System.Drawing.Point(12, 29);
            this.btnMA.Name = "btnMA";
            this.btnMA.Size = new System.Drawing.Size(260, 23);
            this.btnMA.TabIndex = 1;
            this.btnMA.Text = "MA";
            this.btnMA.UseVisualStyleBackColor = true;
            this.btnMA.Click += new System.EventHandler(this.btnMA_Click);
            // 
            // btnPSDB
            // 
            this.btnPSDB.Location = new System.Drawing.Point(12, 58);
            this.btnPSDB.Name = "btnPSDB";
            this.btnPSDB.Size = new System.Drawing.Size(260, 23);
            this.btnPSDB.TabIndex = 2;
            this.btnPSDB.Text = "PSDB";
            this.btnPSDB.UseVisualStyleBackColor = true;
            this.btnPSDB.Click += new System.EventHandler(this.btnPSDB_Click);
            // 
            // btnODPubPlan
            // 
            this.btnODPubPlan.Location = new System.Drawing.Point(12, 87);
            this.btnODPubPlan.Name = "btnODPubPlan";
            this.btnODPubPlan.Size = new System.Drawing.Size(260, 23);
            this.btnODPubPlan.TabIndex = 3;
            this.btnODPubPlan.Text = "OD PubPlan";
            this.btnODPubPlan.UseVisualStyleBackColor = true;
            this.btnODPubPlan.Click += new System.EventHandler(this.btnODPubPlan_Click);
            // 
            // ProductionForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 124);
            this.Controls.Add(this.btnODPubPlan);
            this.Controls.Add(this.btnPSDB);
            this.Controls.Add(this.btnMA);
            this.Controls.Add(this.lbPrdNmr);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ProductionForm";
            this.Text = "ProduktionsNummer værktøj";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private void btnMA_Click(object sender, EventArgs e)
        {
            var process = Process.Start($"http://ma:7000/#!/search/item?q={_prdNmr}");
            this.Close();
        }

        private void btnODPubPlan_Click(object sender, EventArgs e)
        {
            var process = Process.Start($"https://odpubplan/Publication/Search?query={_prdNmr}");
            this.Close();
        }

        private void btnPSDB_Click(object sender, EventArgs e)
        {
            var process = Process.Start($"http://mu.net.dr.dk/admin/#searchString={_prdNmr}&searchCategories=10%2C20%2C30%2C90&limit=20&channelType=TV%2CRADIO");
            this.Close();
        }

    }
}
