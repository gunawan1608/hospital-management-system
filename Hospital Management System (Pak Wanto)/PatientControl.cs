using Guna.UI2.WinForms;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Hospital_Management_System__Pak_Wanto_
{
    public partial class PatientControl : UserControl
    {
        private SqlConnection conn = new SqlConnection("Data Source=LAPTOP-9SGQJSHQ\\SQLEXPRESS;Initial Catalog=DB_Hospital;Integrated Security=True;");

        public PatientControl()
        {
            InitializeComponent();
        }

        private void PatientControl_Load(object sender, EventArgs e)
        {
            RefreshDataGridView();
        }

        private void RefreshDataGridView()
        {
            try
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                conn.Open();
                string query = "SELECT PatientId, MedicalRecordNumber, Name, DateOfBirth, Gender, Address, Phone, Email, BloodType FROM Patients";
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error refreshing data: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e) // Insert
        {
            try
            {
                if (string.IsNullOrWhiteSpace(guna2TextBox1.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox2.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox4.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox5.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox6.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox7.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox8.Text))
                {
                    MessageBox.Show("Please fill in all fields");
                    return;
                }

                // Validasi email
                if (!guna2TextBox7.Text.Contains("@"))
                {
                    MessageBox.Show("Invalid email format. Email must contain '@'.");
                    return;
                }

                // Validasi gender (Uppercase/Lowercase)
                string gender = guna2TextBox4.Text.Trim().ToLower(); // Konversi ke lowercase
                if (gender != "male" && gender != "female")
                {
                    MessageBox.Show("Invalid gender. Please enter 'Male' or 'Female' (case insensitive).");
                    return;
                }

                conn.Open();
                string query = @"INSERT INTO Patients 
                       (MedicalRecordNumber, Name, DateOfBirth, Gender, Address, Phone, Email, BloodType, CreatedAt, UpdatedAt) 
                       VALUES 
                       (@MRN, @Name, @DOB, @Gender, @Address, @Phone, @Email, @BloodType, GETDATE(), GETDATE())";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MRN", guna2TextBox1.Text.Trim());
                cmd.Parameters.AddWithValue("@Name", guna2TextBox2.Text.Trim());
                cmd.Parameters.AddWithValue("@DOB", guna2DateTimePicker1.Value.Date);
                cmd.Parameters.AddWithValue("@Gender", guna2TextBox4.Text.Trim());
                cmd.Parameters.AddWithValue("@Address", guna2TextBox5.Text.Trim());
                cmd.Parameters.AddWithValue("@Phone", guna2TextBox8.Text.Trim());
                cmd.Parameters.AddWithValue("@Email", guna2TextBox7.Text.Trim());
                cmd.Parameters.AddWithValue("@BloodType", guna2TextBox6.Text.Trim());

                cmd.ExecuteNonQuery();
                MessageBox.Show("Patient added successfully!");
                ClearFields();
                RefreshDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }


        private void guna2Button2_Click(object sender, EventArgs e) // Update
        {
            try
            {
                if (dataGridView1.CurrentRow == null)
                {
                    MessageBox.Show("Please select a patient to update");
                    return;
                }

                // Validasi email
                if (!guna2TextBox7.Text.Contains("@"))
                {
                    MessageBox.Show("Invalid email format. Email must contain '@'.");
                    return;
                }

                // Validasi gender (Uppercase/Lowercase)
                string gender = guna2TextBox4.Text.Trim().ToLower(); // Konversi ke lowercase
                if (gender != "male" && gender != "female")
                {
                    MessageBox.Show("Invalid gender. Please enter 'Male' or 'Female' (case insensitive).");
                    return;
                }

                conn.Open();
                string query = @"UPDATE Patients SET 
                       MedicalRecordNumber = @MRN,
                       Name = @Name,
                       DateOfBirth = @DOB,
                       Gender = @Gender,
                       Address = @Address,
                       Phone = @Phone,
                       Email = @Email,
                       BloodType = @BloodType,
                       UpdatedAt = GETDATE()
                       WHERE PatientID = @PatientID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PatientID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["PatientId"].Value));
                cmd.Parameters.AddWithValue("@MRN", guna2TextBox1.Text.Trim());
                cmd.Parameters.AddWithValue("@Name", guna2TextBox2.Text.Trim());
                cmd.Parameters.AddWithValue("@DOB", guna2DateTimePicker1.Value.Date);
                cmd.Parameters.AddWithValue("@Gender", guna2TextBox4.Text.Trim());
                cmd.Parameters.AddWithValue("@Address", guna2TextBox5.Text.Trim());
                cmd.Parameters.AddWithValue("@Phone", guna2TextBox8.Text.Trim());
                cmd.Parameters.AddWithValue("@Email", guna2TextBox7.Text.Trim());
                cmd.Parameters.AddWithValue("@BloodType", guna2TextBox6.Text.Trim());

                cmd.ExecuteNonQuery();
                MessageBox.Show("Patient updated successfully!");
                ClearFields();
                RefreshDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }


        private void guna2Button3_Click(object sender, EventArgs e) // Delete
        {
            try
            {
                if (dataGridView1.CurrentRow == null)
                {
                    MessageBox.Show("Please select a patient to delete");
                    return;
                }

                if (MessageBox.Show("Are you sure you want to delete this patient?", "Confirm Delete",
                    MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }

                conn.Open();
                string query = "DELETE FROM Patients WHERE PatientID = @PatientID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PatientID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["PatientId"].Value));
                cmd.ExecuteNonQuery();

                MessageBox.Show("Patient deleted successfully!");
                ClearFields();
                RefreshDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void guna2ImageButton1_Click(object sender, EventArgs e) // Search
        {
            try
            {
                string searchText = guna2TextBox9.Text.Trim();
                conn.Open();
                string query = @"SELECT PatientId, MedicalRecordNumber, Name, DateOfBirth, Gender, Address, Phone, Email, BloodType 
                               FROM Patients 
                               WHERE Name LIKE @Search OR MedicalRecordNumber LIKE @Search";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Search", "%" + searchText + "%");
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void ClearFields()
        {
            guna2TextBox1.Clear();
            guna2TextBox2.Clear();
            guna2DateTimePicker1.Value = DateTime.Now;
            guna2TextBox4.Clear();
            guna2TextBox5.Clear();
            guna2TextBox6.Clear();
            guna2TextBox7.Clear();
            guna2TextBox8.Clear();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                    guna2TextBox1.Text = row.Cells["MedicalRecordNumber"].Value?.ToString() ?? "";
                    guna2TextBox2.Text = row.Cells["Name"].Value?.ToString() ?? "";

                    if (row.Cells["DateOfBirth"].Value != null && row.Cells["DateOfBirth"].Value != DBNull.Value)
                    {
                        guna2DateTimePicker1.Value = Convert.ToDateTime(row.Cells["DateOfBirth"].Value);
                    }

                    guna2TextBox4.Text = row.Cells["Gender"].Value?.ToString() ?? "";
                    guna2TextBox5.Text = row.Cells["Address"].Value?.ToString() ?? "";
                    guna2TextBox8.Text = row.Cells["Phone"].Value?.ToString() ?? "";
                    guna2TextBox7.Text = row.Cells["Email"].Value?.ToString() ?? "";
                    guna2TextBox6.Text = row.Cells["BloodType"].Value?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }
    }
}