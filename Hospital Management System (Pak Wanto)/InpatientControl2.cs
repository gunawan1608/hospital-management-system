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
    public partial class InpatientControl2 : UserControl
    {
        private SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-9SGQJSHQ\SQLEXPRESS;Initial Catalog=DB_Hospital;Integrated Security=True;");

        public InpatientControl2()
        {
            InitializeComponent();

            // Set nilai default
            guna2DateTimePicker1.Value = DateTime.Now;
            guna2DateTimePicker2.Value = DateTime.Now;
            guna2TextBox5.Text = "Active";

            // Set timer dan langsung jalankan update pertama kali
            timer1.Interval = 60000; // Check setiap 1 menit
            timer1.Enabled = true;

            // Update status segera setelah form dibuka
            UpdateInpatientStatuses();
        }


        private void guna2Button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Validasi
                if (guna2ComboBox1.SelectedValue == null ||
                    guna2ComboBox2.SelectedValue == null)
                {
                    MessageBox.Show("Please select Patient and Room!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(guna2TextBox5.Text))
                {
                    MessageBox.Show("Status is required!");
                    return;
                }

                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"INSERT INTO Inpatients 
                                   (PatientID, RoomID, AdmissionDate, DischargeDate, 
                                    Status, Notes, CreatedAt, UpdatedAt)
                                   VALUES 
                                   (@PatientID, @RoomID, @AdmissionDate, @DischargeDate,
                                    @Status, @Notes, GETDATE(), GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@PatientID", guna2ComboBox1.SelectedValue);
                        cmd.Parameters.AddWithValue("@RoomID", guna2ComboBox2.SelectedValue);
                        cmd.Parameters.AddWithValue("@AdmissionDate", guna2DateTimePicker2.Value.Date);
                        cmd.Parameters.AddWithValue("@DischargeDate", guna2DateTimePicker1.Value.Date);
                        cmd.Parameters.AddWithValue("@Status", guna2TextBox5.Text.Trim());
                        cmd.Parameters.AddWithValue("@Notes", guna2TextBox6.Text.Trim());

                        cmd.ExecuteNonQuery();
                    }

                    // Update room status to Occupied
                    UpdateRoomStatus(Convert.ToInt32(guna2ComboBox2.SelectedValue), "Occupied");
                }

                MessageBox.Show("Inpatient record added successfully!");
                ClearFields();
                RefreshDataGridView();
                LoadRooms(); // Reload available rooms
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
                    MessageBox.Show("Please select a record to update!");
                    return;
                }

                if (guna2ComboBox1.SelectedValue == null ||
                    guna2ComboBox2.SelectedValue == null)
                {
                    MessageBox.Show("Please select Patient and Room!");
                    return;
                }

                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"UPDATE Inpatients 
                                   SET PatientID = @PatientID,
                                       RoomID = @RoomID,
                                       AdmissionDate = @AdmissionDate,
                                       DischargeDate = @DischargeDate,
                                       Status = @Status,
                                       Notes = @Notes,
                                       UpdatedAt = GETDATE()
                                   WHERE InpatientID = @InpatientID";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@InpatientID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["InpatientID"].Value));
                        cmd.Parameters.AddWithValue("@PatientID", guna2ComboBox1.SelectedValue);
                        cmd.Parameters.AddWithValue("@RoomID", guna2ComboBox2.SelectedValue);
                        cmd.Parameters.AddWithValue("@AdmissionDate", guna2DateTimePicker2.Value.Date);
                        cmd.Parameters.AddWithValue("@DischargeDate", guna2DateTimePicker1.Value.Date);
                        cmd.Parameters.AddWithValue("@Status", guna2TextBox5.Text.Trim());
                        cmd.Parameters.AddWithValue("@Notes", guna2TextBox6.Text.Trim());

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Inpatient record updated successfully!");
                ClearFields();
                RefreshDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow == null)
                {
                    MessageBox.Show("Please select a record to delete!");
                    return;
                }

                if (MessageBox.Show("Are you sure you want to delete this record?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return;
                }

                int roomId = 0;
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();

                    // Get RoomID before deleting
                    string getRoomQuery = "SELECT RoomID FROM Inpatients WHERE InpatientID = @InpatientID";
                    using (SqlCommand getRoomCmd = new SqlCommand(getRoomQuery, tempConn))
                    {
                        getRoomCmd.Parameters.AddWithValue("@InpatientID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["InpatientID"].Value));
                        roomId = Convert.ToInt32(getRoomCmd.ExecuteScalar());
                    }

                    // Delete inpatient record
                    string deleteQuery = "DELETE FROM Inpatients WHERE InpatientID = @InpatientID";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, tempConn))
                    {
                        deleteCmd.Parameters.AddWithValue("@InpatientID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["InpatientID"].Value));
                        deleteCmd.ExecuteNonQuery();
                    }
                }

                // Update room status to Available
                UpdateRoomStatus(roomId, "Available");

                MessageBox.Show("Inpatient record deleted successfully!");
                ClearFields();
                RefreshDataGridView();
                LoadRooms(); // Reload available rooms
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                    // Get InpatientID for reference
                    if (row.Cells["InpatientID"].Value != DBNull.Value)
                    {
                        int inpatientId = Convert.ToInt32(row.Cells["InpatientID"].Value);

                        using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                        {
                            tempConn.Open();
                            string query = @"SELECT i.*, p.PatientID, r.RoomID 
                                   FROM Inpatients i
                                   INNER JOIN Patients p ON i.PatientID = p.PatientID
                                   INNER JOIN Rooms r ON i.RoomID = r.RoomID
                                   WHERE i.InpatientID = @InpatientID";

                            using (SqlCommand cmd = new SqlCommand(query, tempConn))
                            {
                                cmd.Parameters.AddWithValue("@InpatientID", inpatientId);
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        // Set ComboBox values with null checking
                                        guna2ComboBox1.SelectedValue = reader["PatientID"] != DBNull.Value ? reader["PatientID"] : null;
                                        guna2ComboBox2.SelectedValue = reader["RoomID"] != DBNull.Value ? reader["RoomID"] : null;

                                        // Set DateTimePicker values with null checking
                                        guna2DateTimePicker2.Value = reader["AdmissionDate"] != DBNull.Value ?
                                            Convert.ToDateTime(reader["AdmissionDate"]) : DateTime.Now;
                                        guna2DateTimePicker1.Value = reader["DischargeDate"] != DBNull.Value ?
                                            Convert.ToDateTime(reader["DischargeDate"]) : DateTime.Now;

                                        // Set status and notes with null checking
                                        guna2TextBox5.Text = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : "";
                                        guna2TextBox6.Text = reader["Notes"] != DBNull.Value ? reader["Notes"].ToString() : "";
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading record details: " + ex.Message);
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
                    string query = @"SELECT i.InpatientID,
                                  p.Name AS PatientName,
                                  p.MedicalRecordNumber AS MRN,
                                  r.RoomNumber,
                                  r.RoomType,
                                  FORMAT(i.AdmissionDate, 'dd/MM/yyyy') AS AdmissionDate,
                                  FORMAT(i.DischargeDate, 'dd/MM/yyyy') AS DischargeDate,
                                  i.Status,
                                  i.Notes
                           FROM Inpatients i
                           INNER JOIN Patients p ON i.PatientID = p.PatientID
                           INNER JOIN Rooms r ON i.RoomID = r.RoomID
                           WHERE p.Name LIKE @Search 
                           OR p.MedicalRecordNumber LIKE @Search
                           OR r.RoomNumber LIKE @Search
                           OR i.Status LIKE @Search
                           ORDER BY i.InpatientID ASC";  // Changed to ASC order by ID

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@Search", "%" + searchText + "%");
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dataGridView1.DataSource = dt;

                            // Set column headers
                            if (dataGridView1.Columns["InpatientID"] != null)
                                dataGridView1.Columns["InpatientID"].HeaderText = "ID";
                            if (dataGridView1.Columns["PatientName"] != null)
                                dataGridView1.Columns["PatientName"].HeaderText = "Patient Name";
                            if (dataGridView1.Columns["MRN"] != null)
                                dataGridView1.Columns["MRN"].HeaderText = "MRN";
                            if (dataGridView1.Columns["RoomNumber"] != null)
                                dataGridView1.Columns["RoomNumber"].HeaderText = "Room No";
                            if (dataGridView1.Columns["RoomType"] != null)
                                dataGridView1.Columns["RoomType"].HeaderText = "Room Type";
                            if (dataGridView1.Columns["AdmissionDate"] != null)
                                dataGridView1.Columns["AdmissionDate"].HeaderText = "Admission Date";
                            if (dataGridView1.Columns["DischargeDate"] != null)
                                dataGridView1.Columns["DischargeDate"].HeaderText = "Discharge Date";
                            if (dataGridView1.Columns["Status"] != null)
                                dataGridView1.Columns["Status"].HeaderText = "Status";
                            if (dataGridView1.Columns["Notes"] != null)
                                dataGridView1.Columns["Notes"].HeaderText = "Notes";

                            // Adjust columns
                            dataGridView1.AutoResizeColumns();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching data: " + ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateInpatientStatuses();
        }

        // Load form
        private void InpatientControl2_Load(object sender, EventArgs e)
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

                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    if (tempConn == null)
                        return;

                    tempConn.Open();

                    // Update status inpatients yang sudah melewati discharge date
                    List<int> roomsToUpdate = new List<int>();

                    string getRoomsQuery = @"SELECT RoomID 
                                   FROM Inpatients 
                                   WHERE Status = 'Active' 
                                   AND DischargeDate <= CAST(GETDATE() AS DATE)";

                    using (SqlCommand getRoomsCmd = new SqlCommand(getRoomsQuery, tempConn))
                    using (SqlDataReader reader = getRoomsCmd.ExecuteReader())
                    {
                        while (reader != null && reader.Read())
                        {
                            roomsToUpdate.Add(reader.GetInt32(0));
                        }
                    }

                    string updateInpatientQuery = @"UPDATE Inpatients 
                                          SET Status = 'Discharged',
                                              UpdatedAt = GETDATE()
                                          WHERE Status = 'Active' 
                                          AND DischargeDate <= CAST(GETDATE() AS DATE)";

                    using (SqlCommand cmd = new SqlCommand(updateInpatientQuery, tempConn))
                    {
                        if (cmd != null)
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Update status kamar menjadi Available
                    foreach (int roomId in roomsToUpdate)
                    {
                        UpdateRoomStatus(roomId, "Available");
                    }
                }

                // Load data secara aman
                if (!this.IsDisposed && this.IsHandleCreated)
                {
                    LoadPatients();
                    LoadRooms();
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

        private void LoadRooms()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT RoomID, 
                                   CONCAT(RoomNumber, ' - ', RoomType, ' (', Floor, ' Floor)') as DisplayName 
                                   FROM Rooms 
                                   WHERE Status = 'Available'
                                   ORDER BY RoomNumber";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, tempConn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        guna2ComboBox2.DataSource = dt;
                        guna2ComboBox2.DisplayMember = "DisplayName";
                        guna2ComboBox2.ValueMember = "RoomID";
                        guna2ComboBox2.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading rooms: " + ex.Message);
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

                    string query = @"SELECT i.InpatientID,
                          p.Name AS PatientName,
                          p.MedicalRecordNumber AS MRN,
                          r.RoomNumber,
                          r.RoomType,
                          FORMAT(i.AdmissionDate, 'dd/MM/yyyy') AS AdmissionDate,
                          FORMAT(i.DischargeDate, 'dd/MM/yyyy') AS DischargeDate,
                          i.Status,
                          i.Notes
                   FROM Inpatients i
                   INNER JOIN Patients p ON i.PatientID = p.PatientID
                   INNER JOIN Rooms r ON i.RoomID = r.RoomID
                   ORDER BY i.InpatientID ASC";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        tempConn.Open();
                        adapter.Fill(dt);

                        if (!this.IsDisposed && this.IsHandleCreated && dataGridView1 != null)
                        {
                            dataGridView1.DataSource = dt;

                            // Configure columns safely
                            foreach (DataGridViewColumn col in dataGridView1.Columns)
                            {
                                if (col != null)
                                {
                                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                                }
                            }

                            // Set specific column properties safely
                            if (dataGridView1.Columns["InpatientID"] != null)
                            {
                                dataGridView1.Columns["InpatientID"].HeaderText = "ID";
                                dataGridView1.Columns["InpatientID"].Width = 50;
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


        private void UpdateRoomStatus(int roomId, string status)
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = "UPDATE Rooms SET Status = @Status WHERE RoomID = @RoomID";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@RoomID", roomId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating room status: " + ex.Message);
            }
        }

        private void ClearFields()
        {
            guna2ComboBox1.SelectedIndex = -1;
            guna2ComboBox2.SelectedIndex = -1;
            guna2DateTimePicker1.Value = DateTime.Now;
            guna2DateTimePicker2.Value = DateTime.Now;
            guna2TextBox5.Text = "Active";
            guna2TextBox6.Clear();
        }

        private void guna2DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                // Update status based on discharge date
                if (guna2DateTimePicker1.Value.Date <= DateTime.Now.Date)
                {
                    guna2TextBox5.Text = "Discharged";  // Ubah dari "Discharge" ke "Discharged"
                }
                else
                {
                    guna2TextBox5.Text = "Active";
                }
                UpdateStatusColor(guna2TextBox5);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating status: " + ex.Message);
            }
        }

        private void UpdateStatusColor(Guna.UI2.WinForms.Guna2TextBox statusTextBox)
        {
            if (statusTextBox.Text.ToLower() == "active")
            {
                statusTextBox.ForeColor = Color.Green;
                statusTextBox.Font = new Font(statusTextBox.Font, FontStyle.Bold);
            }
            else if (statusTextBox.Text.ToLower() == "discharged")  // Ubah dari "discharge" ke "discharged"
            {
                statusTextBox.ForeColor = Color.Red;
                statusTextBox.Font = new Font(statusTextBox.Font, FontStyle.Bold);
            }
            else
            {
                statusTextBox.ForeColor = Color.Black;
                statusTextBox.Font = new Font(statusTextBox.Font, FontStyle.Regular);
            }
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString().ToLower();
                if (status == "active")
                {
                    e.CellStyle.BackColor = Color.LightGreen;
                }
                else if (status == "discharged")  // Ubah dari "discharge" ke "discharged"
                {
                    e.CellStyle.BackColor = Color.Pink;
                }
            }
        }

        private void UpdateInpatientStatuses()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();

                    // Dapatkan daftar kamar yang perlu diupdate statusnya
                    string getRoomsQuery = @"SELECT RoomID 
                                   FROM Inpatients 
                                   WHERE Status = 'Active' 
                                   AND DischargeDate <= CAST(GETDATE() AS DATE)";

                    List<int> roomsToUpdate = new List<int>();
                    using (SqlCommand getRoomsCmd = new SqlCommand(getRoomsQuery, tempConn))
                    {
                        using (SqlDataReader reader = getRoomsCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                roomsToUpdate.Add(reader.GetInt32(0));
                            }
                        }
                    }

                    // Update status inpatient
                    string updateInpatientQuery = @"UPDATE Inpatients 
                                          SET Status = 'Discharged',
                                              UpdatedAt = GETDATE()
                                          WHERE Status = 'Active' 
                                          AND DischargeDate <= CAST(GETDATE() AS DATE)";

                    using (SqlCommand cmd = new SqlCommand(updateInpatientQuery, tempConn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Update status kamar menjadi Available
                    foreach (int roomId in roomsToUpdate)
                    {
                        UpdateRoomStatus(roomId, "Available");
                    }

                    // Refresh data grid dan rooms
                    RefreshDataGridView();
                    LoadRooms();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating statuses: " + ex.Message);
            }
        }

        private void guna2GradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
