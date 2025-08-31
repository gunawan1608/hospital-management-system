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
    public partial class MedicalRecord : UserControl
    {
        private SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-9SGQJSHQ\SQLEXPRESS;Initial Catalog=DB_Hospital;Integrated Security=True;");
        private int? selectedRecordID = null;

        public MedicalRecord()
        {
            InitializeComponent();
        }

        private void LoadPatients()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT PatientID, 
                                   CONCAT(Name, ' (', MedicalRecordNumber, ')') as DisplayName 
                                   FROM Patients 
                                   ORDER BY Name";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, tempConn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        guna2ComboBox1.DataSource = dt;
                        guna2ComboBox1.DisplayMember = "DisplayName";
                        guna2ComboBox1.ValueMember = "PatientID";
                        guna2ComboBox1.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading patients: " + ex.Message);
            }
        }

        private void guna2TextBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            InsertMedicalRecord();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            UpdateMedicalRecord();
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            DeleteMedicalRecord();
        }

        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            string keyword = guna2TextBox9.Text; // Search box
            SearchMedicalRecords(keyword);
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0) // Ensure the row index is valid
                {
                    DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                    // Get data from the selected row
                    selectedRecordID = Convert.ToInt32(row.Cells["MedicalRecordID"].Value); // Adjust the column name
                    guna2ComboBox1.SelectedValue = row.Cells["PatientID"].Value;    // Adjust PatientID binding
                    guna2DateTimePicker1.Value = Convert.ToDateTime(row.Cells["RecordDate"].Value);
                    guna2TextBox1.Text = row.Cells["Description"].Value.ToString();
                    guna2TextBox2.Text = row.Cells["AttachmentURL"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting record: " + ex.Message);
            }
        }

        private void MedicalRecord_Load(object sender, EventArgs e)
        {
            try
            {
                LoadPatients();
                LoadMedicalRecords();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading medicine data: " + ex.Message);
            }
        }

        private void LoadMedicalRecords()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT mr.MedicalRecordID, 
                                    mr.PatientID,
                                    p.Name AS PatientName, 
                                    mr.RecordDate, 
                                    mr.Description, 
                                    mr.AttachmentURL, 
                                    mr.CreatedAt, 
                                    mr.UpdatedAt
                             FROM MedicalRecords mr
                             JOIN Patients p ON mr.PatientID = p.PatientID
                             ORDER BY mr.MedicalRecordID ASC";

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
                MessageBox.Show("Error loading medical records: " + ex.Message);
            }
        }

        private void InsertMedicalRecord()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"INSERT INTO MedicalRecords (PatientID, RecordDate, Description, AttachmentURL, CreatedAt, UpdatedAt) 
                             VALUES (@PatientID, @RecordDate, @Description, @AttachmentURL, GETDATE(), GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@PatientID", guna2ComboBox1.SelectedValue);
                        cmd.Parameters.AddWithValue("@RecordDate", guna2DateTimePicker1.Value);
                        cmd.Parameters.AddWithValue("@Description", guna2TextBox1.Text);
                        cmd.Parameters.AddWithValue("@AttachmentURL", guna2TextBox2.Text);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Medical record added successfully.");
                        LoadMedicalRecords();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting medical record: " + ex.Message);
            }
        }

        private void UpdateMedicalRecord()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"UPDATE MedicalRecords 
                             SET PatientID = @PatientID, 
                                 RecordDate = @RecordDate, 
                                 Description = @Description, 
                                 AttachmentURL = @AttachmentURL, 
                                 UpdatedAt = GETDATE()
                             WHERE MedicalRecordID = @RecordID";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@RecordID", selectedRecordID);
                        cmd.Parameters.AddWithValue("@PatientID", guna2ComboBox1.SelectedValue);
                        cmd.Parameters.AddWithValue("@RecordDate", guna2DateTimePicker1.Value);
                        cmd.Parameters.AddWithValue("@Description", guna2TextBox1.Text);
                        cmd.Parameters.AddWithValue("@AttachmentURL", guna2TextBox2.Text);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Medical record updated successfully.");
                        LoadMedicalRecords();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating medical record: " + ex.Message);
            }
        }

        private void DeleteMedicalRecord()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = "DELETE FROM MedicalRecords WHERE MedicalRecordID = @RecordID";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@RecordID", selectedRecordID);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Medical record deleted successfully.");
                        LoadMedicalRecords();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting medical record: " + ex.Message);
            }
        }

        private void SearchMedicalRecords(string keyword)
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT mr.MedicalRecordID, 
                                    mr.PatientID,
                                    p.Name AS PatientName, 
                                    mr.RecordDate, 
                                    mr.Description, 
                                    mr.AttachmentURL, 
                                    mr.CreatedAt, 
                                    mr.UpdatedAt
                             FROM MedicalRecords mr
                             JOIN Patients p ON mr.PatientID = p.PatientID
                             WHERE p.Name LIKE @Keyword 
                                OR mr.Description LIKE @Keyword
                             ORDER BY mr.MedicalRecordID ASC";

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
                MessageBox.Show("Error searching medical records: " + ex.Message);
            }
        }

    }
}
