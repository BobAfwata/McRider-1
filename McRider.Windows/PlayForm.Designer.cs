namespace McRider.Windows
{
    partial class PlayForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlayForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.total_distance = new System.Windows.Forms.Label();
            this.total_time = new System.Windows.Forms.Label();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.total_progressbar = new System.Windows.Forms.ProgressBar();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblperson2 = new System.Windows.Forms.Label();
            this.lblperson1 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.player_two_distance = new System.Windows.Forms.Label();
            this.player_two_time = new System.Windows.Forms.Label();
            this.player_one_distance = new System.Windows.Forms.Label();
            this.player_one_time = new System.Windows.Forms.Label();
            this.pictureBox6 = new System.Windows.Forms.PictureBox();
            this.player_two_progressbar = new System.Windows.Forms.ProgressBar();
            this.player_one_progressbar = new System.Windows.Forms.ProgressBar();
            this.panel3 = new System.Windows.Forms.Panel();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.maximize_button = new System.Windows.Forms.Button();
            this.home_button = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).BeginInit();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.panel1.Controls.Add(this.total_distance);
            this.panel1.Controls.Add(this.total_time);
            this.panel1.Controls.Add(this.pictureBox4);
            this.panel1.Controls.Add(this.total_progressbar);
            this.panel1.Location = new System.Drawing.Point(25, 71);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(400, 49);
            this.panel1.TabIndex = 3;
            // 
            // total_distance
            // 
            this.total_distance.AutoSize = true;
            this.total_distance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.total_distance.Location = new System.Drawing.Point(258, 13);
            this.total_distance.Name = "total_distance";
            this.total_distance.Size = new System.Drawing.Size(65, 13);
            this.total_distance.TabIndex = 11;
            this.total_distance.Text = "D: 000 km";
            this.total_distance.Click += new System.EventHandler(this.label5_Click);
            // 
            // total_time
            // 
            this.total_time.AutoSize = true;
            this.total_time.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.total_time.Location = new System.Drawing.Point(60, 13);
            this.total_time.Name = "total_time";
            this.total_time.Size = new System.Drawing.Size(89, 13);
            this.total_time.TabIndex = 10;
            this.total_time.Text = "T: 00 h 00 min";
            // 
            // pictureBox4
            // 
            this.pictureBox4.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox4.BackgroundImage")));
            this.pictureBox4.Image = global::McRider.Windows.Properties.Resources.location;
            this.pictureBox4.Location = new System.Drawing.Point(10, 4);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(39, 39);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox4.TabIndex = 8;
            this.pictureBox4.TabStop = false;
            this.pictureBox4.Click += new System.EventHandler(this.pictureBox4_Click);
            // 
            // total_progressbar
            // 
            this.total_progressbar.Location = new System.Drawing.Point(63, 29);
            this.total_progressbar.Name = "total_progressbar";
            this.total_progressbar.Size = new System.Drawing.Size(325, 10);
            this.total_progressbar.TabIndex = 9;
            // 
            // panel2
            // 
            this.panel2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.panel2.Controls.Add(this.lblperson2);
            this.panel2.Controls.Add(this.lblperson1);
            this.panel2.Controls.Add(this.pictureBox2);
            this.panel2.Controls.Add(this.player_two_distance);
            this.panel2.Controls.Add(this.player_two_time);
            this.panel2.Controls.Add(this.player_one_distance);
            this.panel2.Controls.Add(this.player_one_time);
            this.panel2.Controls.Add(this.pictureBox6);
            this.panel2.Controls.Add(this.player_two_progressbar);
            this.panel2.Controls.Add(this.player_one_progressbar);
            this.panel2.Location = new System.Drawing.Point(25, 136);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(400, 120);
            this.panel2.TabIndex = 4;
            // 
            // lblperson2
            // 
            this.lblperson2.AutoSize = true;
            this.lblperson2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblperson2.Location = new System.Drawing.Point(19, 107);
            this.lblperson2.Name = "lblperson2";
            this.lblperson2.Size = new System.Drawing.Size(15, 13);
            this.lblperson2.TabIndex = 18;
            this.lblperson2.Text = "--";
            // 
            // lblperson1
            // 
            this.lblperson1.AutoSize = true;
            this.lblperson1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblperson1.Location = new System.Drawing.Point(19, 50);
            this.lblperson1.Name = "lblperson1";
            this.lblperson1.Size = new System.Drawing.Size(15, 13);
            this.lblperson1.TabIndex = 17;
            this.lblperson1.Text = "--";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(10, 4);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(47, 39);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 16;
            this.pictureBox2.TabStop = false;
            // 
            // player_two_distance
            // 
            this.player_two_distance.AutoSize = true;
            this.player_two_distance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player_two_distance.Location = new System.Drawing.Point(258, 73);
            this.player_two_distance.Name = "player_two_distance";
            this.player_two_distance.Size = new System.Drawing.Size(65, 13);
            this.player_two_distance.TabIndex = 15;
            this.player_two_distance.Text = "D: 000 km";
            // 
            // player_two_time
            // 
            this.player_two_time.AutoSize = true;
            this.player_two_time.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player_two_time.Location = new System.Drawing.Point(63, 73);
            this.player_two_time.Name = "player_two_time";
            this.player_two_time.Size = new System.Drawing.Size(89, 13);
            this.player_two_time.TabIndex = 14;
            this.player_two_time.Text = "T: 00 h 00 min";
            // 
            // player_one_distance
            // 
            this.player_one_distance.AutoSize = true;
            this.player_one_distance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player_one_distance.Location = new System.Drawing.Point(258, 11);
            this.player_one_distance.Name = "player_one_distance";
            this.player_one_distance.Size = new System.Drawing.Size(65, 13);
            this.player_one_distance.TabIndex = 13;
            this.player_one_distance.Text = "D: 000 km";
            // 
            // player_one_time
            // 
            this.player_one_time.AutoSize = true;
            this.player_one_time.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player_one_time.Location = new System.Drawing.Point(63, 11);
            this.player_one_time.Name = "player_one_time";
            this.player_one_time.Size = new System.Drawing.Size(89, 13);
            this.player_one_time.TabIndex = 12;
            this.player_one_time.Text = "T: 00 h 00 min";
            // 
            // pictureBox6
            // 
            this.pictureBox6.Location = new System.Drawing.Point(10, 66);
            this.pictureBox6.Name = "pictureBox6";
            this.pictureBox6.Size = new System.Drawing.Size(47, 39);
            this.pictureBox6.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox6.TabIndex = 10;
            this.pictureBox6.TabStop = false;
            // 
            // player_two_progressbar
            // 
            this.player_two_progressbar.Location = new System.Drawing.Point(63, 89);
            this.player_two_progressbar.Name = "player_two_progressbar";
            this.player_two_progressbar.Size = new System.Drawing.Size(325, 10);
            this.player_two_progressbar.TabIndex = 5;
            // 
            // player_one_progressbar
            // 
            this.player_one_progressbar.BackColor = System.Drawing.Color.White;
            this.player_one_progressbar.Location = new System.Drawing.Point(63, 27);
            this.player_one_progressbar.Name = "player_one_progressbar";
            this.player_one_progressbar.Size = new System.Drawing.Size(325, 10);
            this.player_one_progressbar.TabIndex = 2;
            // 
            // panel3
            // 
            this.panel3.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.panel3.Controls.Add(this.pictureBox5);
            this.panel3.Location = new System.Drawing.Point(25, 273);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(400, 371);
            this.panel3.TabIndex = 5;
            // 
            // pictureBox5
            // 
            this.pictureBox5.Image = global::McRider.Windows.Properties.Resources.bike;
            this.pictureBox5.Location = new System.Drawing.Point(10, 13);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(378, 375);
            this.pictureBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox5.TabIndex = 0;
            this.pictureBox5.TabStop = false;
            // 
            // button3
            // 
            this.button3.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button3.Location = new System.Drawing.Point(25, 675);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(40, 38);
            this.button3.TabIndex = 6;
            this.button3.Text = "<";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button4.Location = new System.Drawing.Point(387, 677);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(38, 35);
            this.button4.TabIndex = 7;
            this.button4.Text = ">";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // maximize_button
            // 
            this.maximize_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.maximize_button.BackgroundImage = global::McRider.Windows.Properties.Resources.hamburger;
            this.maximize_button.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.maximize_button.Location = new System.Drawing.Point(403, 13);
            this.maximize_button.Name = "maximize_button";
            this.maximize_button.Size = new System.Drawing.Size(22, 22);
            this.maximize_button.TabIndex = 8;
            this.maximize_button.UseVisualStyleBackColor = true;
            this.maximize_button.Click += new System.EventHandler(this.button2_Click);
            // 
            // home_button
            // 
            this.home_button.BackgroundImage = global::McRider.Windows.Properties.Resources.icon_home;
            this.home_button.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.home_button.Location = new System.Drawing.Point(25, 13);
            this.home_button.Name = "home_button";
            this.home_button.Size = new System.Drawing.Size(24, 23);
            this.home_button.TabIndex = 1;
            this.home_button.UseVisualStyleBackColor = true;
            this.home_button.Click += new System.EventHandler(this.button1_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = global::McRider.Windows.Properties.Resources.main_background;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(450, 726);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // timer2
            // 
            this.timer2.Enabled = true;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // PlayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 726);
            this.Controls.Add(this.maximize_button);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.home_button);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PlayForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PlayForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.PlayForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).EndInit();
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button home_button;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label total_distance;
        private System.Windows.Forms.Label total_time;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.ProgressBar total_progressbar;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ProgressBar player_two_progressbar;
        private System.Windows.Forms.ProgressBar player_one_progressbar;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.PictureBox pictureBox6;
        private System.Windows.Forms.Label player_two_distance;
        private System.Windows.Forms.Label player_two_time;
        private System.Windows.Forms.Label player_one_distance;
        private System.Windows.Forms.Label player_one_time;
        private System.Windows.Forms.Button maximize_button;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Label lblperson2;
        private System.Windows.Forms.Label lblperson1;
    }
}