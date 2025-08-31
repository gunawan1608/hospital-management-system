using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Hospital_Management_System__Pak_Wanto_
{
    public partial class Main_Form : Form
    {
        SqlConnection conn = new SqlConnection("Data Source=LAPTOP-9SGQJSHQ\\SQLEXPRESS;Initial Catalog=DB_Hospital;Integrated Security=True;");
        private System.Windows.Forms.Timer timer;
        private string loggedInUser;

        public Main_Form(string userName)
        {
            InitializeComponent();
            SetupTimer();
            loggedInUser = userName; // Assign the logged-in user's name
            DisplayWelcomeMessage();
        }

        private void DisplayWelcomeMessage()
        {
            label2.Text = $"Welcome, {loggedInUser}";
        }

        private void SetupTimer()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000; // 1 detik
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateAndTime();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            guna2Panel3.Height = button8.Height;
            guna2Panel3.Top = button8.Top;

            // Tampilkan MessageBox untuk konfirmasi
            DialogResult result = MessageBox.Show("Apakah Anda yakin ingin keluar dari sesi ini?",
                                                  "Konfirmasi Sign Out",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question);

            // Periksa jawaban pengguna
            if (result == DialogResult.Yes)
            {
                // Tampilkan form login
                Login loginForm = new Login();
                loginForm.Show();

                // Tutup form utama (Main_Form)
                this.Close();
            }
        }


        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            guna2Panel3.Height = button1.Height;
            guna2Panel3.Top = button1.Top;
            homeControl1.Visible = true;
            patientControl1.Visible = false;
            appointmentControl1.Visible = false;
            staffControl1.Visible = false;
            inpatientControl1.Visible = false;
            pharmacyControl1.Visible = false;
            paymentControl1.Visible = false;
            medicalRecord1.Visible = false;
            diagnosesControl1.Visible = false;
        }

        private void guna2Panel3_Paint(object sender, PaintEventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            guna2Panel3.Height = button2.Height;
            guna2Panel3.Top = button2.Top;
            homeControl1.Visible = false;
            patientControl1.Visible = true;
            appointmentControl1.Visible = false;
            staffControl1.Visible = false;
            inpatientControl1.Visible = false;
            pharmacyControl1.Visible = false;
            paymentControl1.Visible = false;
            medicalRecord1.Visible = false;
            diagnosesControl1.Visible = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            guna2Panel3.Height = button3.Height;
            guna2Panel3.Top = button3.Top;
            homeControl1.Visible = false;
            patientControl1.Visible = false;
            appointmentControl1.Visible = true;
            staffControl1.Visible = false;
            inpatientControl1.Visible = false;
            pharmacyControl1.Visible = false;
            paymentControl1.Visible = false;
            medicalRecord1.Visible = false;
            diagnosesControl1.Visible = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            guna2Panel3.Height = button4.Height;
            guna2Panel3.Top = button4.Top;
            homeControl1.Visible = false;
            patientControl1.Visible = false;
            appointmentControl1.Visible = false;
            staffControl1.Visible = true;
            inpatientControl1.Visible = false;
            pharmacyControl1.Visible = false;
            paymentControl1.Visible = false;
            medicalRecord1.Visible = false;
            diagnosesControl1.Visible = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            guna2Panel3.Height = button5.Height;
            guna2Panel3.Top = button5.Top;
            homeControl1.Visible = false;
            patientControl1.Visible = false;
            appointmentControl1.Visible = false;
            staffControl1.Visible = false;
            inpatientControl1.Visible = true;
            pharmacyControl1.Visible = false;
            paymentControl1.Visible = false;
            medicalRecord1.Visible = false;
            diagnosesControl1.Visible = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            guna2Panel3.Height = button6.Height;
            guna2Panel3.Top = button6.Top;
            homeControl1.Visible = false;
            patientControl1.Visible = false;
            appointmentControl1.Visible = false;
            staffControl1.Visible = false;
            inpatientControl1.Visible = false;
            pharmacyControl1.Visible = true;
            paymentControl1.Visible = false;
            medicalRecord1.Visible = false;
            diagnosesControl1.Visible = false;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            guna2Panel3.Height = button7.Height;
            guna2Panel3.Top = button7.Top;
            homeControl1.Visible = false;
            patientControl1.Visible = false;
            appointmentControl1.Visible = false;
            staffControl1.Visible = false;
            inpatientControl1.Visible = false;
            pharmacyControl1.Visible = false;
            paymentControl1.Visible = true;
            medicalRecord1.Visible = false;
            diagnosesControl1.Visible = false;
        }

        private void label4_Click(object sender, EventArgs e)
        {
        }

        private void Main_Form_Load(object sender, EventArgs e)
        {
            UpdateDateAndTime();
            homeControl1.Visible = true;
            patientControl1.Visible = false;
            appointmentControl1.Visible = false;
            staffControl1.Visible= false;
            inpatientControl1.Visible = false;
            pharmacyControl1.Visible = false;
            paymentControl1.Visible = false;
            medicalRecord1.Visible = false;
            diagnosesControl1.Visible = false;
        }

        private void UpdateDateAndTime()
        {
            // Dapatkan waktu saat ini dengan zona waktu Indonesia (WIB)
            DateTime currentDateTime = DateTime.Now;

            // Format tanggal dan waktu dalam bahasa Indonesia
            string formattedDateTime = currentDateTime.ToString("dddd, dd MMMM yyyy HH:mm:ss",
                new System.Globalization.CultureInfo("id-ID"));

            // Tampilkan di label
            label4.Text = formattedDateTime;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            guna2Panel3.Height = button9.Height;
            guna2Panel3.Top = button9.Top;
            homeControl1.Visible = false;
            patientControl1.Visible = false;
            appointmentControl1.Visible = false;
            staffControl1.Visible = false;
            inpatientControl1.Visible = false;
            pharmacyControl1.Visible = false;
            paymentControl1.Visible = false;
            medicalRecord1.Visible = true;
            diagnosesControl1.Visible = false;
        }

        private void patientControl1_Load(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {
            guna2Panel3.Height = button10.Height;
            guna2Panel3.Top = button10.Top;
            homeControl1.Visible = false;
            patientControl1.Visible = false;
            appointmentControl1.Visible = false;
            staffControl1.Visible = false;
            inpatientControl1.Visible = false;
            pharmacyControl1.Visible = false;
            paymentControl1.Visible = false;
            medicalRecord1.Visible = false;
            diagnosesControl1.Visible = true;
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel4_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}