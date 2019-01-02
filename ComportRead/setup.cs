using System;
using System.IO;
using System.Windows.Forms;

namespace ComportRead
{
    public partial class setup : Form
    {
        String datapath = Path.GetDirectoryName(Application.UserAppDataPath); //設定檔路徑

        public setup()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {

            StreamWriter sb = File.CreateText(Path.Combine(this.datapath, "disco_type.txt"));
            sb.Write(txtDISCOtype.Text);
            sb.Flush();
            sb.Close();

            StreamWriter sc = File.CreateText(Path.Combine(this.datapath, "ase_code.txt"));
            sc.Write(txtEncodingtype.Text);
            sc.Flush();
            sc.Close();

            MessageBox.Show("save succesed!!");

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void setup_Load(object sender, EventArgs e)
        {
            try
            {
                StreamReader sb = File.OpenText(Path.Combine(this.datapath, "disco_type.txt"));
                txtDISCOtype.Text = sb.ReadToEnd();
                sb.Close();

                StreamReader sc = File.OpenText(Path.Combine(this.datapath, "ase_code.txt"));
                txtEncodingtype.Text = sc.ReadToEnd();
                sc.Close();

                int count1 = txtEncodingtype.Lines.Length;
                int count2 = txtDISCOtype.Lines.Length;

                label1.Text = count1.ToString();
                label2.Text = count2.ToString();

                if (count1 == count2)
                {
                    label3.Text = "比數相符";
                }
                else { label3.Text = "比數不符"; }

            }
            catch (Exception EX)
            {
            }
        }

        private void txtEncodingtype_TextChanged(object sender, EventArgs e)
        {
            label1.Text = txtEncodingtype.Lines.Length.ToString();

        }

        private void txtDISCOtype_TextChanged(object sender, EventArgs e)
        {
            label2.Text = txtDISCOtype.Lines.Length.ToString();
        }
    }
}
