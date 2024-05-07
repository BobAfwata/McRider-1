//using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web.Script.Serialization;
using System.Windows.Forms;


namespace McRider.Windows
{
    public partial class PlayForm : Form
    {
        //object serialport to listen usb
        System.IO.Ports.SerialPort Port;

        //variable to check if arduino is connect
        bool IsClosed = false;
        Double MYTIME = 0;
        //DispatcherTimer dispatcherTimer = new DispatcherTimer();

        public PlayForm(String filename)
        {
            InitializeComponent();
            player_two_distance.ForeColor = player_two_time.ForeColor = player_one_distance.ForeColor = player_one_time.ForeColor =
            total_distance.ForeColor = total_time.ForeColor = Color.FromArgb(53, 90, 146);
            button4.Parent = pictureBox1;
            button4.BackColor = Color.Transparent;
            //configuration of arduino, you check if com3 is the port correct, 
            //in arduino ide you can make it
            Port = new System.IO.Ports.SerialPort();

            // automatically detect ports

            Port.PortName = "COM7";//AutodetectArduinoPort();//
            Port.BaudRate = 9600;
            Port.ReadTimeout = 5000; //500

            String myjson = File.ReadAllText("profiles/" + filename);
            JObject json_object = JObject.Parse(myjson);

            //Print the parsed Json object
            String A = (string)json_object["firstPerson"];
            String B = (string)json_object["secondPerson"];
            lblperson1.Text = A.ToUpper();
            lblperson2.Text = B.ToUpper();
            lblperson1.ForeColor = Color.FromArgb(53, 90, 146);
            lblperson2.ForeColor = Color.FromArgb(53, 90, 146);

            String g1 = (string)json_object["gender1"];
            String g2 = (string)json_object["gender2"];


            if (g1 == "Male")
            {
                pictureBox2.Image = Properties.Resources.Avatar_male;

            }
            if (g2 == "Male")
            {
                pictureBox6.Image = Properties.Resources.Avatar_male;
            }


            if (g1 == "Female")
            {
                pictureBox2.Image = Properties.Resources.Avatar_female_1;

            }
            if (g2 == "Female")
            {
                pictureBox6.Image = Properties.Resources.Avatar_female_1;
            }
            try
            {
                Port.Open();
            }
            catch { }
            finally
            {

                // BackgroundWorker worker = new BackgroundWorker();
                // worker.DoWork += Form1_Load;

                //  worker.RunWorkerAsync();
            }

        }

        private class MyDataMOdel
        {
            public string Id { get; set; }
            public string total_dist { get; set; }
            public string player_one_dist { get; set; } //distance
            public string player_two_dist { get; set; }

            public string total_tm { get; set; }
            public string player_one_tm { get; set; } //time
            public string player_two_tm { get; set; }

        }
        //public void FetchData()
        //{
        //    MessageBox.Show("boooom"+ IsClosed.ToString());
        //    //while (!IsClosed)
        //    //{
        //    //    //``pzxxxxz  Thread.Sleep(2000);
        //    //    //A Thread to listen forever the serial port
        //    //   // dispatcherTimer.Tick += new EventHandler(ListenSerial);
        //    //   // dispatcherTimer.Start();

        //    //}

        //    // Thread Hilo = new Thread(ListenSerial);

