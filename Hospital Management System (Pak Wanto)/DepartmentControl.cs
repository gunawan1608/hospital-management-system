using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Hospital_Management_System__Pak_Wanto_
{
    public partial class DepartmentControl : UserControl
    {
        private SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-9SGQJSHQ\SQLEXPRESS;Initial Catalog=DB_Hospital;Integrated Security=True;");

        public DepartmentControl()
        {
            InitializeComponent();
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
                // Ubah DESC menjadi ASC untuk mengurutkan dari ID terkecil ke terbesar
                string query = @"SELECT DepartmentID, DepartmentName, Description
                           FROM Departments ORDER BY DepartmentID ASC";
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
                if (string.IsNullOrWhiteSpace(guna2TextBox1.Text))
                {
                    MessageBox.Show("Department Name is required!");
                    return;
                }

                conn.Open();
                string query = @"INSERT INTO Departments 
                               (DepartmentName, Description, CreatedAt, UpdatedAt) 
                               VALUES 
                               (@DepartmentName, @Description, GETDATE(), GETDATE())";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@DepartmentName", guna2TextBox1.Text.Trim());
                cmd.Parameters.AddWithValue("@Description", guna2TextBox2.Text.Trim());

                cmd.ExecuteNonQuery();
                MessageBox.Show("Department added successfully!");
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
                    MessageBox.Show("Please select a department to update");
                    return;
                }

                if (string.IsNullOrWhiteSpace(guna2TextBox1.Text))
                {
                    MessageBox.Show("Department Name is required!");
                    return;
                }

                conn.Open();
                string query = @"UPDATE Departments SET 
                               DepartmentName = @DepartmentName,
                               Description = @Description,
                               UpdatedAt = GETDATE()
                               WHERE DepartmentID = @DepartmentID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@DepartmentID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["DepartmentID"].Value));
                cmd.Parameters.AddWithValue("@DepartmentName", guna2TextBox1.Text.Trim());
                cmd.Parameters.AddWithValue("@Description", guna2TextBox2.Text.Trim());

                cmd.ExecuteNonQuery();
                MessageBox.Show("Department updated successfully!");
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
                    MessageBox.Show("Please select a department to delete");
                    return;
                }

                if (MessageBox.Show("Are you sure you want to delete this department?", "Confirm Delete",
                    MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }

                conn.Open();

                // Check if department is being used
                string checkQuery = @"SELECT 
                                    (SELECT COUNT(*) FROM Nurses WHERE DepartmentID = @DeptID) +
                                    (SELECT COUNT(*) FROM Appointments WHERE DepartmentID = @DeptID) as UseCount";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@DeptID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["DepartmentID"].Value));

                int useCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (useCount > 0)
                {
                    MessageBox.Show("This department cannot be deleted because it is being used by nurses or appointments!");
                    return;
                }

                string deleteQuery = "DELETE FROM Departments WHERE DepartmentID = @DepartmentID";
                SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn);
                deleteCmd.Parameters.AddWithValue("@DepartmentID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["DepartmentID"].Value));
                deleteCmd.ExecuteNonQuery();

                MessageBox.Show("Department deleted successfully!");
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
                string query = @"SELECT DepartmentID, DepartmentName, Description 
                               FROM Departments 
                               WHERE DepartmentName LIKE @Search 
                               OR Description LIKE @Search";

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
            guna2TextBox1.Clear(); // DepartmentName
            guna2TextBox2.Clear(); // Description
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                    guna2TextBox1.Text = row.Cells["DepartmentName"].Value?.ToString() ?? "";
                    guna2TextBox2.Text = row.Cells["Description"].Value?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void DepartmentControl_Load(object sender, EventArgs e)
        {
            RefreshDataGridView();
        }
    }
}