using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cryptocurrency_Checkbook
{
    public partial class CryptoCheckbook : Form
    {
        public CryptoCheckbook()
        {
            InitializeComponent();
            try
            {
                LoadEntries();
            }
            catch(Exception e)
            {

            }
        }

        private void dataGridView_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            UpdateAllData();
        }            

        private void UpdateMarketValues()
        {
            foreach(DataGridViewRow r in dataGridView.Rows)
            {
                if(r.Cells["Currency"].Value != null && r.Cells["Currency"].Value.ToString() != "")
                {
                    string json = GetMarketValue(r.Cells["Currency"].Value.ToString());
                    json = json.Replace("[", "").Replace("]",""); //only ever going to be 1 object
                    var obj = JsonConvert.DeserializeObject<Currency>(json);
                    r.Cells["NowPerCoin"].Value = obj.price_usd;

                    labelLastLoad.Text = "Last market value load: " + DateTime.Now.ToString();
                }
            }
        }

        private void UpdateNetGain()
        {
            foreach (DataGridViewRow r in dataGridView.Rows)
            {
                try
                {
                    if ((r.Cells["PaidPerCoin"].Value != null && r.Cells["PaidPerCoin"].Value.ToString() != "")
                        && (r.Cells["NowPerCoin"].Value != null && r.Cells["NowPerCoin"].Value.ToString() != "")
                        && (r.Cells["Quantity"].Value != null && r.Cells["Quantity"].Value.ToString() != ""))
                    {
                        decimal paidValue = Convert.ToDecimal(r.Cells["PaidPerCoin"].Value);
                        decimal quantity = Convert.ToDecimal(r.Cells["Quantity"].Value);
                        decimal marketValue = Convert.ToDecimal(r.Cells["NowPerCoin"].Value);

                        r.Cells["Net"].Value = Convert.ToString((marketValue - paidValue) * quantity);
                        if (Convert.ToDecimal(r.Cells["Net"].Value) < 0)
                            r.Cells["Net"].Style.BackColor = Color.Red;
                        else
                            r.Cells["Net"].Style.BackColor = Color.Green;
                    }
                }
                catch (Exception e)
                {

                }
            }
        }

        private void UpdatePerCoinPaid()
        {
            foreach (DataGridViewRow r in dataGridView.Rows)
            {
                try
                {
                    if ((r.Cells["PurchasePrice"].Value != null && r.Cells["PurchasePrice"].Value.ToString() != "")
                        && (r.Cells["Quantity"].Value != null && r.Cells["Quantity"].Value.ToString() != ""))
                    {
                        decimal paidValue = Convert.ToDecimal(r.Cells["PurchasePrice"].Value);
                        decimal quantity = Convert.ToDecimal(r.Cells["Quantity"].Value);

                        r.Cells["PaidPerCoin"].Value = Convert.ToString(paidValue/quantity);
                    }
                }
                catch (Exception e)
                {

                }
            }
        }

        private string GetMarketValue(string currency)
        {
            string url = string.Format(@"https://api.coinmarketcap.com/v1/ticker/{0}/?convert=USD", currency);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    // log errorText
                }
                return "error";//throw;
            }
        }

        private void SaveEntries()
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(@"UserEntries.txt");
            foreach (DataGridViewRow r in dataGridView.Rows)
            {
                if ((r.Cells["Currency"].Value != null && r.Cells["Currency"].Value.ToString() != "")
                    && (r.Cells["PurchasePrice"].Value != null && r.Cells["PurchasePrice"].Value.ToString() != "")
                    && (r.Cells["Quantity"].Value != null && r.Cells["Quantity"].Value.ToString() != ""))
                {
                    file.WriteLine(r.Cells["Currency"].Value + ";" + r.Cells["PurchasePrice"].Value + ";" + r.Cells["Quantity"].Value + ";");
                }
            }
            file.Close();
        }

        private void LoadEntries()
        {
            dataGridView.Rows.Clear();

            System.IO.StreamReader file = new System.IO.StreamReader(@"UserEntries.txt");
            string line;
            while ((line = file.ReadLine()) != null)
            {
                DataGridViewRow row = (DataGridViewRow)dataGridView.Rows[0].Clone();
                int loc = line.IndexOf(";"); //first
                row.Cells[0].Value = line.Substring(0, loc); //currency

                line = line.Substring(loc + 1);
                loc = line.IndexOf(";"); //second
                row.Cells[2].Value = line.Substring(0, loc); //quantity

                line = line.Substring(loc + 1);
                loc = line.IndexOf(";"); //thirs
                row.Cells[1].Value = line.Substring(0, loc); //price paid

                dataGridView.Rows.Add(row);
            }
            UpdateAllData();
            file.Close();
        }

        private void UpdateAllData()
        {
            UpdateMarketValues();
            UpdatePerCoinPaid();
            UpdateNetGain();
            UpdateLink();
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveEntries();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadEntries();
        }

        private void clearGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView.Rows.Clear();
        }

        private void refreshDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateAllData();
        }

        private void UpdateLink()
        {
            foreach (DataGridViewRow r in dataGridView.Rows)
            {
                if (r.Cells["Currency"].Value != null && r.Cells["Currency"].Value.ToString() != "")
                {
                    r.Cells["Link"].Value = string.Format(@"https://coinmarketcap.com/currencies/{0}/", r.Cells["Currency"].Value.ToString());
                }
            }
        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.ColumnIndex == 6)
            {
                var url = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                Process.Start(url);
            }
        }

        private void CryptoCheckbook_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                SaveEntries();
            }
            catch(Exception ex)
            {

            }
        }
    }
}
