using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Hospital_Management_System__Pak_Wanto_
{
    public partial class NurseControl : UserControl
    {
        private SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-9SGQJSHQ\SQLEXPRESS;Initial Catalog=DB_Hospital;Integrated Security=True;");

        public NurseControl()
        {
            InitializeComponent();
        }

        private void NurseControl_Load(object sender, EventArgs e)
        {
            LoadDepartments();
            RefreshDataGridView();
        }

        private void LoadDepartments()
        {
            try
            {
                conn.Open();
                string query = "SELECT DepartmentID, DepartmentName FROM Departments ORDER BY DepartmentName";
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                guna2ComboBox1.DataSource = dt;
                guna2ComboBox1.DisplayMember = "DepartmentName";
                guna2ComboBox1.ValueMember = "DepartmentID";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading departments: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
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
                // Ubah DESC menjadi ASC untuk mengurutkan NurseID secara menaik
                string query = @"SELECT n.NurseID, n.Name, n.LicenseNumber, 
                       d.DepartmentName, n.Phone, n.Email, n.Address
                       FROM Nurses n
                       LEFT JOIN Departments d ON n.DepartmentID = d.DepartmentID
                       ORDER BY n.NurseID ASC"; // Menggunakan ASC untuk urutan menaik

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
                if (string.IsNullOrWhiteSpace(guna2TextBox1.Text) || // Name
                    string.IsNullOrWhiteSpace(guna2TextBox2.Text) || // LicenseNumber
                    guna2ComboBox1.SelectedValue == null)
                {
                    MessageBox.Show("Name, License Number, and Department are required!");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(guna2TextBox5.Text) && !guna2TextBox5.Text.Contains("@"))
                {
                    MessageBox.Show("Please enter a valid email address!");
                    return;
                }

                conn.Open();
                string query = @"INSERT INTO Nurses 
                               (Name, LicenseNumber, DepartmentID, Phone, Email, Address, CreatedAt, UpdatedAt) 
                               VALUES 
                               (@Name, @LicenseNumber, @DepartmentID, @Phone, @Email, @Address, GETDATE(), GETDATE())";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", guna2TextBox1.Text.Trim());
                cmd.Parameters.AddWithValue("@LicenseNumber", guna2TextBox2.Text.Trim());
                cmd.Parameters.AddWithValue("@DepartmentID", guna2ComboBox1.SelectedValue);
                cmd.Parameters.AddWithValue("@Phone", guna2TextBox4.Text.Trim());
                cmd.Parameters.AddWithValue("@Email", guna2TextBox5.Text.Trim());
                cmd.Parameters.AddWithValue("@Address", guna2TextBox6.Text.Trim());

                cmd.ExecuteNonQuery();
                MessageBox.Show("Nurse added successfully!");
                ClearFields();
                RefreshDataGridView();
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                MessageBox.Show("This License Number is already registered!");
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
                    MessageBox.Show("Please select a nurse to update!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(guna2TextBox1.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox2.Text) ||
                    guna2ComboBox1.SelectedValue == null)
                {
                    MessageBox.Show("Name, License Number, and Department are required!");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(guna2TextBox5.Text) && !guna2TextBox5.Text.Contains("@"))
                {
                    MessageBox.Show("Please enter a valid email address!");
                    return;
                }

                conn.Open();
                string query = @"UPDATE Nurses SET 
                               Name = @Name,
                               LicenseNumber = @LicenseNumber,
                               DepartmentID = @DepartmentID,
                               Phone = @Phone,
                               Email = @Email,
                               Address = @Address,
                               UpdatedAt = GETDATE()
                               WHERE NurseID = @NurseID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@NurseID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["NurseID"].Value));
                cmd.Parameters.AddWithValue("@Name", guna2TextBox1.Text.Trim());
                cmd.Parameters.AddWithValue("@LicenseNumber", guna2TextBox2.Text.Trim());
                cmd.Parameters.AddWithValue("@DepartmentID", guna2ComboBox1.SelectedValue);
                cmd.Parameters.AddWithValue("@Phone", guna2TextBox4.Text.Trim());
                cmd.Parameters.AddWithValue("@Email", guna2TextBox5.Text.Trim());
                cmd.Parameters.AddWithValue("@Address", guna2TextBox6.Text.Trim());

                cmd.ExecuteNonQuery();
                MessageBox.Show("Nurse updated successfully!");
                ClearFields();
                RefreshDataGridView();
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                MessageBox.Show("This License Number is already used by another nurse!");
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
                    MessageBox.Show("Please select a nurse to delete!");
                    return;
                }

                if (MessageBox.Show("Are you sure you want to delete this nurse?", "Confirm Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return;
                }

                conn.Open();
                string deleteQuery = "DELETE FROM Nurses WHERE NurseID = @NurseID";
                SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn);
                deleteCmd.Parameters.AddWithValue("@NurseID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["NurseID"].Value));
                deleteCmd.ExecuteNonQuery();

                MessageBox.Show("Nurse deleted successfully!");
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
                string query = @"SELECT n.NurseID, n.Name, n.LicenseNumber, 
                               d.DepartmentName, n.Phone, n.Email, n.Address
                               FROM Nurses n
                               LEFT JOIN Departments d ON n.DepartmentID = d.DepartmentID
                               WHERE n.Name LIKE @Search 
                               OR n.LicenseNumber LIKE @Search 
                               OR d.DepartmentName LIKE @Search";

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
            guna2TextBox2.Clear(); // LicenseNumber
            guna2ComboBox1.SelectedIndex = -1; // Reset Department selection
            guna2TextBox4.Clear(); // Phone
            guna2TextBox5.Clear(); // Email
            guna2TextBox6.Clear(); // Address
            guna2TextBox1.Focus();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                    guna2TextBox1.Text = row.Cells["Name"].Value?.ToString() ?? "";
                    guna2TextBox2.Text = row.Cells["LicenseNumber"].Value?.ToString() ?? "";

                    // Find and select the correct department in ComboBox
                    string departmentName = row.Cells["DepartmentName"].Value?.ToString() ?? "";
                    var dataSource = (DataTable)guna2ComboBox1.DataSource;
                    var rows = dataSource.Select($"DepartmentName = '{departmentName}'");
                    if (rows.Length > 0)
                    {
                        guna2ComboBox1.SelectedValue = rows[0]["DepartmentID"];
                    }

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