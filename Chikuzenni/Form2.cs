using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chikuzenni
{
    public partial class Form2 : Form
    {
        Form1 form1;
        public Form2(Form1 form)
        {
            form1 = form;
            this.FormClosing += Form2_FormClosing;
            InitializeComponent();
        }
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            form1.Show();
        }
        private void cmbPortName_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
            form1.PortButtton_Click(cmbPortName.SelectedItem.ToString());
            
        }
    }
}
