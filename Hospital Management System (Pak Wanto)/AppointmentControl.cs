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
    public partial class AppointmentControl : UserControl
    {
        private SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-9SGQJSHQ\SQLEXPRESS;Initial Catalog=DB_Hospital;Integrated Security=True;");

        public AppointmentControl()
        {
            InitializeComponent();

            // Set default values
            guna2DateTimePicker1.Value = DateTime.Today;
            guna2DateTimePicker2.Value = DateTime.Now;
            guna2TextBox4.Text = "Scheduled";

            // Set timer interval (1 menit)
            timer1.Interval = 60000;
            timer1.Enabled = true;

            // Subscribe to events
            timer1.Tick += timer1_Tick;
            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
        }

        private void AppointmentControl_Load(object sender, EventArgs e)
        {
            try
            {
                LoadPatients();
                LoadDoctors();
                LoadDepartments();
                RefreshDataGridView();
                UpdateAvailableDoctorsCount();
                UpdateTodayAppointmentsCount();
                UpdateExpiredAppointmentsStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading appointment control: " + ex.Message);
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

        private void LoadDoctors()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT DoctorID, 
                           'Dr. ' + Name + ' - ' + Specialization as DisplayName 
                           FROM Doctors 
                           ORDER BY Name";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, tempConn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        guna2ComboBox2.DataSource = dt;
                        guna2ComboBox2.DisplayMember = "DisplayName";
                        guna2ComboBox2.ValueMember = "DoctorID";
                        guna2ComboBox2.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading doctors: " + ex.Message);
            }
        }

        private void LoadDepartments()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = "SELECT DepartmentID, DepartmentName FROM Departments ORDER BY DepartmentName";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, tempConn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        guna2ComboBox3.DataSource = dt;
                        guna2ComboBox3.DisplayMember = "DepartmentName";
                        guna2ComboBox3.ValueMember = "DepartmentID";
                        guna2ComboBox3.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading departments: " + ex.Message);
            }
        }

        private void UpdateAvailableDoctorsCount()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT COUNT(*) 
                           FROM Doctors d
                           WHERE NOT EXISTS (
                               SELECT 1 FROM Appointments a
                               WHERE a.DoctorID = d.DoctorID
                               AND CAST(a.AppointmentDate AS DATE) = CAST(GETDATE() AS DATE)
                               AND a.Status = 'Scheduled'
                           )";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        int availableDoctors = (int)cmd.ExecuteScalar();
                        guna2HtmlLabel10.Text = availableDoctors.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating available doctors count: " + ex.Message);
            }
        }

        private void UpdateTodayAppointmentsCount()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT COUNT(*) 
                           FROM Appointments 
                           WHERE CAST(AppointmentDate AS DATE) = CAST(GETDATE() AS DATE)
                           AND Status = 'Scheduled'";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        int todayAppointments = (int)cmd.ExecuteScalar();
                        guna2HtmlLabel8.Text = todayAppointments.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating today's appointments count: " + ex.Message);
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
               a.AppointmentID, 
               p.Name AS PatientName,
               p.MedicalRecordNumber AS MRN,
               d.Name AS DoctorName,
               dep.DepartmentName,
               FORMAT(a.AppointmentDate, 'dd/MM/yyyy') AS AppointmentDate,
               FORMAT(CAST(a.AppointmentTime AS DATETIME), 'HH:mm') AS AppointmentTime,
               a.Status,
               a.Notes
               FROM Appointments a
               INNER JOIN Patients p ON a.PatientID = p.PatientID
               INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
               INNER JOIN Departments dep ON a.DepartmentID = dep.DepartmentID
               ORDER BY a.AppointmentDate ASC, a.AppointmentTime ASC";  // Changed to ASC for ascending order

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, tempConn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridView1.DataSource = dt;

                        // Set column headers
                        if (dataGridView1.Columns["AppointmentID"] != null)
                            dataGridView1.Columns["AppointmentID"].HeaderText = "ID";
                        if (dataGridView1.Columns["PatientName"] != null)
                            dataGridView1.Columns["PatientName"].HeaderText = "Patient Name";
                        if (dataGridView1.Columns["MRN"] != null)
                            dataGridView1.Columns["MRN"].HeaderText = "MRN";
                        if (dataGridView1.Columns["DoctorName"] != null)
                            dataGridView1.Columns["DoctorName"].HeaderText = "Doctor";
                        if (dataGridView1.Columns["DepartmentName"] != null)
                            dataGridView1.Columns["DepartmentName"].HeaderText = "Department";
                        if (dataGridView1.Columns["AppointmentDate"] != null)
                            dataGridView1.Columns["AppointmentDate"].HeaderText = "Date";
                        if (dataGridView1.Columns["AppointmentTime"] != null)
                            dataGridView1.Columns["AppointmentTime"].HeaderText = "Time";
                        if (dataGridView1.Columns["Status"] != null)
                            dataGridView1.Columns["Status"].HeaderText = "Status";
                        if (dataGridView1.Columns["Notes"] != null)
                            dataGridView1.Columns["Notes"].HeaderText = "Notes";

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


        private void CheckDoctorAvailability(int doctorId, DateTime appointmentDate, TimeSpan appointmentTime, int currentAppointmentId = 0)
        {
            using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
            {
                try
                {
                    tempConn.Open();
                    string query = @"SELECT COUNT(*) 
                           FROM Appointments 
                           WHERE DoctorID = @DoctorID 
                           AND AppointmentDate = @AppointmentDate 
                           AND AppointmentTime = @AppointmentTime
                           AND Status = 'Scheduled'
                           AND (@CurrentAppointmentID = 0 OR AppointmentID != @CurrentAppointmentID)";

                    SqlCommand cmd = new SqlCommand(query, tempConn);
                    cmd.Parameters.AddWithValue("@DoctorID", doctorId);
                    cmd.Parameters.AddWithValue("@AppointmentDate", appointmentDate.Date);
                    cmd.Parameters.AddWithValue("@AppointmentTime", appointmentTime);
                    cmd.Parameters.AddWithValue("@CurrentAppointmentID", currentAppointmentId);

                    int existingAppointments = (int)cmd.ExecuteScalar();
                    if (existingAppointments > 0)
                    {
                        throw new Exception("Doctor is not available at the selected time!");
                    }
                }
                finally
                {
                    if (tempConn.State == ConnectionState.Open)
                        tempConn.Close();
                }
            }
        }

        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            try
            {
                string searchText = guna2TextBox9.Text.Trim();
                conn.Open();
                string query = @"SELECT a.AppointmentID, 
                           p.Name AS PatientName,
                           p.MedicalRecordNumber AS MRN,
                           d.Name AS DoctorName,
                           dep.DepartmentName,
                           FORMAT(a.AppointmentDate, 'dd/MM/yyyy') AS AppointmentDate,
                           FORMAT(CAST(a.AppointmentTime AS DATETIME), 'HH:mm') AS AppointmentTime,
                           a.Status,
                           a.Notes
                           FROM Appointments a
                           JOIN Patients p ON a.PatientID = p.PatientID
                           JOIN Doctors d ON a.DoctorID = d.DoctorID
                           JOIN Departments dep ON a.DepartmentID = dep.DepartmentID
                           WHERE p.Name LIKE @Search 
                           OR p.MedicalRecordNumber LIKE @Search
                           OR d.Name LIKE @Search 
                           OR dep.DepartmentName LIKE @Search
                           OR a.Status LIKE @Search
                           ORDER BY a.AppointmentID ASC";  // Mengubah pengurutan berdasarkan AppointmentID

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

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                    // Find and select the patient in ComboBox1
                    string patientName = row.Cells["PatientName"].Value?.ToString();
                    foreach (DataRowView drv in guna2ComboBox1.Items)
                    {
                        if (drv["DisplayName"].ToString().Contains(patientName))
                        {
                            guna2ComboBox1.SelectedValue = drv["PatientID"];
                            break;
                        }
                    }

                    // Find and select the doctor in ComboBox2
                    string doctorName = row.Cells["DoctorName"].Value?.ToString();
                    foreach (DataRowView drv in guna2ComboBox2.Items)
                    {
                        if (drv["DisplayName"].ToString().Contains(doctorName))
                        {
                            guna2ComboBox2.SelectedValue = drv["DoctorID"];
                            break;
                        }
                    }

                    // Find and select the department in ComboBox3
                    string departmentName = row.Cells["DepartmentName"].Value?.ToString();
                    foreach (DataRowView drv in guna2ComboBox3.Items)
                    {
                        if (drv["DepartmentName"].ToString() == departmentName)
                        {
                            guna2ComboBox3.SelectedValue = drv["DepartmentID"];
                            break;
                        }
                    }

                    // Set date and time
                    if (DateTime.TryParse(row.Cells["AppointmentDate"].Value?.ToString(), out DateTime appointmentDate))
                    {
                        guna2DateTimePicker1.Value = appointmentDate;
                    }

                    if (DateTime.TryParse(row.Cells["AppointmentTime"].Value?.ToString(), out DateTime appointmentTime))
                    {
                        guna2DateTimePicker2.Value = DateTime.Today.Add(appointmentTime.TimeOfDay);
                    }

                    guna2TextBox4.Text = row.Cells["Status"].Value?.ToString() ?? "Scheduled";
                    guna2TextBox5.Text = row.Cells["Notes"].Value?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow == null)
                {
                    MessageBox.Show("Please select an appointment to delete!");
                    return;
                }

                if (MessageBox.Show("Are you sure you want to delete this appointment?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return;
                }

                conn.Open();
                string query = "DELETE FROM Appointments WHERE AppointmentID = @AppointmentID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@AppointmentID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["AppointmentID"].Value));
                cmd.ExecuteNonQuery();

                MessageBox.Show("Appointment deleted successfully!");
                ClearFields();
                RefreshDataGridView();
                UpdateAvailableDoctorsCount();
                UpdateTodayAppointmentsCount();
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
            using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
            {
                try
                {
                    if (dataGridView1.CurrentRow == null)
                    {
                        MessageBox.Show("Please select an appointment to update!");
                        return;
                    }

                    // Validasi
                    if (guna2ComboBox1.SelectedValue == null ||
                        guna2ComboBox2.SelectedValue == null ||
                        guna2ComboBox3.SelectedValue == null)
                    {
                        MessageBox.Show("Please select Patient, Doctor, and Department!");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(guna2TextBox4.Text))
                    {
                        MessageBox.Show("Status is required!");
                        return;
                    }

                    int currentAppointmentId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["AppointmentID"].Value);

                    // Check availability using the new method
                    CheckDoctorAvailability(
                        Convert.ToInt32(guna2ComboBox2.SelectedValue),
                        guna2DateTimePicker1.Value.Date,
                        guna2DateTimePicker2.Value.TimeOfDay,
                        currentAppointmentId);

                    tempConn.Open();
                    string query = @"UPDATE Appointments SET 
                           PatientID = @PatientID,
                           DoctorID = @DoctorID,
                           DepartmentID = @DepartmentID,
                           AppointmentDate = @AppointmentDate,
                           AppointmentTime = @AppointmentTime,
                           Status = @Status,
                           Notes = @Notes,
                           UpdatedAt = GETDATE()
                           WHERE AppointmentID = @AppointmentID";

                    SqlCommand cmd = new SqlCommand(query, tempConn);
                    cmd.Parameters.AddWithValue("@AppointmentID", currentAppointmentId);
                    cmd.Parameters.AddWithValue("@PatientID", guna2ComboBox1.SelectedValue);
                    cmd.Parameters.AddWithValue("@DoctorID", guna2ComboBox2.SelectedValue);
                    cmd.Parameters.AddWithValue("@DepartmentID", guna2ComboBox3.SelectedValue);
                    cmd.Parameters.AddWithValue("@AppointmentDate", guna2DateTimePicker1.Value.Date);
                    cmd.Parameters.AddWithValue("@AppointmentTime", guna2DateTimePicker2.Value.TimeOfDay);
                    cmd.Parameters.AddWithValue("@Status", guna2TextBox4.Text.Trim());
                    cmd.Parameters.AddWithValue("@Notes", guna2TextBox5.Text.Trim());

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Appointment updated successfully!");
                    ClearFields();
                    RefreshDataGridView();
                    UpdateAvailableDoctorsCount();
                    UpdateTodayAppointmentsCount();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    if (tempConn.State == ConnectionState.Open)
                        tempConn.Close();
                }
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Validasi
                if (guna2ComboBox1.SelectedValue == null ||
                    guna2ComboBox2.SelectedValue == null ||
                    guna2ComboBox3.SelectedValue == null)
                {
                    MessageBox.Show("Please select Patient, Doctor, and Department!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(guna2TextBox4.Text))
                {
                    MessageBox.Show("Status is required!");
                    return;
                }

                if (guna2DateTimePicker1.Value.Date < DateTime.Today)
                {
                    MessageBox.Show("Cannot schedule appointment for past dates!");
                    return;
                }

                conn.Open();

                // Check doctor availability using CheckDoctorAvailability method
                try
                {
                    CheckDoctorAvailability(
                        Convert.ToInt32(guna2ComboBox2.SelectedValue),
                        guna2DateTimePicker1.Value.Date,
                        guna2DateTimePicker2.Value.TimeOfDay);
                }
                catch (Exception availabilityEx)
                {
                    MessageBox.Show(availabilityEx.Message);
                    return;
                }

                string query = @"INSERT INTO Appointments 
                       (PatientID, DoctorID, DepartmentID, AppointmentDate, 
                        AppointmentTime, Status, Notes, CreatedAt, UpdatedAt) 
                       VALUES 
                       (@PatientID, @DoctorID, @DepartmentID, @AppointmentDate, 
                        @AppointmentTime, @Status, @Notes, GETDATE(), GETDATE())";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PatientID", guna2ComboBox1.SelectedValue);
                cmd.Parameters.AddWithValue("@DoctorID", guna2ComboBox2.SelectedValue);
                cmd.Parameters.AddWithValue("@DepartmentID", guna2ComboBox3.SelectedValue);
                cmd.Parameters.AddWithValue("@AppointmentDate", guna2DateTimePicker1.Value.Date);
                cmd.Parameters.AddWithValue("@AppointmentTime", guna2DateTimePicker2.Value.TimeOfDay);
                cmd.Parameters.AddWithValue("@Status", guna2TextBox4.Text.Trim());
                cmd.Parameters.AddWithValue("@Notes", guna2TextBox5.Text.Trim());

                cmd.ExecuteNonQuery();
                MessageBox.Show("Appointment scheduled successfully!");
                ClearFields();
                RefreshDataGridView();
                UpdateAvailableDoctorsCount();
                UpdateTodayAppointmentsCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        private void ClearFields()
        {
            guna2ComboBox1.SelectedIndex = -1; // Patient
            guna2ComboBox2.SelectedIndex = -1; // Doctor
            guna2ComboBox3.SelectedIndex = -1; // Department
            guna2DateTimePicker1.Value = DateTime.Now;
            guna2DateTimePicker2.Value = DateTime.Now;
            guna2TextBox4.Text = "Scheduled"; // Default status
            guna2TextBox5.Clear(); // Notes
        }

        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Optional: Update departemen secara otomatis berdasarkan dokter yang dipilih
            if (guna2ComboBox2.SelectedValue != null)
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT TOP 1 d.DepartmentID 
                                   FROM Departments d 
                                   JOIN Appointments a ON d.DepartmentID = a.DepartmentID
                                   WHERE a.DoctorID = @DoctorID
                                   GROUP BY d.DepartmentID
                                   ORDER BY COUNT(*) DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@DoctorID", guna2ComboBox2.SelectedValue);
                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        guna2ComboBox3.SelectedValue = result;
                    }
                }
                catch (Exception ex)
                {
                    // Silently handle error
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Check if we're formatting the Status column
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString().ToLower();

                switch (status)
                {
                    case "scheduled":
                        e.CellStyle.BackColor = Color.LightBlue;
                        break;
                    case "cancelled":
                        e.CellStyle.BackColor = Color.LightCoral;
                        break;
                    case "completed":
                        e.CellStyle.BackColor = Color.LightGreen;
                        break;
                    default:
                        // Reset to default background color if status doesn't match
                        e.CellStyle.BackColor = dataGridView1.DefaultCellStyle.BackColor;
                        break;
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateExpiredAppointmentsStatus();
        }

        private void UpdateExpiredAppointmentsStatus()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"
                UPDATE Appointments
                SET Status = 'Completed'
                WHERE Status = 'Scheduled' 
                AND (
                    AppointmentDate < CAST(GETDATE() AS DATE)
                    OR 
                    (AppointmentDate = CAST(GETDATE() AS DATE) AND AppointmentTime < CAST(GETDATE() AS TIME))
                )";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        int updatedCount = cmd.ExecuteNonQuery();

                        if (updatedCount > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"{updatedCount} appointments automatically marked as Completed.");
                            RefreshDataGridView();
                            UpdateAvailableDoctorsCount();
                            UpdateTodayAppointmentsCount();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating expired appointments: " + ex.Message);
            }
        }
    }
}
