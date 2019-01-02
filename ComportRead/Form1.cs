using System;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO.Ports;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace ComportRead
{
    public partial class Form1 : Form
    {
        String datapath = Path.GetDirectoryName(Application.UserAppDataPath); //設定檔路徑
        string[] ports = SerialPort.GetPortNames();
        setup f2 = new setup();
        List<string> discotype = new List<string>();
        List<string> asecode = new List<string>();
        DataTable dt;
        DataRow dr;
        
        
        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = true;
            this.Text = "Unitech Scaner v" + "1.0.5";
#if DEBUG
            this.Text = this.Text + "(D)";
#endif
            this.ResumeLayout(false);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (string port in ports)
            {
                ComportName.Items.Add(port);
            }

            f2.Show();
            f2.Hide();

            try
            {
                streamdata();
            }
            catch (Exception ex)
            {
                MessageBox.Show("pls creat data !");
            }
        }

        private void _port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(Convert.ToInt32(txtThreadTime.Text));
            if (this.serialPort1.BytesToRead > 0)
            {
                string data = serialPort1.ReadExisting();
                BeginInvoke((Action)delegate { CopyPaste(data); });
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {

            if (ComportName.SelectedItem != null)
            {
                if (radNormal.Checked == false && radSpecialCaptrue.Checked == false)
                {
                    MessageBox.Show("請選擇Scan Mode");
                }
                else
                {
                    serialPort1.PortName = ComportName.SelectedItem.ToString();
                    if (serialPort1.IsOpen)
                    {
                        serialPort1.Close();
                    }
                    serialPort1.Open();
                    serialPort1.DtrEnable = true;
                    //true 表示啟用 Data Terminal Ready (DTR)，否則為 false。
                    serialPort1.DataReceived += new SerialDataReceivedEventHandler(_port_DataReceived);

                    if (serialPort1.IsOpen)
                    {
                        btnConnect.Enabled = false;
                        btnSetup.Enabled = false;
                    }
                }
            }
            else
            {
                MessageBox.Show("請選擇連線comport");
            }
        }

        private void CopyPaste(string data)
        {
            if (radSpecialCaptrue.Checked)
            {
                if (data.Length == 15 && data.Substring(0, 1) == "H")
                {
                    KBsimulation(data, "{ENTER}");
                }
                else
                {
                    for (int i = 0; i < asecode.Count; i++)
                    {
                        if (data.IndexOf(asecode[i].ToString()) >= 0)
                        {
                            data = asecode[i].ToString();
                            break;
                        }
                    }
                    try
                    {
                        dt = new DataTable();
                        dt.Columns.Add("input", typeof(string));
                        dt.Columns.Add("output", typeof(string));

                        for (int i = 0; i < discotype.Count; i++)
                        {
                            dr = dt.NewRow();
                            dr["input"] = asecode[i].ToString();
                            dr["output"] = discotype[i].ToString();
                            dt.Rows.Add(dr);
                        }

                        object result = dt.Select("input ='" + data + "'")[0]["output"].ToString();
                        string datadelete = result.ToString();

                        KBsimulation(datadelete, "{ENTER}");

                    }
                    catch (TimeoutException) { return; }
                    catch (InvalidOperationException) { return; }
                    catch (Exception ex)
                    {
                        MessageBox.Show("該號碼不存在!!");
                       
                    }
                }
            }
            else if (radNormal.Checked)
            {           
                KBsimulation(data, "{ENTER}");              
            }
        }

        private void KBsimulation(string InputData, string inputkey)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                txtLabel.AppendText(InputData);
            }
            else
            {
                Clipboard.SetDataObject(InputData, true);
                keybd_event(0x11, 0, 0, 0);
                keybd_event(0x56, 0, 0, 0);
                keybd_event(0x56, 0, 2, 0);
                keybd_event(0x11, 0, 2, 0);

                Thread.Sleep(50);

                SendKeys.Send(inputkey);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (MessageBox.Show(this, "Are you sure to exit?", "UWedge", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
            if (serialPort1 != null) serialPort1.Close();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.notifyIcon1.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        private void btnSetup_Click(object sender, EventArgs e)
        {
            f2.Show();
        }

        private void streamdata()
        {
            StreamReader sb = File.OpenText(Path.Combine(this.datapath, "disco_type.txt"));
            for (int i = 0; i < f2.txtDISCOtype.Lines.Length; i++)
            {
                discotype.Add(sb.ReadLine());
            }
            sb.Close();

            StreamReader sc = File.OpenText(Path.Combine(this.datapath, "ase_code.txt"));
            for (int i = 0; i < f2.txtEncodingtype.Lines.Length; i++)
            {
                asecode.Add(sc.ReadLine());
            }
            sc.Close();
        }
       

        #region DataChange
        private string ReadCString(Encoding encoding, byte[] cString)
        {
            var nullIndex = Array.IndexOf(cString, (byte)0);
            nullIndex = (nullIndex == -1) ? cString.Length : nullIndex;
            return encoding.GetString(cString, 0, nullIndex);
        }

        private byte[] StringToByteArray(string hexString)
        {
            hexString = hexString.Replace("\r", "");
            hexString = hexString.Replace("\n", "");
            int nBytes = (hexString.Length) / 2;
            byte[] buffer = new byte[nBytes];
            for (int i = 0; i < nBytes; i++)
            {
                buffer[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return buffer;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            dt = new DataTable();
            dt.Columns.Add("input", typeof(string));
            dt.Columns.Add("output", typeof(string));

            for (int i = 0; i < discotype.Count; i++)
            {
                dr = dt.NewRow();
                dr["input"] = asecode[i].ToString();
                dr["output"] = discotype[i].ToString();
                dt.Rows.Add(dr);
            }
            dataGridView1.DataSource = dt;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            discotype.Clear();
            asecode.Clear();
            streamdata();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                btnConnect.Enabled = true;
                btnSetup.Enabled = true;
            }

        }
    }


}
