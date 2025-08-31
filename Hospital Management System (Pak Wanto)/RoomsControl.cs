using Guna.UI2.WinForms;
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
    public partial class RoomsControl : UserControl
    {
        private SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-9SGQJSHQ\SQLEXPRESS;Initial Catalog=DB_Hospital;Integrated Security=True;");

        public RoomsControl()
        {
            InitializeComponent();

            // Set default values - pindahkan ke TextBox status yang benar
            guna2TextBox5.Text = "Available"; // Default Status
        }

        private void RoomsControl_Load(object sender, EventArgs e)
        {
            try
            {
                // Set DataGridView properties untuk seleksi satu cell saja
                dataGridView1.SelectionMode = DataGridViewSelectionMode.CellSelect;
                dataGridView1.MultiSelect = false;

                // Refresh data
                RefreshDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading room control: " + ex.Message);
            }
        }

        private void RefreshDataGridView()
        {
            try
            {
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();
                    string query = @"SELECT RoomID,
                            RoomNumber, 
                            RoomType,
                            Floor,
                            Status,
                            DailyRate,
                            FORMAT(CreatedAt, 'dd/MM/yyyy HH:mm') as CreatedAt,
                            FORMAT(UpdatedAt, 'dd/MM/yyyy HH:mm') as UpdatedAt
                     FROM Rooms
                     ORDER BY RoomID ASC";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, tempConn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error refreshing data: " + ex.Message);
            }
        }


        private void guna2Button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Validasi
                if (string.IsNullOrWhiteSpace(guna2TextBox1.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox2.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox3.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox4.Text))
                {
                    MessageBox.Show("Please fill all required fields!");
                    return;
                }

                // Validate Daily Rate is numeric
                if (!decimal.TryParse(guna2TextBox4.Text, out decimal dailyRate))
                {
                    MessageBox.Show("Daily Rate must be a valid number!");
                    return;
                }

                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();

                    // Check if room number already exists
                    string checkQuery = "SELECT COUNT(1) FROM Rooms WHERE RoomNumber = @RoomNumber";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, tempConn))
                    {
                        checkCmd.Parameters.AddWithValue("@RoomNumber", guna2TextBox1.Text.Trim());
                        int exists = (int)checkCmd.ExecuteScalar();
                        if (exists > 0)
                        {
                            MessageBox.Show("Room Number already exists!");
                            return;
                        }
                    }

                    string query = @"INSERT INTO Rooms 
                                   (RoomNumber, RoomType, Floor, Status, DailyRate, CreatedAt, UpdatedAt)
                                   VALUES 
                                   (@RoomNumber, @RoomType, @Floor, @Status, @DailyRate, GETDATE(), GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@RoomNumber", guna2TextBox1.Text.Trim());
                        cmd.Parameters.AddWithValue("@RoomType", guna2TextBox2.Text.Trim());
                        cmd.Parameters.AddWithValue("@Floor", guna2TextBox3.Text.Trim());
                        cmd.Parameters.AddWithValue("@Status", guna2TextBox4.Text.Trim());
                        cmd.Parameters.AddWithValue("@DailyRate", dailyRate);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Room added successfully!");
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
                    MessageBox.Show("Please select a room to update!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(guna2TextBox1.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox2.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox3.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox4.Text) ||
                    string.IsNullOrWhiteSpace(guna2TextBox5.Text))
                {
                    MessageBox.Show("Please fill all required fields!");
                    return;
                }

                // Validate Daily Rate is numeric
                if (!decimal.TryParse(guna2TextBox4.Text, out decimal dailyRate))
                {
                    MessageBox.Show("Daily Rate must be a valid number!");
                    return;
                }

                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();

                    // Check if room number exists on other records
                    string checkQuery = @"SELECT COUNT(1) FROM Rooms 
                                WHERE RoomNumber = @RoomNumber 
                                AND RoomID != @RoomID";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, tempConn))
                    {
                        checkCmd.Parameters.AddWithValue("@RoomNumber", guna2TextBox1.Text.Trim());
                        checkCmd.Parameters.AddWithValue("@RoomID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["RoomID"].Value));
                        int exists = (int)checkCmd.ExecuteScalar();
                        if (exists > 0)
                        {
                            MessageBox.Show("Room Number already exists!");
                            return;
                        }
                    }

                    string query = @"UPDATE Rooms 
                           SET RoomNumber = @RoomNumber,
                               RoomType = @RoomType,
                               Floor = @Floor,
                               Status = @Status,
                               DailyRate = @DailyRate,
                               UpdatedAt = GETDATE()
                           WHERE RoomID = @RoomID";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@RoomID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["RoomID"].Value));
                        cmd.Parameters.AddWithValue("@RoomNumber", guna2TextBox1.Text.Trim());
                        cmd.Parameters.AddWithValue("@RoomType", guna2TextBox2.Text.Trim());
                        cmd.Parameters.AddWithValue("@Floor", guna2TextBox3.Text.Trim());
                        cmd.Parameters.AddWithValue("@Status", guna2TextBox5.Text.Trim()); // Status sekarang menggunakan guna2TextBox5
                        cmd.Parameters.AddWithValue("@DailyRate", dailyRate); // Daily Rate dari guna2TextBox4

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Room updated successfully!");
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
                    MessageBox.Show("Please select a room to delete!");
                    return;
                }

                if (MessageBox.Show("Are you sure you want to delete this room?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return;
                }

                // Check if room is in use
                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
                {
                    tempConn.Open();

                    string checkQuery = @"SELECT COUNT(1) FROM Inpatients 
                                        WHERE RoomID = @RoomID 
                                        AND Status = 'Active'";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, tempConn))
                    {
                        checkCmd.Parameters.AddWithValue("@RoomID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["RoomID"].Value));
                        int inUse = (int)checkCmd.ExecuteScalar();
                        if (inUse > 0)
                        {
                            MessageBox.Show("Cannot delete room. Room is currently in use!");
                            return;
                        }
                    }

                    string deleteQuery = "DELETE FROM Rooms WHERE RoomID = @RoomID";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, tempConn))
                    {
                        deleteCmd.Parameters.AddWithValue("@RoomID", Convert.ToInt32(dataGridView1.CurrentRow.Cells["RoomID"].Value));
                        deleteCmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Room deleted successfully!");
                ClearFields();
                RefreshDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
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
                    string query = @"SELECT RoomID,
                                          RoomNumber,
                                          RoomType,
                                          Floor,
                                          Status,
                                          DailyRate,
                                          FORMAT(CreatedAt, 'dd/MM/yyyy HH:mm') as CreatedAt,
                                          FORMAT(UpdatedAt, 'dd/MM/yyyy HH:mm') as UpdatedAt
                                   FROM Rooms
                                   WHERE RoomNumber LIKE @Search 
                                   OR RoomType LIKE @Search
                                   OR Floor LIKE @Search
                                   OR Status LIKE @Search
                                   ORDER BY RoomID ASC";

                    using (SqlCommand cmd = new SqlCommand(query, tempConn))
                    {
                        cmd.Parameters.AddWithValue("@Search", "%" + searchText + "%");
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dataGridView1.DataSource = dt;

                            // Set column headers
                            if (dataGridView1.Columns["RoomID"] != null)
                                dataGridView1.Columns["RoomID"].HeaderText = "ID";
                            if (dataGridView1.Columns["RoomNumber"] != null)
                                dataGridView1.Columns["RoomNumber"].HeaderText = "Room No";
                            if (dataGridView1.Columns["RoomType"] != null)
                                dataGridView1.Columns["RoomType"].HeaderText = "Room Type";
                            if (dataGridView1.Columns["Floor"] != null)
                                dataGridView1.Columns["Floor"].HeaderText = "Floor";
                            if (dataGridView1.Columns["Status"] != null)
                                dataGridView1.Columns["Status"].HeaderText = "Status";
                            if (dataGridView1.Columns["DailyRate"] != null)
                                dataGridView1.Columns["DailyRate"].HeaderText = "Daily Rate";
                            if (dataGridView1.Columns["CreatedAt"] != null)
                                dataGridView1.Columns["CreatedAt"].HeaderText = "Created At";
                            if (dataGridView1.Columns["UpdatedAt"] != null)
                                dataGridView1.Columns["UpdatedAt"].HeaderText = "Updated At";

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

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                    guna2TextBox1.Text = row.Cells["RoomNumber"].Value?.ToString();
                    guna2TextBox2.Text = row.Cells["RoomType"].Value?.ToString();
                    guna2TextBox3.Text = row.Cells["Floor"].Value?.ToString();
                    guna2TextBox5.Text = row.Cells["Status"].Value?.ToString(); // Status
                    guna2TextBox4.Text = row.Cells["DailyRate"].Value?.ToString(); // Daily Rate
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void ClearFields()
        {
            guna2TextBox1.Clear(); // Room Number
            guna2TextBox2.Clear(); // Room Type
            guna2TextBox3.Clear(); // Floor
            guna2TextBox4.Text = "Available"; // Default Status
            guna2TextBox5.Clear(); // Daily Rate
        }

        private void ValidateNumericInput(object sender, KeyPressEventArgs e)
        {
            // Mengizinkan hanya angka, backspace, dan decimal point
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // Hanya mengizinkan satu decimal point
            if (e.KeyChar == '.' && ((TextBox)sender).Text.Contains("."))
            {
                e.Handled = true;
            }
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString().ToLower();

                switch (status)
                {
                    case "available":
                        e.CellStyle.BackColor = Color.LightGreen;
                        break;
                    case "occupied":
                        e.CellStyle.BackColor = Color.LightCoral;
                        break;
                    case "maintenance":
                        e.CellStyle.BackColor = Color.LightYellow;
                        break;
                    default:
                        e.CellStyle.BackColor = dataGridView1.DefaultCellStyle.BackColor;
                        break;
                }
            }
        }

        public class Room
        {
            public int RoomID { get; set; }
            public string RoomNumber { get; set; }
            public string RoomType { get; set; }
            public string Floor { get; set; }
            public string Status { get; set; }
            public decimal DailyRate { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
}
