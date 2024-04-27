using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Management;

namespace McRider.Windows
{
    public partial class ProfilesForm : Form
    {
        public ProfilesForm()
        {
            InitializeComponent();
           
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, pictureBox2.Width - 3, pictureBox2.Height - 3);
            Region rg = new Region(gp);
            pictureBox2.Region = rg;
            button5.ForeColor = Color.FromArgb(61,125,183);
            button5.FlatAppearance.BorderSize = 0;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var myForm = new Splash2();
            myForm.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            // fill the screen
            this.Bounds = Screen.PrimaryScreen.Bounds;
        }



        //remove this
        private void button5_Click(object sender, EventArgs e)
        {
            var myForm = new SignupForm();
            myForm.Show();
            this.Hide();
        }
        private string AutodetectArduinoPort()
        {
            ManagementScope connectionScope = new ManagementScope();
            SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_SerialPort");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(connectionScope, serialQuery);


            try
            {
                foreach (ManagementObject item in searcher.Get())
                {
                 
                    string desc = item["Description"].ToString();
                    string deviceId = item["DeviceID"].ToString();
          
                   
                    comboBox1.Items.Add(deviceId);
                }
            }
            catch (ManagementException e)
            {
                /* Do Nothing */
            }

            return null;
        }


        private void ProfilesForm_Load(object sender, EventArgs e)
        {

            AutodetectArduinoPort();





            int k1 = 0;
          
                       DirectoryInfo dir4 = new DirectoryInfo(@"profiles");
            string[] title =new String[ dir4.GetFiles().Length];
            foreach (FileInfo file3 in dir4.GetFiles())
            {
               
                try
                {
                    String myjson = File.ReadAllText( file3.FullName);
                    JObject json_object = JObject.Parse(myjson);

                    //Print the parsed Json object
                    
                    String A = (string)json_object["session1"];
                    //title[ko] = file.Name;
                    if(A== "Rider")
                    {
                        this.imageList1.Images.Add(Image.FromFile("CyclistIcon.png"));
                    }
                    if (A == "Runner")
                    {
                        this.imageList1.Images.Add(Image.FromFile("Runnercon.png"));
                    }
                    title[k1] = file3.Name.ToString();
                    k1 = k1 + 1;
                }
                catch(Exception rt)
                {
                   
                    Console.WriteLine("This is not an image file");
                }
            }
            this.listView1.View = View.LargeIcon;
            this.imageList1.ImageSize = new Size(200, 200);
            this.listView1.LargeImageList = this.imageList1;
            //or
            //this.listView1.View = View.SmallIcon;
            //this.listView1.SmallImageList = this.imageList1;

            //int ko = 0;
            //DirectoryInfo dir2 = new DirectoryInfo(@"profiles");
            //foreach (FileInfo file2 in dir2.GetFiles())
            //{

            //    try
            //    {
            //        String myjson = File.ReadAllText(file2.FullName);
            //        JObject json_object = JObject.Parse(myjson);

            //        //Print the parsed Json object
            //        ListViewItem item = new ListViewItem();
            //        item.ImageIndex = ko;
            //        this.listView1.Items.Add(file2.Name, item.ImageIndex);
            //        this.listView1.Items.Add(item);

            //        ko = ko + 1;
            //    }
            //    catch (Exception rt)
            //    {

            //        Console.WriteLine("This is not an image file");
            //    }
            //}

            for (int j = 0; j < this.imageList1.Images.Count; j++)
            {
                ListViewItem item = new ListViewItem();
                item.ImageIndex = j;
                this.listView1.Items.Add(title[j], item.ImageIndex);
                //this.listView1.Items.Add(item);
            }

            //Graphics g = Graphics.FromHwnd(this.Handle);

            //ImageList photoList = new ImageList();
            //photoList.TransparentColor = Color.Blue;
            //photoList.ColorDepth = ColorDepth.Depth32Bit;
            //photoList.ImageSize = new Size(200, 200);

            //photoList.Images.Add(Image.FromFile(@"profiles/CyclistIcon.png"));
            //photoList.Images.Add(Image.FromFile(@"profiles/Runnercon.png"));


            //for (int count = 0; count < photoList.Images.Count; count++)
            //{
            //    photoList.Draw(g, new Point(20, 20), count);

            //    // Paint the form and wait to load the image 
            //    Application.DoEvents();
            //    System.Threading.Thread.Sleep(1000);
            //}
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            String foo = (String)listView1.SelectedItems[0].Text.ToString();

            var myForm = new PlayForm(foo);
            myForm.Show();
            this.Hide();
        }
    }
}
