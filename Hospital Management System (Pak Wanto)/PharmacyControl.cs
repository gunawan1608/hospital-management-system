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
    public partial class PharmacyControl : UserControl
    {
        public PharmacyControl()
        {
            InitializeComponent();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            guna2Button1.Checked = true;
            guna2Button2.Checked = false;
            guna2Button3.Checked = false;

            medicineControl1.Visible = true;
            prescriptionControl1.Visible = false;
            detailsControl1.Visible = false;
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            guna2Button1.Checked = false;
            guna2Button2.Checked = true;
            guna2Button3.Checked = false;

            medicineControl1.Visible = false;
            prescriptionControl1.Visible = true;
            detailsControl1.Visible = false;
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            guna2Button1.Checked = false;
            guna2Button2.Checked = false;
            guna2Button3.Checked = true;

            medicineControl1.Visible = false;
            prescriptionControl1.Visible = false;
            detailsControl1.Visible = true;
        }

        private void PharmacyControl_Load(object sender, EventArgs e)
        {
            guna2Button1.Checked = true;
            guna2Button2.Checked = false;
            guna2Button3.Checked = false;

            medicineControl1.Visible = true;
            prescriptionControl1.Visible = false;
            detailsControl1.Visible = false;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
