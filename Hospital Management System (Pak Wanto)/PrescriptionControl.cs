using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Hospital_Management_System__Pak_Wanto_
{
    public partial class PrescriptionControl : UserControl
    {
        private SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-9SGQJSHQ\SQLEXPRESS;Initial Catalog=DB_Hospital;Integrated Security=True;");

        public PrescriptionControl()
        {
            InitializeComponent();

            // Set default values
            guna2DateTimePicker1.Value = DateTime.Today;
            guna2TextBox3.Text = "Active";

            // Set timer interval (1 menit)
            timer1.Interval = 60000;
            timer1.Enabled = true;

            // Subscribe to events
            timer1.Tick += timer1_Tick;
            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
        }

        private void PrescriptionControl_Load(object sender, EventArgs e)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            try
            {
                if (dataGridView1 != null)
                {
                    dataGridView1.SelectionMode = DataGridViewSelectionMode.CellSelect;
                    dataGridView1.MultiSelect = false;
                }

                // Update status prescriptions yang sudah melewati tanggal
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    if (tempConn == null)
                        return;

                    tempConn.Open();
                    string query = @"UPDATE Prescriptions 
                           SET Status = 'Completed',
                               UpdatedAt = GETDATE()
                           WHERE Status = 'Active' 
                           AND PrescriptionDate <= CAST(GETDATE() AS DATE)";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        if (cmd != null)
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // Load data secara aman
                if (!this.IsDisposed && this.IsHandleCreated)
                {
                    LoadAppointments();
                    RefreshDataGridView();

                    if (timer1 != null)
                    {
                        timer1.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                if (!this.IsDisposed && this.IsHandleCreated)
                {
                    MessageBox.Show("Error initializing form: " + ex.Message);
                }
            }
        }


        private void LoadAppointments()
        {
            if (this.IsDisposed || !this.IsHandleCreated || guna2ComboBox1 == null)
                return;

            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    if (tempConn == null)
                        return;

                    tempConn.Open();
                    string query = @"SELECT a.AppointmentID, 
                           CONCAT(p.Name, ' (', p.MedicalRecordNumber, ') - ', 
                           FORMAT(a.AppointmentDate, 'dd/MM/yyyy')) as DisplayName 
                           FROM Appointments a
                           JOIN Patients p ON a.PatientID = p.PatientID 
                           ORDER BY a.AppointmentDate DESC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, tempConn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        if (!this.IsDisposed && this.IsHandleCreated && guna2ComboBox1 != null)
                        {
                            guna2ComboBox1.DataSource = dt;
                            guna2ComboBox1.DisplayMember = "DisplayName";
                            guna2ComboBox1.ValueMember = "AppointmentID";
                            guna2ComboBox1.SelectedIndex = -1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!this.IsDisposed && this.IsHandleCreated)
                {
                    MessageBox.Show("Error loading appointments: " + ex.Message);
                }
            }
        }


        private void RefreshDataGridView()
        {
            if (this.IsDisposed || !this.IsHandleCreated || dataGridView1 == null)
                return;

            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    if (tempConn == null)
                        return;

                    DataTable dt = new DataTable();

                    string query = @"SELECT 
                           p.PrescriptionID,
                           CONCAT(pat.Name, ' (', pat.MedicalRecordNumber, ')') AS PatientName,
                           FORMAT(a.AppointmentDate, 'dd/MM/yyyy') AS AppointmentDate,
                           FORMAT(p.PrescriptionDate, 'dd/MM/yyyy') AS PrescriptionDate,
                           p.Status,
                           p.Notes
                           FROM Prescriptions p
                           JOIN Appointments a ON p.AppointmentID = a.AppointmentID
                           JOIN Patients pat ON a.PatientID = pat.PatientID
                           ORDER BY p.PrescriptionID ASC";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        tempConn.Open();
                        adapter.Fill(dt);

                        if (!this.IsDisposed && this.IsHandleCreated && dataGridView1 != null)
                        {
                            dataGridView1.DataSource = dt;

                            foreach (DataGridViewColumn col in dataGridView1.Columns)
                            {
                                if (col != null)
                                {
                                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                                }
                            }

                            dataGridView1.Refresh();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!this.IsDisposed && this.IsHandleCreated)
                {
                    MessageBox.Show("Error refreshing data: " + ex.Message);
                }
            }
        }

        private void UpdateExpiredPrescriptionsStatus()
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    if (tempConn == null)
                        return;

                    tempConn.Open();
                    string query = @"UPDATE Prescriptions
                        SET Status = 'Completed',
                            UpdatedAt = GETDATE()
                        WHERE Status = 'Active' 
                        AND PrescriptionDate <= CAST(GETDATE() AS DATE)";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        if (cmd != null)
                        {
                            int rowsAffected = cmd.ExecuteNonQuery();
                            // Refresh regardless of rowsAffected
                            if (!this.IsDisposed && this.IsHandleCreated)
                            {
                                RefreshDataGridView();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating expired prescriptions: " + ex.Message);
            }
        }

        private void ClearFields()
        {
            guna2ComboBox1.SelectedIndex = -1;
            guna2DateTimePicker1.Value = DateTime.Today;
            guna2TextBox3.Text = "Active";
            guna2TextBox4.Clear();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (guna2ComboBox1.SelectedValue == null)
                {
                    MessageBox.Show("Please select an Appointment!");
                    return;
                }

                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"INSERT INTO Prescriptions 
                       (AppointmentID, PrescriptionDate, Status, Notes, CreatedAt, UpdatedAt) 
                       VALUES 
                       (@AppointmentID, @PrescriptionDate, @Status, @Notes, GETDATE(), GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@AppointmentID", guna2ComboBox1.SelectedValue);
                        cmd.Parameters.AddWithValue("@PrescriptionDate", guna2DateTimePicker1.Value.Date);
                        cmd.Parameters.AddWithValue("@Status", guna2TextBox3.Text.Trim());
                        cmd.Parameters.AddWithValue("@Notes", guna2TextBox4.Text.Trim());

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Prescription added successfully!");
                ClearFields();
                RefreshDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow == null)
                {
                    MessageBox.Show("Please select a prescription to update!");
                    return;
                }

                if (guna2ComboBox1.SelectedValue == null)
                {
                    MessageBox.Show("Please select an Appointment!");
                    return;
                }

                conn.Open();
                string query = @"UPDATE Prescriptions SET 
                       AppointmentID = @AppointmentID,
                       PrescriptionDate = @PrescriptionDate,
                       Status = @Status,
                       Notes = @Notes,
                       UpdatedAt = GETDATE()
                       WHERE PrescriptionID = @PrescriptionID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PrescriptionID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["PrescriptionID"].Value));
                cmd.Parameters.AddWithValue("@AppointmentID", guna2ComboBox1.SelectedValue);
                cmd.Parameters.AddWithValue("@PrescriptionDate", guna2DateTimePicker1.Value.Date);
                cmd.Parameters.AddWithValue("@Status", guna2TextBox3.Text.Trim());
                cmd.Parameters.AddWithValue("@Notes", guna2TextBox4.Text.Trim());

                cmd.ExecuteNonQuery();
                MessageBox.Show("Prescription updated successfully!");
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
                    MessageBox.Show("Please select a prescription to delete!");
                    return;
                }

                if (MessageBox.Show("Are you sure you want to delete this prescription?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return;
                }

                conn.Open();
                string query = "DELETE FROM Prescriptions WHERE PrescriptionID = @PrescriptionID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PrescriptionID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["PrescriptionID"].Value));
                cmd.ExecuteNonQuery();

                MessageBox.Show("Prescription deleted successfully!");
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

                    // Find and select the appointment in ComboBox
                    string patientName = row.Cells["PatientName"].Value?.ToString();
                    string appointmentDate = row.Cells["AppointmentDate"].Value?.ToString();
                    foreach (DataRowView drv in guna2ComboBox1.Items)
                    {
                        if (drv["DisplayName"].ToString().Contains(patientName) &&
                            drv["DisplayName"].ToString().Contains(appointmentDate))
                        {
                            guna2ComboBox1.SelectedValue = drv["AppointmentID"];
                            break;
                        }
                    }

                    if (DateTime.TryParse(row.Cells["PrescriptionDate"].Value?.ToString(), out DateTime prescriptionDate))
                    {
                        guna2DateTimePicker1.Value = prescriptionDate;
                    }

                    guna2TextBox3.Text = row.Cells["Status"].Value?.ToString() ?? "Active";
                    guna2TextBox4.Text = row.Cells["Notes"].Value?.ToString() ?? "";
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
                           p.PrescriptionID,
                           CONCAT(pat.Name, ' (', pat.MedicalRecordNumber, ')') AS PatientName,
                           FORMAT(a.AppointmentDate, 'dd/MM/yyyy') AS AppointmentDate,
                           FORMAT(p.PrescriptionDate, 'dd/MM/yyyy') AS PrescriptionDate,
                           p.Status,
                           p.Notes
                           FROM Prescriptions p
                           JOIN Appointments a ON p.AppointmentID = a.AppointmentID
                           JOIN Patients pat ON a.PatientID = pat.PatientID
                           WHERE pat.Name LIKE @Search
                           OR pat.MedicalRecordNumber LIKE @Search
                           OR p.Status LIKE @Search
                           ORDER BY p.PrescriptionID ASC";

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

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!this.IsDisposed && this.IsHandleCreated)
            {
                UpdateExpiredPrescriptionsStatus();
            }
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString().ToLower();

                switch (status)
                {
                    case "active":
                        e.CellStyle.BackColor = Color.LightBlue;
                        break;
                    case "completed":
                        e.CellStyle.BackColor = Color.LightGreen;
                        break;
                    case "cancelled":
                        e.CellStyle.BackColor = Color.LightCoral;
                        break;
                    default:
                        e.CellStyle.BackColor = dataGridView1.DefaultCellStyle.BackColor;
                        break;
                }
            }
        }
    }
}