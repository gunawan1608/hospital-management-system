using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Hospital_Management_System__Pak_Wanto_
{
    public partial class DoctorControl : UserControl
    {
        private SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-9SGQJSHQ\SQLEXPRESS;Initial Catalog=DB_Hospital;Integrated Security=True;");

        public DoctorControl()
        {
            InitializeComponent();
        }

        private void DoctorControl_Load(object sender, EventArgs e)
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
                string query = @"SELECT DoctorID, Name, Specialization, LicenseNumber, Phone, Email, Address 
                           FROM Doctors 
                           ORDER BY DoctorID DESC";

                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                // Mengatur DataTable agar diurutkan dari 1 ke 20 (ASC) tanpa mengubah query SQL
                dt.DefaultView.Sort = "DoctorID ASC";
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
                // Validasi input
                if (string.IsNullOrWhiteSpace(guna2TextBox1.Text) || // Name
                    string.IsNullOrWhiteSpace(guna2TextBox2.Text) || // Specialization
                    string.IsNullOrWhiteSpace(guna2TextBox3.Text))   // LicenseNumber
                {
                    MessageBox.Show("Name, Specialization, and License Number are required fields!");
                    return;
                }

                // Validasi email jika diisi
                if (!string.IsNullOrWhiteSpace(guna2TextBox5.Text) && !guna2TextBox5.Text.Contains("@"))
                {
                    MessageBox.Show("Please enter a valid email address!");
                    return;
                }

                conn.Open();
                string query = @"INSERT INTO Doctors 
                               (Name, Specialization, LicenseNumber, Phone, Email, Address, CreatedAt, UpdatedAt) 
                               VALUES 
                               (@Name, @Specialization, @LicenseNumber, @Phone, @Email, @Address, GETDATE(), GETDATE())";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", guna2TextBox1.Text.Trim());
                cmd.Parameters.AddWithValue("@Specialization", guna2TextBox2.Text.Trim());
                cmd.Parameters.AddWithValue("@LicenseNumber", guna2TextBox3.Text.Trim());
                cmd.Parameters.AddWithValue("@Phone", guna2TextBox4.Text.Trim());
                cmd.Parameters.AddWithValue("@Email", guna2TextBox5.Text.Trim());
                cmd.Parameters.AddWithValue("@Address", guna2TextBox6.Text.Trim());

                cmd.ExecuteNonQuery();
                MessageBox.Show("Doctor data added successfully!");
                ClearFields();
                RefreshDataGridView();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // Violation of unique constraint
                {
                    MessageBox.Show("This License Number is already registered!");
                }
                else
                {
                    MessageBox.Show("Database Error: " + ex.Message);
                }
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
                    MessageBox.Show("Please select a doctor to update!");
                    return;
                }

                // Validasi input
                if (string.IsNullOrWhiteSpace(guna2TextBox1.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox2.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox3.Text))
                {
                    MessageBox.Show("Name, Specialization, and License Number are required fields!");
                    return;
                }

                // Validasi email jika diisi
                if (!string.IsNullOrWhiteSpace(guna2TextBox5.Text) && !guna2TextBox5.Text.Contains("@"))
                {
                    MessageBox.Show("Please enter a valid email address!");
                    return;
                }

                conn.Open();
                string query = @"UPDATE Doctors SET 
                               Name = @Name,
                               Specialization = @Specialization,
                               LicenseNumber = @LicenseNumber,
                               Phone = @Phone,
                               Email = @Email,
                               Address = @Address,
                               UpdatedAt = GETDATE()
                               WHERE DoctorID = @DoctorID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@DoctorID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["DoctorID"].Value));
                cmd.Parameters.AddWithValue("@Name", guna2TextBox1.Text.Trim());
                cmd.Parameters.AddWithValue("@Specialization", guna2TextBox2.Text.Trim());
                cmd.Parameters.AddWithValue("@LicenseNumber", guna2TextBox3.Text.Trim());
                cmd.Parameters.AddWithValue("@Phone", guna2TextBox4.Text.Trim());
                cmd.Parameters.AddWithValue("@Email", guna2TextBox5.Text.Trim());
                cmd.Parameters.AddWithValue("@Address", guna2TextBox6.Text.Trim());

                cmd.ExecuteNonQuery();
                MessageBox.Show("Doctor data updated successfully!");
                ClearFields();
                RefreshDataGridView();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // Violation of unique constraint
                {
                    MessageBox.Show("This License Number is already used by another doctor!");
                }
                else
                {
                    MessageBox.Show("Database Error: " + ex.Message);
                }
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
                    MessageBox.Show("Please select a doctor to delete!");
                    return;
                }

                if (MessageBox.Show("Are you sure you want to delete this doctor?\nThis action cannot be undone!",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return;
                }

                conn.Open();
                // First check if doctor has any appointments
                string checkQuery = "SELECT COUNT(*) FROM Appointments WHERE DoctorID = @DoctorID";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@DoctorID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["DoctorID"].Value));

                int appointmentCount = (int)checkCmd.ExecuteScalar();
                if (appointmentCount > 0)
                {
                    MessageBox.Show("This doctor cannot be deleted because they have appointments in the system!");
                    return;
                }

                // If no appointments, proceed with deletion
                string deleteQuery = "DELETE FROM Doctors WHERE DoctorID = @DoctorID";
                SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn);
                deleteCmd.Parameters.AddWithValue("@DoctorID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["DoctorID"].Value));
                deleteCmd.ExecuteNonQuery();

                MessageBox.Show("Doctor deleted successfully!");
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
                string query = @"SELECT DoctorID, Name, Specialization, LicenseNumber, Phone, Email, Address 
                               FROM Doctors 
                               WHERE Name LIKE @Search 
                               OR Specialization LIKE @Search 
                               OR LicenseNumber LIKE @Search";

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
            guna2TextBox1.Clear(); // Name
            guna2TextBox2.Clear(); // Specialization
            guna2TextBox3.Clear(); // LicenseNumber
            guna2TextBox4.Clear(); // Phone
            guna2TextBox5.Clear(); // Email
            guna2TextBox6.Clear(); // Address
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                    guna2TextBox1.Text = row.Cells["Name"].Value?.ToString() ?? "";
                    guna2TextBox2.Text = row.Cells["Specialization"].Value?.ToString() ?? "";
                    guna2TextBox3.Text = row.Cells["LicenseNumber"].Value?.ToString() ?? "";
                    guna2TextBox4.Text = row.Cells["Phone"].Value?.ToString() ?? "";
                    guna2TextBox5.Text = row.Cells["Email"].Value?.ToString() ?? "";
                    guna2TextBox6.Text = row.Cells["Address"].Value?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }
    }
}