using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using System.Windows.Forms;

namespace McRider.Windows
{
    public partial class Splash2 : Form
    {
        public Splash2()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Hide();
            timer1.Dispose();
            var myForm = new SignupForm2();
            myForm.Show();
            this.Hide();
        }

        private void lblTagline_Click(object sender, EventArgs e)
        {

        }

        private void Splash2_Load(object sender, EventArgs e)
        {
            SoundPlayer soundPlayer = new SoundPlayer("counter.wav");
            soundPlayer.Play();
        }
    }
}
