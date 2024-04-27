using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace McRider.Windows
{
    public partial class SignupForm : Form
    {
        public SignupForm()
        {
            InitializeComponent();
            panel1.Parent = button2.Parent = label1.Parent = pictureBox1;

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }



        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().ToString().Length < 1)
            {
                MessageBox.Show("Enter Name");
                return;
            }
            if (comboBox1.Text.Trim().ToString().Length < 1)
            {
                MessageBox.Show("Select Gender");
                return;
            }
            String session1 = "";
            if (radioButton1.Checked)
            {
                session1 = radioButton1.Text;
            }
            else if (radioButton2.Checked)
            {
                session1 = radioButton2.Text;
            }
            else
            {
                MessageBox.Show("Select Session");
                return;
            }


            if (dataGridView1.Rows.Count == 0)
            {
                string[] row = new string[] { textBox1.Text, comboBox1.Text, session1 };
                dataGridView1.Rows.Add(row);
            }
            else if (dataGridView1.Rows.Count == 1)
            {
                if (dataGridView1.Rows[0].Cells[2].Value.ToString() != session1)
                {
                    MessageBox.Show("The Sessions Must be the same");
                    return;
                }
                string[] row = new string[] { textBox1.Text, comboBox1.Text, session1 };
                dataGridView1.Rows.Add(row);
            }
            else
            {
                MessageBox.Show("we can only allow To people per session");
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 2)
            {
                var my_jsondata = new
                {
                    firstPerson = @dataGridView1.Rows[0].Cells[0].Value.ToString(),
                    gender1 = dataGridView1.Rows[0].Cells[1].Value.ToString(),
                    session1 = dataGridView1.Rows[0].Cells[2].Value.ToString(),
                    secondPerson = dataGridView1.Rows[1].Cells[0].Value.ToString(),
                    gender2 = dataGridView1.Rows[1].Cells[1].Value.ToString(),
                    session2 = dataGridView1.Rows[1].Cells[2].Value.ToString()
                };

                String filename = my_jsondata.firstPerson + "" + my_jsondata.secondPerson + ".json";
                String json_data = JsonConvert.SerializeObject(my_jsondata);

                File.WriteAllText("profiles/" + filename, json_data);
                var myForm = new PlayForm(filename);
                myForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Select two People");
            }


        }

        private void button6_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
        }

        private void SignupForm_Load(object sender, EventArgs e)
        {

            dataGridView1.Columns.Add("Name", "Name");
            dataGridView1.Columns.Add("Gender", "Gender");
            dataGridView1.Columns.Add("Session", "Session");
            dataGridView1.Rows.Clear();
        }
    }
}
public class User
{
    public string firstPerson { get; set; }
    public string gender1 { get; set; }
    public string session1 { get; set; }
    public string secondPerson { get; set; }
    public string gender2 { get; set; }
    public string session2 { get; set; }
}