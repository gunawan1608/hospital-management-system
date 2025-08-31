using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Hospital_Management_System__Pak_Wanto_
{
    public partial class Login : Form
    {
        SqlConnection conn = new SqlConnection("Data Source=LAPTOP-9SGQJSHQ\\SQLEXPRESS;Initial Catalog=DB_Hospital;Integrated Security=True;");
        private string loggedInUser;

        public Login()
        {
            InitializeComponent();
            // Initialize username and password fields
            textBox1.Text = "Username";
            textBox1.ForeColor = Color.Silver;

            textBox3.Text = "Password";
            textBox3.ForeColor = Color.Silver;
            textBox3.UseSystemPasswordChar = false;
        }

        private void Login_Load(object sender, EventArgs e)
        {
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Register register = new Register();
            register.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "Username")
            {
                textBox1.Text = "";
                textBox1.ForeColor = Color.Black;
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = "Username";
                textBox1.ForeColor = Color.Silver;
            }
        }

        private void textBox3_Enter(object sender, EventArgs e)
        {
            if (textBox3.Text == "Password")
            {
                textBox3.Text = "";
                textBox3.ForeColor = Color.Black;
                textBox3.UseSystemPasswordChar = !checkBox1.Checked;
            }
        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox3.Text))
            {
                textBox3.Text = "Password";
                textBox3.ForeColor = Color.Silver;
                textBox3.UseSystemPasswordChar = false;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (textBox3.Text != "Password" && !string.IsNullOrWhiteSpace(textBox3.Text))
            {
                textBox3.UseSystemPasswordChar = !checkBox1.Checked;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (textBox3.Text != "Password" && !string.IsNullOrWhiteSpace(textBox3.Text))
            {
                textBox3.ForeColor = Color.Black;
                textBox3.UseSystemPasswordChar = !checkBox1.Checked;
            }
            else if (textBox3.Text == "Password")
            {
                textBox3.ForeColor = Color.Silver;
                textBox3.UseSystemPasswordChar = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Validasi input kosong
            if (textBox1.Text == "Username" || string.IsNullOrWhiteSpace(textBox1.Text) ||
                textBox3.Text == "Password" || string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Buka koneksi ke database
                conn.Open();

                // Query untuk mendapatkan data username dan password dari database
                string query = "SELECT Username, Password FROM Users";
                SqlCommand cmd = new SqlCommand(query, conn);

                SqlDataReader reader = cmd.ExecuteReader();

                bool usernameExists = false;
                bool passwordValid = false;

                while (reader.Read())
                {
                    string dbUsername = reader["Username"].ToString();
                    string dbPassword = reader["Password"].ToString();

                    // Validasi username
                    if (dbUsername.Equals(textBox1.Text.Trim(), StringComparison.Ordinal))
                    {
                        usernameExists = true;

                        // Validasi password hanya jika username benar
                        if (dbPassword.Equals(textBox3.Text.Trim(), StringComparison.Ordinal))
                        {
                            passwordValid = true;
                            loggedInUser = dbUsername; // Simpan nama pengguna yang login
                            break;
                        }
                    }
                }

                reader.Close();

                if (usernameExists && passwordValid)
                {
                    // Jika username dan password benar
                    MessageBox.Show($"Welcome, {loggedInUser}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Menampilkan form utama setelah login
                    Main_Form main_Form = new Main_Form(loggedInUser); // Kirim nama pengguna ke form utama
                    main_Form.Show();
                    this.Hide();
                }
                else if (!usernameExists && !passwordValid)
                {
                    // Jika username dan password salah
                    textBox1.Clear();
                    textBox3.Clear();
                    textBox1.Focus();
                    MessageBox.Show("Username and password are invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (!usernameExists)
                {
                    // Jika username salah
                    textBox1.Clear();
                    textBox3.Clear();
                    textBox1.Focus();
                    MessageBox.Show("Incorrect username.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (!passwordValid)
                {
                    // Jika username benar tetapi password salah
                    textBox3.Clear();
                    textBox3.Focus();
                    MessageBox.Show("Incorrect password. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Tutup koneksi
                conn.Close();
            }
        }
    }
}