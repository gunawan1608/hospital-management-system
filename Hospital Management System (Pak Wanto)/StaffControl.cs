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
    public partial class StaffControl : UserControl
    {
        public StaffControl()
        {
            InitializeComponent();
        }

        private void StaffControl_Load(object sender, EventArgs e)
        {
            doctorControl1.Visible = true;
            nurseControl1.Visible = false;
            departmentControl1.Visible = false;

            guna2Button1.Checked = true;
            guna2Button2.Checked = false;
            guna2Button3.Checked = false;
        }

        private void guna2Button1_Click_1(object sender, EventArgs e)
        {
            guna2Button1.Checked = true;
            guna2Button2.Checked = false;
            guna2Button3.Checked = false;

            doctorControl1.Visible = true;
            nurseControl1.Visible = false;
            departmentControl1.Visible = false;
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            guna2Button1.Checked = false;
            guna2Button2.Checked = true;
            guna2Button3.Checked = false;

            doctorControl1.Visible = false;
            nurseControl1.Visible = true;
            departmentControl1.Visible = false;
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            guna2Button1.Checked = false;
            guna2Button2.Checked = false;
            guna2Button3.Checked = true;

            doctorControl1.Visible = false;
            nurseControl1.Visible = false;
            departmentControl1.Visible = true;
        }

        private void departmentControl1_Load(object sender, EventArgs e)
        {

        }
    }
}
