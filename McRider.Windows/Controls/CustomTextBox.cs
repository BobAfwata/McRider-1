using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace McRider.Windows.Controls
{
    public partial class CustomTextBox : TextBox
    {
   

        public CustomTextBox()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            BackColor = Color.Transparent;
            BorderStyle = BorderStyle.None;
            AutoSize = false;


            Controls.Add(new System.Windows.Forms.Label() {
                Height = 2,
                BackColor = System.Drawing.Color.White,
                Dock = System.Windows.Forms.DockStyle.Bottom,
            });
        }        
    }
}
