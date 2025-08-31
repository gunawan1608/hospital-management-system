using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Hospital_Management_System__Pak_Wanto_
{
    public partial class DetailsControl : UserControl
    {
        private SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-9SGQJSHQ\SQLEXPRESS;Initial Catalog=DB_Hospital;Integrated Security=True;");

        public DetailsControl()
        {
            InitializeComponent();
        }

        private void DetailsControl_Load(object sender, EventArgs e)
        {
            try
            {
                LoadPrescriptions();
                LoadMedicines();
                RefreshDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading details control: " + ex.Message);
            }
        }

        private void LoadPrescriptions()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT p.PrescriptionID, 
                           CONCAT('Prescription #', p.PrescriptionID, ' - ', pat.Name, 
                           ' (', FORMAT(p.PrescriptionDate, 'dd/MM/yyyy'), ')') as DisplayName 
                           FROM Prescriptions p
                           JOIN Appointments a ON p.AppointmentID = a.AppointmentID
                           JOIN Patients pat ON a.PatientID = pat.PatientID 
                           ORDER BY p.PrescriptionID DESC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, tempConn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        guna2ComboBox1.DataSource = dt;
                        guna2ComboBox1.DisplayMember = "DisplayName";
                        guna2ComboBox1.ValueMember = "PrescriptionID";
                        guna2ComboBox1.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading prescriptions: " + ex.Message);
            }
        }

        private void LoadMedicines()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT MedicineID, 
                           CONCAT(MedicineName, ' (Stock: ', Stock, ')') as DisplayName 
                           FROM Medicines 
                           WHERE Stock > 0
                           ORDER BY MedicineName";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, tempConn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        guna2ComboBox2.DataSource = dt;
                        guna2ComboBox2.DisplayMember = "DisplayName";
                        guna2ComboBox2.ValueMember = "MedicineID";
                        guna2ComboBox2.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading medicines: " + ex.Message);
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
                           pd.PrescriptionDetailID,
                           CONCAT('Prescription #', p.PrescriptionID) as PrescriptionNumber,
                           pat.Name as PatientName,
                           m.MedicineName,
                           pd.Quantity,
                           pd.Instructions,
                           FORMAT(p.PrescriptionDate, 'dd/MM/yyyy') as PrescriptionDate
                           FROM PrescriptionDetails pd
                           JOIN Prescriptions p ON pd.PrescriptionID = p.PrescriptionID
                           JOIN Medicines m ON pd.MedicineID = m.MedicineID
                           JOIN Appointments a ON p.AppointmentID = a.AppointmentID
                           JOIN Patients pat ON a.PatientID = pat.PatientID
                           ORDER BY pd.PrescriptionDetailID ASC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, tempConn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridView1.DataSource = dt;

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
            guna2ComboBox1.SelectedIndex = -1;
            guna2ComboBox2.SelectedIndex = -1;
            guna2TextBox3.Clear();
            guna2TextBox4.Clear();
        }

        private bool ValidateStock(int medicineId, int requestedQuantity, int? currentDetailId = null)
        {
            using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
            {
                try
                {
                    tempConn.Open();
                    string query = @"SELECT Stock FROM Medicines WHERE MedicineID = @MedicineID";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@MedicineID", medicineId);
                        int currentStock = (int)cmd.ExecuteScalar();

                        // If updating, add back the current prescription's quantity
                        if (currentDetailId.HasValue)
                        {
                            string getCurrentQtyQuery = "SELECT Quantity FROM PrescriptionDetails WHERE PrescriptionDetailID = @DetailID";
                            using (SqlCommand getQtyCmd = new SqlCommand(getCurrentQtyQuery, tempConn))
                            {
                                getQtyCmd.Parameters.AddWithValue("@DetailID", currentDetailId.Value);
                                int currentQty = (int)getQtyCmd.ExecuteScalar();
                                currentStock += currentQty;
                            }
                        }

                        return requestedQuantity <= currentStock;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error validating stock: " + ex.Message);
                    return false;
                }
            }
        }

        private void UpdateMedicineStock(int medicineId, int quantity, bool isAdd)
        {
            using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
            {
                try
                {
                    tempConn.Open();
                    string query = @"UPDATE Medicines 
                                   SET Stock = Stock + @QuantityChange 
                                   WHERE MedicineID = @MedicineID";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@MedicineID", medicineId);
                        cmd.Parameters.AddWithValue("@QuantityChange", isAdd ? quantity : -quantity);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating stock: " + ex.Message);
                }
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (guna2ComboBox1.SelectedValue == null || guna2ComboBox2.SelectedValue == null)
                {
                    MessageBox.Show("Please select both Prescription and Medicine!");
                    return;
                }

                if (!int.TryParse(guna2TextBox3.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Please enter a valid quantity!");
                    return;
                }

                // Validate stock
                if (!ValidateStock(Convert.ToInt32(guna2ComboBox2.SelectedValue), quantity))
                {
                    MessageBox.Show("Insufficient stock!");
                    return;
                }

                conn.Open();
                string query = @"INSERT INTO PrescriptionDetails 
                       (PrescriptionID, MedicineID, Quantity, Instructions, CreatedAt, UpdatedAt) 
                       VALUES 
                       (@PrescriptionID, @MedicineID, @Quantity, @Instructions, GETDATE(), GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PrescriptionID", guna2ComboBox1.SelectedValue);
                    cmd.Parameters.AddWithValue("@MedicineID", guna2ComboBox2.SelectedValue);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@Instructions", guna2TextBox4.Text.Trim());

                    cmd.ExecuteNonQuery();
                }

                // Update stock
                UpdateMedicineStock(Convert.ToInt32(guna2ComboBox2.SelectedValue), quantity, false);

                MessageBox.Show("Prescription detail added successfully!");
                ClearFields();
                LoadMedicines(); // Refresh medicine list with updated stock
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
                    MessageBox.Show("Please select a detail to update!");
                    return;
                }

                if (guna2ComboBox1.SelectedValue == null || guna2ComboBox2.SelectedValue == null)
                {
                    MessageBox.Show("Please select both Prescription and Medicine!");
                    return;
                }

                if (!int.TryParse(guna2TextBox3.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Please enter a valid quantity!");
                    return;
                }

                int detailId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["PrescriptionDetailID"].Value);

                // Validate stock for update
                if (!ValidateStock(Convert.ToInt32(guna2ComboBox2.SelectedValue), quantity, detailId))
                {
                    MessageBox.Show("Insufficient stock!");
                    return;
                }

                conn.Open();

                // Get current quantity first
                string getQtyQuery = "SELECT Quantity FROM PrescriptionDetails WHERE PrescriptionDetailID = @DetailID";
                int oldQuantity;
                using (SqlCommand getQtyCmd = new SqlCommand(getQtyQuery, conn))
                {
                    getQtyCmd.Parameters.AddWithValue("@DetailID", detailId);
                    oldQuantity = (int)getQtyCmd.ExecuteScalar();
                }

                string query = @"UPDATE PrescriptionDetails SET 
                       PrescriptionID = @PrescriptionID,
                       MedicineID = @MedicineID,
                       Quantity = @Quantity,
                       Instructions = @Instructions,
                       UpdatedAt = GETDATE()
                       WHERE PrescriptionDetailID = @PrescriptionDetailID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PrescriptionDetailID", detailId);
                    cmd.Parameters.AddWithValue("@PrescriptionID", guna2ComboBox1.SelectedValue);
                    cmd.Parameters.AddWithValue("@MedicineID", guna2ComboBox2.SelectedValue);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@Instructions", guna2TextBox4.Text.Trim());

                    cmd.ExecuteNonQuery();
                }

                // Update stock - add back old quantity and remove new quantity
                UpdateMedicineStock(Convert.ToInt32(guna2ComboBox2.SelectedValue), oldQuantity, true);
                UpdateMedicineStock(Convert.ToInt32(guna2ComboBox2.SelectedValue), quantity, false);

                MessageBox.Show("Prescription detail updated successfully!");
                ClearFields();
                LoadMedicines(); // Refresh medicine list with updated stock
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
                    MessageBox.Show("Please select a detail to delete!");
                    return;
                }

                if (MessageBox.Show("Are you sure you want to delete this detail?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return;
                }

                int detailId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["PrescriptionDetailID"].Value);

                conn.Open();

                // Get quantity and medicine ID before deleting
                string getInfoQuery = @"SELECT MedicineID, Quantity 
                                      FROM PrescriptionDetails 
                                      WHERE PrescriptionDetailID = @DetailID";
                int medicineId, quantity;
                using (SqlCommand getInfoCmd = new SqlCommand(getInfoQuery, conn))
                {
                    getInfoCmd.Parameters.AddWithValue("@DetailID", detailId);
                    using (SqlDataReader reader = getInfoCmd.ExecuteReader())
                    {
                        reader.Read();
                        medicineId = reader.GetInt32(0);
                        quantity = reader.GetInt32(1);
                    }
                }

                // Delete the detail
                string deleteQuery = "DELETE FROM PrescriptionDetails WHERE PrescriptionDetailID = @DetailID";
                using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn))
                {
                    deleteCmd.Parameters.AddWithValue("@DetailID", detailId);
                    deleteCmd.ExecuteNonQuery();
                }

                // Return the quantity to stock
                UpdateMedicineStock(medicineId, quantity, true);

                MessageBox.Show("Prescription detail deleted successfully!");
                ClearFields();
                LoadMedicines(); // Refresh medicine list with updated stock
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

                    // Find and select the prescription in ComboBox1
                    string prescriptionNumber = row.Cells["PrescriptionNumber"].Value?.ToString();
                    foreach (DataRowView drv in guna2ComboBox1.Items)
                    {
                        if (drv["DisplayName"].ToString().Contains(prescriptionNumber))
                        {
                            guna2ComboBox1.SelectedValue = drv["PrescriptionID"];
                            break;
                        }
                    }

                    // Find and select the medicine in ComboBox2
                    string medicineName = row.Cells["MedicineName"].Value?.ToString();
                    foreach (DataRowView drv in guna2ComboBox2.Items)
                    {
                        if (drv["DisplayName"].ToString().Contains(medicineName))
                        {
                            guna2ComboBox2.SelectedValue = drv["MedicineID"];
                            break;
                        }
                    }

                    guna2TextBox3.Text = row.Cells["Quantity"].Value?.ToString();
                    guna2TextBox4.Text = row.Cells["Instructions"].Value?.ToString();
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
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT 
                           pd.PrescriptionDetailID,
                           CONCAT('Prescription #', p.PrescriptionID) as PrescriptionNumber,
                           pat.Name as PatientName,
                           m.MedicineName,
                           pd.Quantity,
                           pd.Instructions,
                           FORMAT(p.PrescriptionDate, 'dd/MM/yyyy') as PrescriptionDate
                           FROM PrescriptionDetails pd
                           JOIN Prescriptions p ON pd.PrescriptionID = p.PrescriptionID
                           JOIN Medicines m ON pd.MedicineID = m.MedicineID
                           JOIN Appointments a ON p.AppointmentID = a.AppointmentID
                           JOIN Patients pat ON a.PatientID = pat.PatientID
                           WHERE pat.Name LIKE @Search
                           OR m.MedicineName LIKE @Search
                           OR pd.Instructions LIKE @Search
                           ORDER BY pd.PrescriptionDetailID ASC";

                    SqlCommand cmd = new SqlCommand(query, tempConn);
                    cmd.Parameters.AddWithValue("@Search", "%" + searchText + "%");
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void guna2GradientPanel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}