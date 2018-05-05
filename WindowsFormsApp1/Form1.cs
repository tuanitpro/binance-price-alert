 
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using WindowsFormsApp1.Core;
using Binance.Net;
using Binance.Net.Objects;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

      
        private int CountItem = 0;
       
        private Setting CurrentSetting = null;
        
        public Form1()
        {
            InitializeComponent();
            BuildDataGrid_Alert();          
        }
        private void BuildDataGrid_Alert()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView1.ColumnHeadersHeight = 30;

            DataGridViewColumn dataGridViewColumn = new DataGridViewTextBoxColumn
            {
                Name = "Symbol",
                DataPropertyName = "Symbol",
                ReadOnly = true,
            };
            dataGridView1.Columns.Add(dataGridViewColumn);

            DataGridViewColumn dataGridViewColumnLastPrice = new DataGridViewTextBoxColumn
            {
                Name = "Price",
                DataPropertyName = "LastPrice",
                ReadOnly = true
            };
            dataGridView1.Columns.Add(dataGridViewColumnLastPrice);

            DataGridViewColumn dataGridViewColumnAbove = new DataGridViewTextBoxColumn
            {
                Name = "Above",
                DataPropertyName = "Above",
                ReadOnly = true
            };
            dataGridView1.Columns.Add(dataGridViewColumnAbove);
            DataGridViewColumn dataGridViewColumnBelow = new DataGridViewTextBoxColumn
            {
                Name = "Below",
                DataPropertyName = "Below",
                ReadOnly = true
            };
            dataGridView1.Columns.Add(dataGridViewColumnBelow);
            DataGridViewColumn dataGridViewColumnChangePrice = new DataGridViewTextBoxColumn
            {
                Name = "Change",
                DataPropertyName = "ChangePrice",
                ReadOnly = true
            };
            dataGridView1.Columns.Add(dataGridViewColumnChangePrice);
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            this.Height = 40;
            this.Top = Screen.PrimaryScreen.WorkingArea.Height - 40;
            btnOption.FlatAppearance.BorderSize = 0;
            btnOption.TabStop = false;

            btnOption.FlatStyle = FlatStyle.Flat;
            GetSetting();
            GetCoinmarketcap();
            GetAllSymbolAsync();

            GetData();

            System.Timers.Timer t = new System.Timers.Timer(5000);
            t.AutoReset = true;
            t.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            t.Start();
        }
        CoinmarketcapService coinMarketCapService = new CoinmarketcapService();
        private async Task GetCoinmarketcap()
        {
            var coinMarketCapData = await coinMarketCapService.GetCoinmarketcapGlobal();
            lblCmc.Text = $"Mkc: {coinMarketCapData.TotalMarketCapUsd} - 24h Vol: {coinMarketCapData.Total24hVolumeUsd} - BTC: {coinMarketCapData.BitcoinPercentageOfMarketCap}%";
        }
        private async Task GetAllSymbolAsync()
        {
            var listSymbol = new List<SelectListItem>();
            using (var client = new BinanceClient())
            {
                var rs = await client.GetAllPricesAsync();
                if (rs.Success && rs.Data != null)
                {
                    foreach (var item in rs.Data)
                    {
                        listSymbol.Add(new SelectListItem
                        {
                            Value = item.Symbol,
                            Text = item.Symbol + " " + item.Price
                        });
                    }
                }
            }
            if (listSymbol.Any())
            {
                cbbListSymbols.DataSource = listSymbol;
                cbbListSymbols.ValueMember = "Value";
                cbbListSymbols.DisplayMember = "Text";
                cbbListSymbols.SelectedIndex = 0;
            }
        }
        private async void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            GetData();
        }         
        private void GetSetting()
        {
            CurrentSetting = SettingSingleton.Instance.GetSetting();
            if (CurrentSetting != null)
            {
                if (CurrentSetting.ListPrices != null)
                {
                    DisplayWatchlists();
                }
                if (!string.IsNullOrEmpty(CurrentSetting.FilePath))
                {
                    cbbAudio.Text = CurrentSetting.FilePath;
                    cbbAudio.SelectedIndex = 0;
                }
            }
        }
        private void DisplayWatchlists()
        {
            if (this.InvokeRequired)
            {
                dataGridView1.Invoke((Action)delegate ()
                {
                    dataGridView1.DataSource = CurrentSetting.ListPrices;
                    foreach (DataGridViewRow item in dataGridView1.Rows)
                    {
                        var getChange = item.Cells[4].Value;
                        if (getChange != null)
                        {
                            var convert = decimal.Parse(getChange.ToString());
                            if (convert > 0)
                            {
                                item.DefaultCellStyle.ForeColor = Color.Green;
                            }
                            else
                            {
                                item.DefaultCellStyle.ForeColor = Color.Red;
                            }
                        }
                    }
                });
            }
            else
            {
                dataGridView1.DataSource = CurrentSetting.ListPrices;
                foreach (DataGridViewRow item in dataGridView1.Rows)
                {
                    item.DefaultCellStyle.ForeColor = Color.Red;
                }
            }
        }
        private async void lblText_Click(object sender, EventArgs e)
        {
            DisplayText();
            if (player != null) player.Stop();
        }
        private List<Binance24HPrice> binance24HPrices = null;        
        private async void GetData()
        {
            try
            {
                var listPrices = CurrentSetting.ListPrices;
                List<string> listSymbol = listPrices.Select(x => x.Symbol).ToList();
                using (var client = new BinanceClient())
                {
                    var rs = await client.Get24HPricesListAsync();
                    if (rs.Success && rs.Data != null)
                    {
                        var data = rs.Data.Where(x => listSymbol.Contains(x.Symbol)).ToList();
                        binance24HPrices = data;
                        DisplayText();
                        CallAlert();
                    }
                }
            }
            catch (Exception ex) { }
        }
        private void DisplayText(string text)
        {
            if (this.InvokeRequired)
            {
                lblText.Invoke((Action)delegate ()
                {
                    lblText.Text = text;
                });
            }
            else
            {
                lblText.Text = text;
            }
        }
        private void DisplayText()
        {
            var listPrices = CurrentSetting.ListPrices;
            var data = binance24HPrices;
            if (data != null && data.Any())
            {
                if (CountItem >= data.Count) CountItem = 0;
                var index = CountItem; //random.Next(data.Count);
                var item = data[index];
                string text = $"{item.Symbol}: Last: {item.LastPrice} H: {item.HighPrice} L: {item.LowPrice} C:{item.PriceChange}";

                DisplayText(item, text);
                if (listPrices.Any())
                {
                    var findItem = listPrices.FirstOrDefault(x => x.Symbol == item.Symbol);
                    findItem.LastPrice = item.LastPrice;
                    findItem.ChangePrice = item.PriceChange;                    
                    CurrentSetting.ListPrices = listPrices;
                    DisplayWatchlists();
                }

                CountItem++;
            }
        }
        private void DisplayText(Binance24HPrice item, string text)
        {
            if (this.InvokeRequired)
            {
                lblText.Invoke((Action)delegate ()
                {
                    lblText.Text = text;
                    lblText.ForeColor = item.PriceChange > 0 ? Color.Green : Color.Red;
                });
            }
            else
            {
                lblText.Text = text;
                lblText.ForeColor = item.PriceChange > 0 ? Color.Green : Color.Red;
            }
        }
        bool callAlert = false;
        private void CallAlert()
        {
            if (CurrentSetting.ListPrices.Any())
            {

                if (binance24HPrices != null && binance24HPrices.Any())
                {
                    var listMarket = CurrentSetting.ListPrices.Select(x => x.Symbol).ToArray();
                    var listDataToCheck = binance24HPrices.Where(x => listMarket.Contains(x.Symbol)).ToList();
                    string text = "";
                    foreach (var item in listDataToCheck)
                    {
                        var itemNeedCheck = CurrentSetting.ListPrices.FirstOrDefault(x => x.Symbol == item.Symbol);
                        var last = item.LastPrice;
                        if (itemNeedCheck.Above < last)
                        {
                            text = $"{item.Symbol} reach price above {itemNeedCheck.Above} on Binance";
                        }
                        else if (itemNeedCheck.Below > last)
                        {
                            text = $"{item.Symbol} reach price below {itemNeedCheck.Below} on Binance";
                        }
                        if (!string.IsNullOrEmpty(text))
                        {
                            PlaySound();
                            DisplayText(item, text);
                        }
                    }
                }
            }
        }
        private System.Media.SoundPlayer player = null;

        private void PlaySound()
        {
            try
            {
                string file = CurrentSetting.FilePath;
                if (string.IsNullOrEmpty(file))
                    file = "alarm0";
                string filePath = $@"{AppDomain.CurrentDomain.BaseDirectory}data\{file}.wav";
                player = new System.Media.SoundPlayer(filePath);                
                player.PlayLooping();
            }
            catch (Exception ex) { }
        }

        private void btnOption_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = true;
            this.Height = 500;
            this.Top = Screen.PrimaryScreen.WorkingArea.Height - 500;
            txtAbove.Text = txtBelow.Text = txtCurrentPrice.Text = ""; 
        }

        private void btnSaveChange_Click(object sender, EventArgs e)
        {
            try
            {
                var fileAudioPath = cbbAudio.Text;                 
                var listPrices = SettingSingleton.Instance.ListPrices;

                var getAbove = txtAbove.Text;
                var getBelow = txtBelow.Text;
                var marketName = btnSaveChange.Tag;
                if (!string.IsNullOrEmpty(getAbove) || !string.IsNullOrEmpty(getBelow))
                {
                    var above = ColorLife.Core.Helper.ConvertType.ToDecimal(getAbove);
                    if (above < 0) above = 0;
                    var below = ColorLife.Core.Helper.ConvertType.ToDecimal(getBelow);
                    if (below < 0) below = 0;
                    if (marketName != null)
                    {
                        var exists = listPrices.FirstOrDefault(x => x.Symbol == marketName.ToString());
                        if (exists == null)
                        {
                            listPrices.Add(new SymbolItem
                            {
                                Symbol = marketName.ToString(),
                                Above = above,
                                Below = below
                            });
                        }
                        else
                        {
                            exists.Above = above;
                            exists.Below = below;
                        }
                    }
                    else
                    {
                        marketName = cbbListSymbols.SelectedValue;
                        var exists = listPrices.FirstOrDefault(x => x.Symbol == marketName.ToString());
                        if (exists == null)
                        {
                            listPrices.Add(new SymbolItem
                            {
                                Symbol = marketName.ToString(),
                                Above = above,
                                Below = below
                            });
                        }
                        else
                        {
                            exists.Above = above;
                            exists.Below = below;
                        }
                    }
                }

                CurrentSetting.FilePath = fileAudioPath;
                CurrentSetting.ListPrices = listPrices;
                SettingHelper.Save(CurrentSetting);
                GetSetting();

                txtAbove.Text = txtBelow.Text = txtCurrentPrice.Text = "";
                btnSaveChange.Tag = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERORR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Height = 40;
            this.Top = Screen.PrimaryScreen.WorkingArea.Height - 40;
            groupBox1.Visible = true;
            txtAbove.Text = txtBelow.Text = txtCurrentPrice.Text = "";
            btnSaveChange.Tag = null;
        }

        private void cbbListMarket_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbbListSymbols.SelectedIndex > -1)
            {
                var value = cbbListSymbols.SelectedValue;
                using (var client = new BinanceClient())
                {
                    var rs = client.Get24HPrice(value.ToString());
                    if (rs.Success && rs.Data != null)
                    {
                        var data = rs.Data;
                        txtCurrentPrice.Text = data.LastPrice.ToString();
                        txtAbove.Text = data.HighPrice.ToString();
                        txtBelow.Text = data.LowPrice.ToString();
                    }
                }
            }
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                var row = dataGridView1.SelectedRows[0].Cells[0].Value;
                if (row != null)
                {
                    string market = row.ToString();
                    if (CurrentSetting.ListPrices.Any())
                    {
                        var item = CurrentSetting.ListPrices.FirstOrDefault(x => x.Symbol == market);
                        if (item != null)
                        {
                            CurrentSetting.ListPrices.Remove(item);
                            SettingHelper.Save(CurrentSetting);
                            GetSetting();
                        }
                    }
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var row = dataGridView1.SelectedRows[0].Cells[0].Value;
            if (row != null)
            {
                string market = row.ToString();
                if (CurrentSetting.ListPrices.Any())
                {
                    var item = CurrentSetting.ListPrices.FirstOrDefault(x => x.Symbol == market);
                    if (item != null)
                    {
                        txtAbove.Text = item.Above.ToString();
                        txtBelow.Text = item.Below.ToString();
                        btnSaveChange.Tag = item.Symbol;
                        cbbListSymbols.SelectedValue = item.Symbol;
                    }
                }
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void lblCmc_Click(object sender, EventArgs e)
        {
            GetCoinmarketcap();
        }
    }
}