using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Collections.Specialized;

namespace McRider.Windows
{
    public partial class PlayForm2 : Form
    {
        public static String distanceWinner = "";
        public static String winnername = "";
        System.IO.Ports.SerialPort Port;
        bool _we_are_running = true;

        double player1Distance = 0,
            player2Distance = 0,
            targetDistance = 2000,
            achievedDistance = 0;

        private Timer timerInnitialCountDown;

        public static void SetDoubleBuffered(System.Windows.Forms.Control c)
        {
            if (System.Windows.Forms.SystemInformation.TerminalServerSession)
                return;
            System.Reflection.PropertyInfo aProp = typeof(System.Windows.Forms.Control).GetProperty("DoubleBuffered",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aProp.SetValue(c, true, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        //DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public PlayForm2(String filename, Image player1, Image player2)
        {
            InitializeComponent();
            tableLayoutPanel4.Controls.SetChildIndex(label4, 0);
            Port = new System.IO.Ports.SerialPort();

            SetDoubleBuffered(tableLayoutPanel4);

            // automatically detect ports

            Port.BaudRate = 9600;
            Port.ReadTimeout = 5000; //500

            String myjson = File.ReadAllText("profiles/" + filename);
            JObject json_object = JObject.Parse(myjson);

            //Print the parsed Json object
            String A = (string)json_object["name1"];
            String B = (string)json_object["name2"];
            label2.Text = A.ToUpper();
            label1.Text = B.ToUpper();

            pictureBox2.BackgroundImage = player1;
            pictureBox3.BackgroundImage = player2;
            String myport = (string)json_object["port"];
            Port.PortName = myport;

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

            var configFile = "profiles/" + (string)json_object["location"] + ".json";
            JObject configObj = null;
            if (File.Exists(configFile))
            {
                myjson = File.ReadAllText(configFile);
                configObj = JObject.Parse(myjson);
            }
            else
            {
                configObj = JObject.FromObject(new
                {
                    dateCreated = DateTime.UtcNow,
                    dateModified = DateTime.UtcNow,
                    runTime = 10,
                    targetDistance = 500,
                    achievedDistance = 0,
                });

                File.WriteAllText(configFile, configObj.ToString());
            }
            var rumTime = 10.0;

            if (configObj["targetDistance"] != null)
                targetDistance = double.Parse(configObj["targetDistance"].ToString());
            if (configObj["achievedDistance"] != null)
                achievedDistance = double.Parse(configObj["achievedDistance"].ToString());
            if (configObj["rumTime"] != null)
                rumTime = double.Parse(configObj["rumTime"].ToString());

            this.timerInnitialCountDown = new Timer()
            {
                Interval = 1000,
                Enabled = true
            };

            var countDown = 4;

            var stopTime = DateTime.UtcNow.Add(TimeSpan.FromMinutes(rumTime));

            timerInnitialCountDown.Tick += (s, e) =>
            {
                if (countDown - 1 == 0)
                    label4.Text = "GO!!";
                else if (countDown > 0)
                    label4.Text = (countDown - 1).ToString();

                if (countDown >= 0)
                    countDown--;

                if (countDown <= 0)
                {
                    if (countDown == 0)
                    {
                        stopTime = DateTime.UtcNow.Add(TimeSpan.FromMinutes(rumTime));
                        this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        //tableLayoutPanel4.BackgroundImage = Properties.Resources.idleimage2;
                        Task.Run(() => DoSerial());
                    }
                    else
                    {
                        label7.Text = targetDistance.ToString("#,##0") + " KM";
                        label12.Text = (achievedDistance + player1Distance + player2Distance).ToString("#,##0.00") + " KM";
                        label5.Text = player1Distance.ToString("#,##0.000") + " KM";
                        label16.Text = player2Distance.ToString("#,##0.000") + " KM";

                        if (DateTime.UtcNow >= stopTime)
                        {
                            timerInnitialCountDown.Stop();
                            _we_are_running = false;

                            Winner winnerForm;

                            if (player1Distance > player2Distance)
                                winnerForm = new Winner(filename, player1, A, player1Distance);
                            else
                                winnerForm = new Winner(filename, player2, B, player2Distance);

                            winnerForm.Tag = this;
                            winnerForm.Show();
                            this.Hide();

                            if (false)
                                Task.Run(() =>
                                {
                                    string urlAddress = "http://18.219.167.242/wezesha_api/public/details/1";
                                    using (var client = new WebClient())
                                    {
                                        var postData = new NameValueCollection()
                                            {
                                               { "location", (string)json_object["location"] },  //order: {"parameter name", "parameter value"}
                                               { "name1", (string)json_object["name1"]},
                                               { "email1", (string)json_object["email1"] },  //order: {"parameter name", "parameter value"}
                                               { "distance1", player1Distance.ToString() },
                                               { "name2", (string)json_object["name2"] },  //order: {"parameter name", "parameter value"}
                                               { "email2", (string)json_object["email2"]},
                                               { "distance2", player2Distance.ToString()},
                                            };

                                        // client.UploadValues returns page's source as byte array (byte[])
                                        // so it must be transformed into a string
                                        string pagesource = Encoding.UTF8.GetString(client.UploadValues(urlAddress, postData));
                                        MessageBox.Show(pagesource);
                                    }
                                });
                        }
                        else
                        {
                            var remainingTime = DateTime.UtcNow - stopTime;
                            label4.Text = remainingTime.ToString(@"mm\:ss");
                        }


                        Task.Run(() =>
                        {
                            var jobject = JObject.FromObject(new
                            {
                                dateModified = DateTime.UtcNow,
                                targetDistance,
                                rumTime,
                                achievedDistance = achievedDistance + player1Distance + player2Distance,
                            });

                            File.WriteAllText(configFile, jobject.ToString());
                        });
                    }
                }
            };

        }

        private void FakeDoSerial()
        {
            while (_we_are_running)
            {
                System.Threading.Thread.Sleep(1000);
                player1Distance += 0.008;
                player2Distance += 0.006;
            }
        }


        public void DoSerial()
        {
            int start_counter_a = 0, start_counter_b = 0;

            while (_we_are_running)
            {
                try
                {
                    string message = Port.ReadLine();
                    // MessageBox.Show(message);
                    JObject json_object = JObject.Parse(message.ToString());
                    String A = (string)json_object["distance1"];
                    String B = (string)json_object["distance2"];

                    if (A != null)
                    {
                        int bike_a = Convert.ToInt32(A);
                        int bike_b = Convert.ToInt32(B);
                        if (start_counter_a == 0)
                        {
                            start_counter_a = bike_a;
                            start_counter_b = bike_b;
                        }
                        else
                        {
                            player1Distance = double.Parse(A) - start_counter_a / 1000;
                            player2Distance = double.Parse(B) - start_counter_b / 1000;
                        }
                    }
                    else
                    {
                        int bike_a = Convert.ToInt32(json_object["bikeA"]);
                        int bike_b = Convert.ToInt32(json_object["bikeB"]);


                        if (start_counter_a == 0)
                        {
                            start_counter_a = bike_a;
                            start_counter_b = bike_b;
                        }
                        else
                        {
                            var distance_a = 0.622 * (bike_a - start_counter_a);
                            var distance_b = 0.622 * (bike_b - start_counter_b);

                            player1Distance = distance_a / 1000;
                            player2Distance = distance_b / 1000;
                        }
                    }
                }
                catch
                {
                    //  MessageBox.Show(ex.Message);
                }
            }
        }

    }

}


