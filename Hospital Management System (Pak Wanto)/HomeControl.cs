using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hospital_Management_System__Pak_Wanto_
{
    public partial class HomeControl : UserControl
    {
        private SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-9SGQJSHQ\SQLEXPRESS;Initial Catalog=DB_Hospital;Integrated Security=True;");

        public HomeControl()
        {
            InitializeComponent();

            // Set timer interval (1 detik = 1000 milliseconds)
            timer1.Interval = 1000;
            timer1.Enabled = true;
        }

        private void guna2HtmlLabel5_Click(object sender, EventArgs e)
        {
            RefreshAllData();
        }

        private void guna2HtmlLabel6_Click(object sender, EventArgs e)
        {
            RefreshAllData();
        }

        private void guna2HtmlLabel7_Click(object sender, EventArgs e)
        {
            RefreshAllData();
        }

        private void guna2HtmlLabel8_Click(object sender, EventArgs e)
        {
            RefreshAllData();
        }

        private void HomeControl_Load(object sender, EventArgs e)
        {
            RefreshAllData();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RefreshAllData();
        }

        public void RefreshAllData()
        {
            try
            {
                UpdateTotalPatients();
                UpdateTodayAppointments();
                UpdateAvailableRooms();
                UpdateActiveStaff();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error refreshing dashboard data: " + ex.Message);
            }
        }

        private void UpdateTotalPatients()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = "SELECT COUNT(*) FROM Patients";
                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        int count = (int)cmd.ExecuteScalar();
                        guna2HtmlLabel5.Text = count.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating total patients: " + ex.Message);
            }
        }

        private void UpdateTodayAppointments()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT COUNT(*) 
                                   FROM Appointments 
                                   WHERE CAST(AppointmentDate AS DATE) = CAST(GETDATE() AS DATE)
                                   AND Status = 'Scheduled'";
                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        int count = (int)cmd.ExecuteScalar();
                        guna2HtmlLabel6.Text = count.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating today's appointments: " + ex.Message);
            }
        }

        private void UpdateAvailableRooms()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = "SELECT COUNT(*) FROM Rooms WHERE Status = 'Available'";
                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        int count = (int)cmd.ExecuteScalar();
                        guna2HtmlLabel7.Text = count.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating available rooms: " + ex.Message);
            }
        }

        private void UpdateActiveStaff()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT 
                                   (SELECT COUNT(*) FROM Doctors) +
                                   (SELECT COUNT(*) FROM Nurses) as TotalStaff";
                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        int count = (int)cmd.ExecuteScalar();
                        guna2HtmlLabel8.Text = count.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating active staff: " + ex.Message);
            }
        }
    }
}
