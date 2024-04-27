using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace McRider.Windows
{
    public partial class Winner : Form
    {
        private string filename;
        private Image playerImage;
        private string playerName;
        private double playerDistance;
        private Form parent;

        public Winner()
        {
            InitializeComponent();
        }

        public Winner(string filename, Image playerImage, string playerName, double playerDistance, Form parent = null)
            :this()
        {
            this.filename = filename;
            this.playerImage = playerImage;
            this.playerName = playerName;
            this.playerDistance = playerDistance;            
        }


        private void Winner_Load(object sender, EventArgs e)
        {            
            label2.Text = this.playerDistance.ToString("#,##0.00") + " KM";
            label3.Text = this.playerName;
            pictureBox1.BackgroundImage = this.playerImage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ((this.Tag as PlayForm2)?.Tag as SignupForm2)?.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ((this.Tag as PlayForm2)?.Tag as SignupForm2)?.Show();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
