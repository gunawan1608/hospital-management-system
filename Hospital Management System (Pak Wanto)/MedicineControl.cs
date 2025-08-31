using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Hospital_Management_System__Pak_Wanto_
{
    public partial class MedicineControl : UserControl
    {
        private SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-9SGQJSHQ\SQLEXPRESS;Initial Catalog=DB_Hospital;Integrated Security=True;");

        public MedicineControl()
        {
            InitializeComponent();
        }

        private void MedicineControl_Load(object sender, EventArgs e)
        {
            try
            {
                RefreshDataGridView();
                SetupRealTimeStockUpdate(); // Start real-time update
                PerformRealTimeStockUpdate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading medicine data: " + ex.Message);
            }
        }

        private void RefreshDataGridView()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT 
                MedicineID,
                MedicineName,
                Description,
                UnitPrice,
                Stock
                FROM Medicines
                ORDER BY MedicineID ASC"; 

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, tempConn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridView1.DataSource = dt;

                        // Set column headers
                        if (dataGridView1.Columns["MedicineID"] != null)
                            dataGridView1.Columns["MedicineID"].HeaderText = "ID";
                        if (dataGridView1.Columns["MedicineName"] != null)
                            dataGridView1.Columns["MedicineName"].HeaderText = "Name";
                        if (dataGridView1.Columns["Description"] != null)
                            dataGridView1.Columns["Description"].HeaderText = "Description";
                        if (dataGridView1.Columns["UnitPrice"] != null)
                            dataGridView1.Columns["UnitPrice"].HeaderText = "Unit Price";
                        if (dataGridView1.Columns["Stock"] != null)
                            dataGridView1.Columns["Stock"].HeaderText = "Stock";

                        // Adjust columns
                        dataGridView1.AutoResizeColumns();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error refreshing data: " + ex.Message);
            }
        }

        private void ClearFields()
        {
            guna2TextBox1.Clear(); // Name
            guna2TextBox2.Clear(); // Description
            guna2TextBox3.Clear(); // Unit Price
            guna2TextBox4.Clear(); // Stock
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(guna2TextBox1.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox3.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox4.Text))
                {
                    MessageBox.Show("Name, Unit Price, and Stock are required!");
                    return;
                }

                if (!decimal.TryParse(guna2TextBox3.Text, out decimal unitPrice) || unitPrice < 0)
                {
                    MessageBox.Show("Please enter a valid Unit Price!");
                    return;
                }

                if (!int.TryParse(guna2TextBox4.Text, out int stock) || stock < 0)
                {
                    MessageBox.Show("Please enter a valid Stock amount!");
                    return;
                }

                conn.Open();
                string query = @"INSERT INTO Medicines 
                    (MedicineName, Description, UnitPrice, Stock, CreatedAt, UpdatedAt) 
                    VALUES 
                    (@MedicineName, @Description, @UnitPrice, @Stock, GETDATE(), GETDATE())";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MedicineName", guna2TextBox1.Text.Trim());
                cmd.Parameters.AddWithValue("@Description", guna2TextBox2.Text.Trim());
                cmd.Parameters.AddWithValue("@UnitPrice", unitPrice);
                cmd.Parameters.AddWithValue("@Stock", stock);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Medicine added successfully!");
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

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow == null)
                {
                    MessageBox.Show("Please select a medicine to update!");
                    return;
                }

                // Validation
                if (string.IsNullOrWhiteSpace(guna2TextBox1.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox3.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox4.Text))
                {
                    MessageBox.Show("Name, Unit Price, and Stock are required!");
                    return;
                }

                if (!decimal.TryParse(guna2TextBox3.Text, out decimal unitPrice) || unitPrice < 0)
                {
                    MessageBox.Show("Please enter a valid Unit Price!");
                    return;
                }

                if (!int.TryParse(guna2TextBox4.Text, out int stock) || stock < 0)
                {
                    MessageBox.Show("Please enter a valid Stock amount!");
                    return;
                }

                conn.Open();
                string query = @"UPDATE Medicines SET 
                    MedicineName = @MedicineName,
                    Description = @Description,
                    UnitPrice = @UnitPrice,
                    Stock = @Stock,
                    UpdatedAt = GETDATE()
                    WHERE MedicineID = @MedicineID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MedicineID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["MedicineID"].Value));
                cmd.Parameters.AddWithValue("@MedicineName", guna2TextBox1.Text.Trim());
                cmd.Parameters.AddWithValue("@Description", guna2TextBox2.Text.Trim());
                cmd.Parameters.AddWithValue("@UnitPrice", unitPrice);
                cmd.Parameters.AddWithValue("@Stock", stock);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Medicine updated successfully!");
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

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow == null)
                {
                    MessageBox.Show("Please select a medicine to delete!");
                    return;
                }

                if (MessageBox.Show("Are you sure you want to delete this medicine?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return;
                }

                conn.Open();
                string query = "DELETE FROM Medicines WHERE MedicineID = @MedicineID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MedicineID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["MedicineID"].Value));
                cmd.ExecuteNonQuery();

                MessageBox.Show("Medicine deleted successfully!");
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

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                    guna2TextBox1.Text = row.Cells["MedicineName"].Value?.ToString();
                    guna2TextBox2.Text = row.Cells["Description"].Value?.ToString();
                    guna2TextBox3.Text = row.Cells["UnitPrice"].Value?.ToString();
                    guna2TextBox4.Text = row.Cells["Stock"].Value?.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            try
            {
                string searchText = guna2TextBox9.Text.Trim();
                conn.Open();
                string query = @"SELECT 
                    MedicineID,
                    MedicineName,
                    Description,
                    UnitPrice,
                    Stock
                    FROM Medicines
                    WHERE MedicineName LIKE @Search 
                    OR Description LIKE @Search
                    ORDER BY MedicineID ASC";

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

        private void guna2TextBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void PerformRealTimeStockUpdate()
        {
            try
            {
                conn.Open();
                string query = @"
                UPDATE Medicines
                SET Stock = Stock - Used.Quantity
                FROM Medicines
                INNER JOIN (
                    SELECT 
                        pd.MedicineID, 
                        SUM(pd.Quantity) AS Quantity
                    FROM 
                        PrescriptionDetails pd
                    INNER JOIN 
                        Prescriptions p ON pd.PrescriptionID = p.PrescriptionID
                    WHERE 
                        CONVERT(DATE, p.PrescriptionDate) = CONVERT(DATE, GETDATE())
                    GROUP BY 
                        pd.MedicineID
                ) Used ON Medicines.MedicineID = Used.MedicineID
                WHERE Medicines.Stock >= Used.Quantity";

                SqlCommand cmd = new SqlCommand(query, conn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in stock update: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            PerformRealTimeStockUpdate();
        }

        private void SetupRealTimeStockUpdate()
        {
            timer1 = new Timer();
            timer1.Interval = 60000; // 1 minute interval
            timer1.Tick += timer1_Tick;
            timer1.Start();
        }
    }
}