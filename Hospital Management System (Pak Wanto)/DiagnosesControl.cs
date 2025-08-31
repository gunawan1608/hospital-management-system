using Guna.UI2.WinForms;
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
    public partial class DiagnosesControl : UserControl
    {
        private SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-9SGQJSHQ\SQLEXPRESS;Initial Catalog=DB_Hospital;Integrated Security=True;");
        public DiagnosesControl()
        {
            InitializeComponent();
        }

        private int? selectedDiagnosisID = null;
        private void LoadAppointments()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT AppointmentID, 
                           CONCAT('APT-', AppointmentID, ' (', p.Name, ')') as DisplayName 
                           FROM Appointments a
                           JOIN Patients p ON a.PatientID = p.PatientID
                           ORDER BY AppointmentID ASC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, tempConn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        guna2ComboBox1.DataSource = dt;
                        guna2ComboBox1.DisplayMember = "DisplayName";
                        guna2ComboBox1.ValueMember = "AppointmentID";
                        guna2ComboBox1.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading appointments: " + ex.Message);
            }
        }

        private void LoadDiagnoses()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT d.DiagnosisID, 
                                    a.AppointmentID, 
                                    p.Name AS PatientName, 
                                    d.DiagnosisCode, 
                                    d.Description, 
                                    d.DateDiagnosed 
                             FROM Diagnoses d
                             JOIN Appointments a ON d.AppointmentID = a.AppointmentID
                             JOIN Patients p ON a.PatientID = p.PatientID
                             ORDER BY d.DiagnosisID ASC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, tempConn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridView1.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading diagnoses: " + ex.Message);
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"INSERT INTO Diagnoses (AppointmentID, DiagnosisCode, Description, DateDiagnosed)
                             VALUES (@AppointmentID, @DiagnosisCode, @Description, @DateDiagnosed)";
                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@AppointmentID", guna2ComboBox1.SelectedValue);
                        cmd.Parameters.AddWithValue("@DiagnosisCode", guna2TextBox1.Text); // Assumed
                        cmd.Parameters.AddWithValue("@Description", guna2TextBox2.Text); // Assumed
                        cmd.Parameters.AddWithValue("@DateDiagnosed", guna2DateTimePicker1.Value); // Assumed
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Diagnosis added successfully.");
                LoadDiagnoses();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding diagnosis: " + ex.Message);
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            if (selectedDiagnosisID == null)
            {
                MessageBox.Show("Please select a diagnosis to update.");
                return;
            }

            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"UPDATE Diagnoses 
                             SET AppointmentID = @AppointmentID, 
                                 DiagnosisCode = @DiagnosisCode, 
                                 Description = @Description, 
                                 DateDiagnosed = @DateDiagnosed, 
                                 UpdatedAt = GETDATE()
                             WHERE DiagnosisID = @DiagnosisID";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@AppointmentID", guna2ComboBox1.SelectedValue);
                        cmd.Parameters.AddWithValue("@DiagnosisCode", guna2TextBox1.Text);
                        cmd.Parameters.AddWithValue("@Description", guna2TextBox2.Text);
                        cmd.Parameters.AddWithValue("@DateDiagnosed", guna2DateTimePicker1.Value);
                        cmd.Parameters.AddWithValue("@DiagnosisID", selectedDiagnosisID);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Diagnosis updated successfully.");
                LoadDiagnoses();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating diagnosis: " + ex.Message);
            }
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            if (selectedDiagnosisID == null)
            {
                MessageBox.Show("Please select a diagnosis to delete.");
                return;
            }

            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"DELETE FROM Diagnoses WHERE DiagnosisID = @DiagnosisID";
                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@DiagnosisID", selectedDiagnosisID);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Diagnosis deleted successfully.");
                LoadDiagnoses();
                selectedDiagnosisID = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting diagnosis: " + ex.Message);
            }
        }

        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            string keyword = guna2TextBox9.Text; // Assumed search text box
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                SearchDiagnoses(keyword);
            }
            else
            {
                LoadDiagnoses(); // Reload all data if the search box is empty
            }
        }

        private void SearchDiagnoses(string keyword)
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT d.DiagnosisID, 
                                    a.AppointmentID, 
                                    p.Name AS PatientName, 
                                    d.DiagnosisCode, 
                                    d.Description, 
                                    d.DateDiagnosed 
                             FROM Diagnoses d
                             JOIN Appointments a ON d.AppointmentID = a.AppointmentID
                             JOIN Patients p ON a.PatientID = p.PatientID
                             WHERE d.DiagnosisCode LIKE @Keyword 
                                OR d.Description LIKE @Keyword 
                                OR p.Name LIKE @Keyword
                             ORDER BY d.DiagnosisID DESC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, tempConn))
                    {
                        DataTable dt = new DataTable();
                        adapter.SelectCommand.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");
                        adapter.Fill(dt);
                        dataGridView1.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching diagnoses: " + ex.Message);
            }
        }


        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                selectedDiagnosisID = Convert.ToInt32(row.Cells["DiagnosisID"].Value);
                guna2ComboBox1.SelectedValue = row.Cells["AppointmentID"].Value;
                guna2TextBox1.Text = row.Cells["DiagnosisCode"].Value.ToString();
                guna2TextBox2.Text = row.Cells["Description"].Value.ToString();
                guna2DateTimePicker1.Value = Convert.ToDateTime(row.Cells["DateDiagnosed"].Value);
            }

        }

        private void DiagnosesControl_Load(object sender, EventArgs e)
        {
            try
            {
                LoadAppointments();
                LoadDiagnoses();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading medicine data: " + ex.Message);
            }
        }
    }
}