        //}
        private void ListenSerial()
        {
            timer2.Stop();
            try
            {

                //read to data from arduino
                //string AString = Port.ReadLine(); 
                // MessageBox.Show(AString);
                //var dta = JsonConvert.DeserializeObject<MyDataMOdel>(AString);
                //write the data in something textbox
                //tb_serial.Text = AString;
                //session_label.Text = AString;


                int i;

                //total 

                // string serial_data1 = @"{""distance1"":""50"",""time1"":""95"",""distance2"":""60"",""timer2"":""100""}";
                string serial_data1 = Port.ReadLine();
                //if (serial_data1.Contains("distance") == false)
                //{


                //}
                //else{
                //MessageBox.Show("true"+serial_data1);
                //}

                //var JSONObj1 = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(serial_data1);
                //var sModel1 = new JavaScriptSerializer().Deserialize<sData>(serial_data1);
                ////sData.name; 
                //sData.age; 


                JObject json_object = JObject.Parse(serial_data1.ToString());
                //MessageBox.Show(serial_data1.ToString());
                //Print the parsed Json object
                String A = (string)json_object["distance1"];



                player_one_progressbar.Minimum = 0;
                player_one_progressbar.Maximum = 5000;
                player_one_progressbar.Value = Convert.ToInt32(A);
                // player_one_progressbar.Value = Convert.ToInt32(AString);

                //player 2 data 

                String B = (string)json_object["distance2"];

                player_two_progressbar.Minimum = 0;
                player_two_progressbar.Maximum = 5000;
                player_two_progressbar.Value = Convert.ToInt32(B);
                // player_two_progressbar.Value = Convert.ToInt32(AString);


                total_progressbar.Minimum = 0;
                total_progressbar.Maximum = 10000;
                total_progressbar.Value = Convert.ToInt32(A) + Convert.ToInt32(B);
                //total_progressbar.Value = Convert.ToInt32(AString);
                // display the time and distance

                total_distance.Text = "D:" + total_progressbar.Value + "km";
                player_one_distance.Text = "D:" + player_one_progressbar.Value + "km";
                player_two_distance.Text = "D:" + player_two_progressbar.Value + "km";

                //total_time.Text = "T:"+AString+"h" + AString + "min";
                //player_one_time.Text ="T:"+AString+"h" + AString + "min";
                //player_two_time.Text ="T:"+AString+"h" + AString + "min";

                //for (i = 0; i <= 200; i++)
                //{
                //    progressBar1.Value = i;
                //}
            }
            catch (Exception er)
            {
                //   MessageBox.Show(er.ToString());
            }
            //dispatcherTimer.Stop();
            //var serial_data3 = @"{""distance1"":""50"",""time1"":""95"",""distance2"":""60"",""timer2"":""100""}";
            //var JSONObj3 = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(serial_data3);
            //var sModel3 = new JavaScriptSerializer().Deserialize<sData>(serial_data3);
            ////sData.name; 
            ////sData.age; 
            //MessageBox.Show(sModel3.distance1);

            timer2.Start();
        }
        class sData
        {
            public string distance1 { get; set; }
            public int time1 { get; set; }
            public string distance2 { get; set; }
            public int time2 { get; set; }
        }
        private void deserialize()
        {
            var serial_data1 = @"{""distance1"":""50"",""time1"":""95"",""distance2"":""60"",""timer2"":""100""}";
            var JSONObj1 = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(serial_data1);
            var sModel1 = new JavaScriptSerializer().Deserialize<sData>(serial_data1);
            //sData.name; 
            //sData.age; 
            MessageBox.Show(sModel1.distance1);
            //var serial_data = @"{""name"":""John Doe"",""age"":20}";
            //var JSONObj = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(serial_data);
            //var sModel = new JavaScriptSerializer().Deserialize<sData>(serial_data);
            ////sData.name; 
            ////sData.age; 
            //total_time.Text = sModel.name;

            //MessageBox.Show(sModel.name);


        }

        private void button3_Click(object sender, EventArgs e)
        {
            var myForm = new ProfilesForm();
            myForm.Show();
            this.Hide();
        }


        //private void LoadWinner(object sender, EventArgs e)
        //{
        //    var myForm = new ProfilesForm();
        //    myForm.Show();
        //    //this.Hide();
        //}


        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // load the home screen

            var myForm = new ProfilesForm();
            myForm.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //// on click maximixe
            //PlayForm.WindowState = FormWindowState.Normal;
            //PlayForm.FormBorderStyle = FormBorderStyle.None;
            //PlayForm.WindowState = FormWindowState.Maximized;
            // hide max,min and close button at top right of Window
            //this.FormBorderStyle = FormBorderStyle.None;
            //// fill the screen
            //this.Bounds = Screen.PrimaryScreen.Bounds;
            //this.WindowState = FormWindowState.Maximized;
            //this.MinimumSize = this.Size;
            //this.MaximumSize = this.Size;

            for (int i = 0; i < Application.OpenForms.Count; i++)
            {
                Application.OpenForms[i].WindowState = FormWindowState.Maximized;
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }


        private void PlayForm_Load(object sender, EventArgs e)
        {
            timer2.Start();
        }

        private void timer2_Tick(object sender, EventArgs e)

        {
            MYTIME = MYTIME + 1;

            if (MYTIME >= 600000)
            {
                timer2.Stop();
                var myForm = new Winner();
                myForm.Show();
                this.Hide();
            }

            if (!IsClosed)
            {
                ListenSerial();
            }

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }
    }
}
