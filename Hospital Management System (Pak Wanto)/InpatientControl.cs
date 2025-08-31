using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hospital_Management_System__Pak_Wanto_
{
    public partial class InpatientControl : UserControl
    {
        public InpatientControl()
        {
            InitializeComponent();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            guna2Button1.Checked = true;
            guna2Button2.Checked = false;

            inpatientControl21.Visible = true;
            roomsControl1.Visible = false;
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            guna2Button1.Checked = false;
            guna2Button2.Checked = true;

            inpatientControl21.Visible = false;
            roomsControl1.Visible = true;
        }

        private void InpatientControl_Load(object sender, EventArgs e)
        {
            inpatientControl21.Visible = true;
            roomsControl1.Visible = false;

            guna2Button1.Checked = true;
            guna2Button2.Checked = false;
        }
    }
}
