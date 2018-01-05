using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace NetworkCheckersWinForms
{
    public partial class StartClientForm : Form
    {
        private Connection connect;
        public StartClientForm(Connection c)
        {
            InitializeComponent();
            connect = c;
            tbHost.Text = "127.0.0.1";
            tbPort.Text = "3567";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                connect.Address = IPAddress.Parse(tbHost.Text);
                connect.Port = int.Parse(tbPort.Text);
            }
            catch (Exception e1) { MessageBox.Show(e1.Message); }
            this.DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
