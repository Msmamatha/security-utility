using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace WirelessNodeSimulation
{
    public partial class ALERT : Form
    {
        public ALERT(string ss)
        {
            InitializeComponent();
            change(ss);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
       public void change(string ss)
        {
            foreach (string pinfo in ss.Split('^'))
            {
                int i = 0;
                ListViewItem it = new ListViewItem();
                foreach (string temp in pinfo.Split('&'))
                {
                    if (i == 0)
                        it.Text = temp;
                    else if (i == 1)
                        it.SubItems.Add(temp);
                    else if (i == 2)
                        it.SubItems.Add(temp);

                    i++;
                }
                listView1.Items.Add(it);
            }
        }
        

        private void ALERT_Load(object sender, EventArgs e)
        {

        }
        static bool fl = false;
        private void timer1_Tick(object sender, EventArgs e)
        {
            string pth = Application.StartupPath.Substring(0,Application.StartupPath.IndexOf("bin"));
            if (fl)
            {
                fl = false;
                pictureBox1.ImageLocation = pth + "\\Resources\\Earth Alert.png";
                pictureBox1.Refresh();
            }
            else
            {
                fl = true;
                pictureBox1.ImageLocation = pth + "\\Resources\\Earth Alert1.png";
                pictureBox1.Refresh();
            }

        }
    }
}