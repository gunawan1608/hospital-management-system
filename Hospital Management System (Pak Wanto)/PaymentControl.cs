using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hospital_Management_System__Pak_Wanto_
{
    public partial class PaymentControl : UserControl
    {
        private SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-9SGQJSHQ\SQLEXPRESS;Initial Catalog=DB_Hospital;Integrated Security=True;");

        public PaymentControl()
        {
            InitializeComponent();

            // Set default values
            guna2DateTimePicker1.Value = DateTime.Now;
            guna2TextBox2.Text = "Pending";

            // Set payment methods
            guna2ComboBox4.Items.AddRange(new string[] { "Insurance", "Credit Card", "Cash", "Bank Transfer" });

            // Subscribe to events
            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
        }

        private void PaymentControl_Load(object sender, EventArgs e)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            try
            {
                // Set timer interval (1 menit)
                timer1.Interval = 60000; // Check setiap 1 menit
                timer1.Enabled = true;

                // Configure DataGridView
                if (dataGridView1 != null)
                {
                    dataGridView1.MultiSelect = false;
                    dataGridView1.ReadOnly = true;
                    dataGridView1.AllowUserToAddRows = false;
                    dataGridView1.AllowUserToDeleteRows = false;
                    // Hapus atau ubah menjadi true
                    dataGridView1.AutoGenerateColumns = true;
                    dataGridView1.CellContentClick += dataGridView1_CellContentClick;
                }

                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    if (tempConn == null)
                        return;

                    tempConn.Open();

                    // Update status untuk pembayaran yang telah lebih dari 2 hari pending
                    string updateQuery = @"UPDATE Payments 
                                         SET Status = 'Failed',
                                             UpdatedAt = GETDATE()
                                         WHERE Status = 'Pending' 
                                         AND DATEDIFF(day, PaymentDate, GETDATE()) >= 2";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, tempConn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    RefreshDataGridView();
                }

                // Load data secara aman
                if (!this.IsDisposed && this.IsHandleCreated)
                {
                    LoadPatients();
                    LoadAppointments();
                    LoadInpatients();
                    RefreshDataGridView();
                    UpdateDashboardCounts();

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
                    MessageBox.Show("Error loading payment control: " + ex.Message);
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

        private void LoadAppointments()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT AppointmentID, 
                           CONCAT('APT-', AppointmentID, ' (', p.Name, ')') as DisplayName 
                           FROM Appointments a
                           JOIN Patients p ON a.PatientID = p.PatientID
                           ORDER BY AppointmentID DESC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, tempConn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        guna2ComboBox2.DataSource = dt;
                        guna2ComboBox2.DisplayMember = "DisplayName";
                        guna2ComboBox2.ValueMember = "AppointmentID";
                        guna2ComboBox2.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading appointments: " + ex.Message);
            }
        }

        private void LoadInpatients()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT InpatientID, 
                           CONCAT('INP-', InpatientID, ' (', p.Name, ')') as DisplayName 
                           FROM Inpatients i
                           JOIN Patients p ON i.PatientID = p.PatientID
                           ORDER BY InpatientID DESC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, tempConn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        guna2ComboBox3.DataSource = dt;
                        guna2ComboBox3.DisplayMember = "DisplayName";
                        guna2ComboBox3.ValueMember = "InpatientID";
                        guna2ComboBox3.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading inpatients: " + ex.Message);
            }
        }

        private void RefreshDataGridView()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT p.PaymentID,
                   pat.Name AS PatientName,
                   pat.MedicalRecordNumber AS MRN,
                   COALESCE('APT-' + CAST(p.AppointmentID AS VARCHAR), 'INP-' + CAST(p.InpatientID AS VARCHAR)) AS ReferenceID,
                   FORMAT(p.PaymentDate, 'dd/MM/yyyy') AS PaymentDate,
                   FORMAT(p.Amount, 'N0') AS Amount,
                   p.PaymentMethod,
                   p.Status,
                   p.Notes
                   FROM Payments p
                   JOIN Patients pat ON p.PatientID = pat.PatientID
                   ORDER BY p.PaymentID ASC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, tempConn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridView1.DataSource = dt;

                        // Hapus kolom "Action" jika sudah ada sebelumnya
                        if (dataGridView1.Columns.Contains("Action"))
                        {
                            dataGridView1.Columns.Remove("Action");
                        }

                        // Tambahkan kolom tombol setelah mengatur DataSource
                        DataGridViewButtonColumn printButtonColumn = new DataGridViewButtonColumn();
                        printButtonColumn.Name = "Action"; // Pastikan nama kolom sesuai
                        printButtonColumn.HeaderText = "Action";
                        printButtonColumn.Text = "Print Receipt";
                        printButtonColumn.UseColumnTextForButtonValue = true; // Menggunakan teks kolom untuk semua baris
                        dataGridView1.Columns.Add(printButtonColumn);

                        // Set column headers
                        if (dataGridView1.Columns["PaymentID"] != null)
                            dataGridView1.Columns["PaymentID"].HeaderText = "ID";
                        if (dataGridView1.Columns["PatientName"] != null)
                            dataGridView1.Columns["PatientName"].HeaderText = "Patient Name";
                        if (dataGridView1.Columns["MRN"] != null)
                            dataGridView1.Columns["MRN"].HeaderText = "MRN";
                        if (dataGridView1.Columns["ReferenceID"] != null)
                            dataGridView1.Columns["ReferenceID"].HeaderText = "Reference";
                        if (dataGridView1.Columns["PaymentDate"] != null)
                            dataGridView1.Columns["PaymentDate"].HeaderText = "Date";
                        if (dataGridView1.Columns["Amount"] != null)
                            dataGridView1.Columns["Amount"].HeaderText = "Amount";
                        if (dataGridView1.Columns["PaymentMethod"] != null)
                            dataGridView1.Columns["PaymentMethod"].HeaderText = "Method";
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

        private void UpdateDashboardCounts()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();

                    // Total Revenue Today (hanya untuk pembayaran yang Completed)
                    string revenueQuery = @"SELECT ISNULL(SUM(Amount), 0)
                                    FROM Payments
                                    WHERE CAST(PaymentDate AS DATE) = CAST(GETDATE() AS DATE)
                                    AND Status = 'Completed'";

                    using (SqlCommand cmd = new SqlCommand(revenueQuery, tempConn))
                    {
                        decimal totalRevenueToday = Convert.ToDecimal(cmd.ExecuteScalar());
                        guna2HtmlLabel5.Text = string.Format("Rp {0:N0}", totalRevenueToday); // Angka utama
                        guna2HtmlLabel15.Text = "Total Revenue Today"; // Catatan kecil
                    }

                    // Total Successful Payments (semua yang Completed)
                    string successQuery = @"SELECT ISNULL(SUM(Amount), 0) AS TotalAmount,
                                    COUNT(*) AS SuccessfulCount
                                    FROM Payments
                                    WHERE Status = 'Completed'";

                    using (SqlCommand cmd = new SqlCommand(successQuery, tempConn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                decimal totalSuccessfulAmount = reader.GetDecimal(0); // Total amount for successful payments
                                int successCount = reader.GetInt32(1); // Count of successful payments

                                guna2HtmlLabel7.Text = string.Format("Rp {0:N0}", totalSuccessfulAmount); // Angka utama
                                guna2HtmlLabel17.Text = successCount + " transactions completed"; // Catatan kecil
                            }
                        }
                    }

                    // Total Pending Payments (dalam bentuk Rp)
                    string pendingQuery = @"SELECT ISNULL(SUM(Amount), 0) AS TotalPending,
                                    COUNT(*) AS PendingCount
                                    FROM Payments
                                    WHERE Status = 'Pending'";

                    using (SqlCommand cmd = new SqlCommand(pendingQuery, tempConn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                decimal totalPending = reader.GetDecimal(0); // Total amount for pending payments
                                int pendingCount = reader.GetInt32(1); // Count of pending payments

                                guna2HtmlLabel6.Text = string.Format("Rp {0:N0}", totalPending); // Angka utama
                                guna2HtmlLabel16.Text = pendingCount + " invoices pending"; // Catatan kecil
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating dashboard: " + ex.Message);
            }
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow == null)
                {
                    MessageBox.Show("Please select a payment to delete!");
                    return;
                }

                if (MessageBox.Show("Are you sure you want to delete this payment?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return;
                }

                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = "DELETE FROM Payments WHERE PaymentID = @PaymentID";
                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@PaymentID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["PaymentID"].Value));
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Payment deleted successfully!");
                ClearFields();
                RefreshDataGridView();
                UpdateDashboardCounts();
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
                    MessageBox.Show("Please select a payment to update!");
                    return;
                }

                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"UPDATE Payments SET 
                           PatientID = @PatientID,
                           AppointmentID = @AppointmentID,
                           InpatientID = @InpatientID,
                           PaymentDate = @PaymentDate,
                           Amount = @Amount,
                           PaymentMethod = @PaymentMethod,
                           Status = @Status,
                           Notes = @Notes,
                           UpdatedAt = GETDATE()
                           WHERE PaymentID = @PaymentID";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@PaymentID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["PaymentID"].Value));
                        cmd.Parameters.AddWithValue("@PatientID", guna2ComboBox1.SelectedValue);
                        cmd.Parameters.AddWithValue("@AppointmentID", (guna2ComboBox2.SelectedValue == null) ? DBNull.Value : guna2ComboBox2.SelectedValue);
                        cmd.Parameters.AddWithValue("@InpatientID", (guna2ComboBox3.SelectedValue == null) ? DBNull.Value : guna2ComboBox3.SelectedValue);
                        cmd.Parameters.AddWithValue("@PaymentDate", guna2DateTimePicker1.Value);
                        cmd.Parameters.AddWithValue("@Amount", Convert.ToDecimal(guna2TextBox1.Text));
                        cmd.Parameters.AddWithValue("@PaymentMethod", guna2ComboBox4.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@Status", guna2TextBox2.Text);
                        cmd.Parameters.AddWithValue("@Notes", guna2TextBox3.Text);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Payment updated successfully!");
                ClearFields();
                RefreshDataGridView();
                UpdateDashboardCounts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (guna2ComboBox1.SelectedValue == null)
                {
                    MessageBox.Show("Please select a patient!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(guna2TextBox1.Text))
                {
                    MessageBox.Show("Amount is required!");
                    return;
                }

                if (guna2ComboBox4.SelectedItem == null)
                {
                    MessageBox.Show("Please select a payment method!");
                    return;
                }

                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"INSERT INTO Payments 
                           (PatientID, AppointmentID, InpatientID, PaymentDate, 
                            Amount, PaymentMethod, Status, Notes, CreatedAt, UpdatedAt)
                           VALUES 
                           (@PatientID, @AppointmentID, @InpatientID, @PaymentDate,
                            @Amount, @PaymentMethod, @Status, @Notes, GETDATE(), GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@PatientID", guna2ComboBox1.SelectedValue);
                        cmd.Parameters.AddWithValue("@AppointmentID", (guna2ComboBox2.SelectedValue == null) ? DBNull.Value : guna2ComboBox2.SelectedValue);
                        cmd.Parameters.AddWithValue("@InpatientID", (guna2ComboBox3.SelectedValue == null) ? DBNull.Value : guna2ComboBox3.SelectedValue);
                        cmd.Parameters.AddWithValue("@PaymentDate", guna2DateTimePicker1.Value);
                        cmd.Parameters.AddWithValue("@Amount", Convert.ToDecimal(guna2TextBox1.Text));
                        cmd.Parameters.AddWithValue("@PaymentMethod", guna2ComboBox4.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@Status", guna2TextBox2.Text);
                        cmd.Parameters.AddWithValue("@Notes", guna2TextBox3.Text);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Payment added successfully!");
                ClearFields();
                RefreshDataGridView();
                UpdateDashboardCounts();
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

                    using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                    {
                        tempConn.Open();
                        string query = @"SELECT * FROM Payments WHERE PaymentID = @PaymentID";

                        using (SqlCommand cmd = new SqlCommand(query, tempConn))
                        {
                            cmd.Parameters.AddWithValue("@PaymentID", Convert.ToInt32(row.Cells["PaymentID"].Value));
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    guna2ComboBox1.SelectedValue = reader["PatientID"];
                                    
                                    if (reader["AppointmentID"] != DBNull.Value)
                                        guna2ComboBox2.SelectedValue = reader["AppointmentID"];
                                    else
                                        guna2ComboBox2.SelectedIndex = -1;

                                    if (reader["InpatientID"] != DBNull.Value)
                                        guna2ComboBox3.SelectedValue = reader["InpatientID"];
                                    else
                                        guna2ComboBox3.SelectedIndex = -1;

                                    guna2DateTimePicker1.Value = Convert.ToDateTime(reader["PaymentDate"]);
                                    guna2TextBox1.Text = reader["Amount"].ToString();
                                    guna2ComboBox4.SelectedItem = reader["PaymentMethod"].ToString();
                                    guna2TextBox2.Text = reader["Status"].ToString();
                                    guna2TextBox3.Text = reader["Notes"].ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading payment details: " + ex.Message);
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
                    string query = @"SELECT p.PaymentID,
                           pat.Name AS PatientName,
                           pat.MedicalRecordNumber AS MRN,
                           COALESCE('APT-' + CAST(p.AppointmentID AS VARCHAR), 'INP-' + CAST(p.InpatientID AS VARCHAR)) AS ReferenceID,
                           FORMAT(p.PaymentDate, 'dd/MM/yyyy') AS PaymentDate,
                           FORMAT(p.Amount, 'N0') AS Amount,
                           p.PaymentMethod,
                           p.Status,
                           p.Notes
                           FROM Payments p
                           JOIN Patients pat ON p.PatientID = pat.PatientID
                           WHERE pat.Name LIKE @Search 
                           OR pat.MedicalRecordNumber LIKE @Search
                           OR p.Status LIKE @Search
                           OR p.PaymentMethod LIKE @Search
                           ORDER BY p.PaymentID ASC";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@Search", "%" + searchText + "%");
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dataGridView1.DataSource = dt;
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
            UpdateDashboardCounts(); // Call the method to refresh the dashboard data
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString().ToLower();

                switch (status)
                {
                    case "completed":
                        e.CellStyle.BackColor = Color.LightGreen;
                        break;
                    case "pending":
                        e.CellStyle.BackColor = Color.White;
                        break;
                    case "failed":
                        e.CellStyle.BackColor = Color.LightCoral;
                        break;
                }
            }
        }


        private void ClearFields()
        {
            guna2ComboBox1.SelectedIndex = -1;
            guna2ComboBox2.SelectedIndex = -1;
            guna2ComboBox3.SelectedIndex = -1;
            guna2DateTimePicker1.Value = DateTime.Now;
            guna2TextBox1.Clear();
            guna2ComboBox4.SelectedIndex = -1;
            guna2TextBox2.Text = "Pending";
            guna2TextBox3.Clear();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView1.Columns["Action"].Index)
            {
                // Ambil PaymentID dari baris yang dipilih
                int paymentId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["PaymentID"].Value);
                PrintReceipt(paymentId);
            }
        }

        private void PrintReceipt(int paymentId)
        {
            try
            {
                // Ambil data dari database
                var receiptData = GetReceiptData(paymentId);

                if (receiptData == null)
                {
                    MessageBox.Show("Payment not found.");
                    return;
                }

                // Konfigurasi dokumen cetak
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrintPage += (s, e) => DrawReceipt(e.Graphics, receiptData);

                // Tampilkan dialog cetak
                PrintDialog printDialog = new PrintDialog
                {
                    Document = printDocument
                };
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error printing receipt: " + ex.Message);
            }
        }

        // Metode untuk mengambil data pembayaran dari database
        private dynamic GetReceiptData(int paymentId)
        {
            using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
            {
                tempConn.Open();
                string query = @"SELECT p.PaymentID, pat.Name AS PatientName, 
                         FORMAT(p.PaymentDate, 'dd/MM/yyyy') AS PaymentDate, 
                         FORMAT(p.Amount, 'N0') AS Amount, 
                         p.PaymentMethod, p.Status, p.Notes
                         FROM Payments p
                         JOIN Patients pat ON p.PatientID = pat.PatientID
                         WHERE p.PaymentID = @PaymentID";

                using (SqlCommand cmd = new SqlCommand(query, tempConn))
                {
                    cmd.Parameters.AddWithValue("@PaymentID", paymentId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new
                            {
                                PaymentID = paymentId, // Tambahkan PaymentID di sini
                                PatientName = reader["PatientName"].ToString(),
                                PaymentDate = reader["PaymentDate"].ToString(),
                                Amount = reader["Amount"].ToString(),
                                PaymentMethod = reader["PaymentMethod"].ToString(),
                                Status = reader["Status"].ToString(),
                                Notes = reader["Notes"].ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }


        // Metode untuk menggambar struk
        private void DrawReceipt(Graphics graphics, dynamic data)
        {
            // Konstanta untuk pengaturan margin dan font
            int marginTop = 20;
            int lineHeight = 30;
            int xLabel = 100;
            int xValue = 250;
            Font titleFont = new Font("Bahnschrift", 16, FontStyle.Bold);
            Font regularFont = new Font("Bahnschrift", 12);
            Font boldFont = new Font("Bahnschrift", 12, FontStyle.Bold);
            Brush brush = Brushes.Black;

            // Header
            graphics.DrawString("VD Hospital", titleFont, brush, xLabel, marginTop);
            graphics.DrawString("Receipt", titleFont, brush, xLabel, marginTop + lineHeight);
            graphics.DrawString($"Date: {data.PaymentDate}", regularFont, brush, xLabel, marginTop + (2 * lineHeight));
            graphics.DrawString($"Payment ID: {data.PaymentID}", regularFont, brush, xLabel, marginTop + (3 * lineHeight));

            // Garis pemisah
            int separatorY = marginTop + (4 * lineHeight);
            graphics.DrawLine(new Pen(brush, 2), xLabel, separatorY, xLabel + 300, separatorY);

            // Informasi pasien dan pembayaran
            int currentY = separatorY + lineHeight;
            graphics.DrawString("Patient Name:", boldFont, brush, xLabel, currentY);
            graphics.DrawString(data.PatientName, regularFont, brush, xValue, currentY);

            currentY += lineHeight;
            graphics.DrawString("Payment Method:", boldFont, brush, xLabel, currentY);
            graphics.DrawString(data.PaymentMethod, regularFont, brush, xValue, currentY);

            currentY += lineHeight;
            graphics.DrawString("Status:", boldFont, brush, xLabel, currentY);
            graphics.DrawString(data.Status, regularFont, brush, xValue, currentY);

            currentY += lineHeight;
            graphics.DrawString("Notes:", boldFont, brush, xLabel, currentY);
            graphics.DrawString(data.Notes, regularFont, brush, xValue, currentY);

            // Total Amount
            currentY += lineHeight * 2;
            graphics.DrawLine(new Pen(brush, 2), xLabel, currentY, xLabel + 300, currentY);
            currentY += lineHeight;
            graphics.DrawString("Total Amount:", boldFont, brush, xLabel, currentY);
            graphics.DrawString(data.Amount, titleFont, brush, xValue, currentY);

            // Footer
            currentY += lineHeight * 2;
            graphics.DrawString("Thank you for choosing VD Hospital!", regularFont, brush, xLabel, currentY);
            graphics.DrawString("For inquiries, please contact us at 0858-8693-4134", regularFont, brush, xLabel, currentY + lineHeight);
        }
    }
}