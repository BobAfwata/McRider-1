using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Management;
using ResourceriderImage = McRider.Windows.Properties.Resources;

namespace McRider.Windows
{
    public partial class SignupForm2 : Form
    {
        Image firsimage = null;
        Image secondimage = null;
        int firstAvatorIndex = -1, secondAvatorIndex = -1;

        public static void SetDoubleBuffered(System.Windows.Forms.Control c)
        {

            if (c == null || System.Windows.Forms.SystemInformation.TerminalServerSession)
                return;

            System.Reflection.PropertyInfo aProp = typeof(System.Windows.Forms.Control).GetProperty("DoubleBuffered",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
            aProp.SetValue(c, true, null);

            foreach (var c1 in c.Controls)
                SetDoubleBuffered(c1 as Control);
        }

        public SignupForm2()
        {
            InitializeComponent();

            foreach (var c in Controls)
                SetDoubleBuffered(c as Control);

            this.button9.FlatAppearance.BorderSize = 0;
            this.button9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button9.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button9.ForeColor = System.Drawing.Color.White;
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }
        private void resetFirst()
        {
            button10.BackgroundImage = ResourceriderImage.Avatar_female_11;
            button1.BackgroundImage = ResourceriderImage.Avatar_female_2;
            button2.BackgroundImage = ResourceriderImage.Avatar_male_1;
            button3.BackgroundImage = ResourceriderImage.Avatar_male_2;
        }
        private void resetSecond()
        {
            button5.BackgroundImage = ResourceriderImage.Avatar_female_11;
            button6.BackgroundImage = ResourceriderImage.Avatar_male_1;
            button7.BackgroundImage = ResourceriderImage.Avatar_female_2;
            button8.BackgroundImage = ResourceriderImage.Avatar_male_2;
        }
        private void button9_Click(object sender, EventArgs e)
        {
            if (customTextBox1.Text.Trim().ToString().Length < 1)
            {
                MessageBox.Show("Enter First Name");
                return;
            }

            if (customTextBox2.Text.Trim().ToString().Length < 1)
            {
                MessageBox.Show("Enter First Email");
                return;
            }


            if (customTextBox3.Text.Trim().ToString().Length < 1)
            {
                MessageBox.Show("Enter second  Name");
                return;
            }

            if (customTextBox4.Text.Trim().ToString().Length < 1)
            {
                MessageBox.Show("Enter Second Email");
                return;
            }

            if (firsimage == null)
            {
                MessageBox.Show("Select First Image");
                return;
            }
            if (secondimage == null)
            {
                MessageBox.Show("Select Second Image");
                return;
            }
                        
            var my_jsondata = new
            {
                name1 = @customTextBox1.Text.Trim().ToString(),
                email1 = customTextBox2.Text.Trim().ToString(),
                name2 = customTextBox3.Text.Trim().ToString(),
                email2 = customTextBox4.Text.Trim().ToString(),
                location = comboBox1.Text.ToString(),
                port = comboBox2.Text.ToString()
            };

            String filename = customTextBox3.Text.ToString() + "" + customTextBox1.Text.ToString() + ".json";
            String json_data = JsonConvert.SerializeObject(my_jsondata);


            File.WriteAllText("profiles/" + filename, json_data);
            var myForm = new PlayForm2(filename, firsimage, secondimage);
            myForm.Show();
            myForm.Tag = this;
            this.Hide();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (firstAvatorIndex == 0)//Uncheck
            {
                firsimage = null;
                firstAvatorIndex = -1;
                button10.BackgroundImage = McRider.Windows.Properties.Resources.Avatar_female_11;
            }
            else //Check
            {
                this.resetFirst();
                firsimage = button10.BackgroundImage;
                firstAvatorIndex = 0;
                button10.BackgroundImage = McRider.Windows.Properties.Resources.Avatar_female_1_checked;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (firstAvatorIndex == 1)//Uncheck
            {
                firsimage = null;
                firstAvatorIndex = -1;
                button2.BackgroundImage = McRider.Windows.Properties.Resources.Avatar_male_1;
            }
            else //Check
            {
                this.resetFirst();
                firsimage = button2.BackgroundImage;
                firstAvatorIndex = 1;
                button2.BackgroundImage = McRider.Windows.Properties.Resources.Avatar__male_1_checked;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (firstAvatorIndex == 2)//Uncheck
            {
                firsimage = null;
                firstAvatorIndex = -1;
                button1.BackgroundImage = McRider.Windows.Properties.Resources.Avatar_female_2;
            }
            else //Check
            {
                this.resetFirst();
                firsimage = button1.BackgroundImage;
                firstAvatorIndex = 2;
                button1.BackgroundImage = McRider.Windows.Properties.Resources.Avatar_female_2_checked;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (firstAvatorIndex == 3)//Uncheck
            {
                firsimage = null;
                firstAvatorIndex = -1;
                button3.BackgroundImage = McRider.Windows.Properties.Resources.Avatar_male_2;
            }
            else //Check
            {
                this.resetFirst();
                firsimage = button3.BackgroundImage;
                firstAvatorIndex = 3;
                button3.BackgroundImage = McRider.Windows.Properties.Resources.Avatar_male_2_checked;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (secondAvatorIndex == 0)//Uncheck
            {
                secondimage = null;
                secondAvatorIndex = -1;
                button5.BackgroundImage = McRider.Windows.Properties.Resources.Avatar_female_11;
            }
            else //Check
            {
                this.resetSecond();
                secondimage = button5.BackgroundImage;
                secondAvatorIndex = 0;
                button5.BackgroundImage = McRider.Windows.Properties.Resources.Avatar_female_1_checked;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (secondAvatorIndex == 1)//Uncheck
            {
                secondimage = null;
                secondAvatorIndex = -1;
                button6.BackgroundImage = McRider.Windows.Properties.Resources.Avatar_male_1;
            }
            else //Check
            {
                this.resetSecond();
                secondimage = button6.BackgroundImage;
                secondAvatorIndex = 1;
                button6.BackgroundImage = McRider.Windows.Properties.Resources.Avatar__male_1_checked;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (secondAvatorIndex == 2)//Uncheck
            {
                secondimage = null;
                secondAvatorIndex = -1;
                button7.BackgroundImage = McRider.Windows.Properties.Resources.Avatar_female_2;
            }
            else //Check
            {
                this.resetSecond();
                secondimage = button7.BackgroundImage;
                secondAvatorIndex = 2;
                button7.BackgroundImage = McRider.Windows.Properties.Resources.Avatar_female_2_checked;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (secondAvatorIndex == 3)//Uncheck
            {
                secondimage = null;
                secondAvatorIndex = -1;
                button8.BackgroundImage = McRider.Windows.Properties.Resources.Avatar_male_2;
            }
            else //Check
            {
                this.resetSecond();
                secondimage = button8.BackgroundImage;
                secondAvatorIndex = 3;
                button8.BackgroundImage = McRider.Windows.Properties.Resources.Avatar_male_2_checked;
            }
        }

        private void SignupForm2_Load(object sender, EventArgs e)
        {
            AutodetectArduinoPort();
        }
        private string AutodetectArduinoPort()
        {
            ManagementScope connectionScope = new ManagementScope();
            SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_SerialPort");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(connectionScope, serialQuery);
            
            try
            {
                comboBox2.Items.Clear();
                dynamic last = null;

                foreach (ManagementObject item in searcher.Get())
                {
                    string desc = item["Description"].ToString();
                    string deviceId = item["DeviceID"].ToString();
                    last = new { deviceId, deviceDesc = desc };
                    comboBox2.Items.Add(last);
                }

                comboBox2.Text = (string)last.deviceId;
            }
            catch (ManagementException e)
            {
                /* Do Nothing */
            }

            return null;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //  AutodetectArduinoPort();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            AutodetectArduinoPort();
        }
    }
}

