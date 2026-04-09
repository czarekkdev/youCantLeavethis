using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace youCantLeavethis
{
    public partial class youCantLeavethis : Form
    {
        public youCantLeavethis()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormClosing += new FormClosingEventHandler(on_close);
        }

        private void on_close(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            MessageBox.Show("Nah that aint gonna work lol");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult question = MessageBox.Show("Oh so you are gay?", "", MessageBoxButtons.YesNo);

            if (question == DialogResult.Yes) {
                MessageBox.Show("alright, remember to tell your parents so they can be disappointed");
                Program.fix();
            } 

            else
            {
                MessageBox.Show("Then why did you lie by clicking the button?");
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
