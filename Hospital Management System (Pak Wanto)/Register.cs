using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Hospital_Management_System__Pak_Wanto_
{
    public partial class Register : Form
    {
        SqlConnection conn = new SqlConnection("Data Source=LAPTOP-9SGQJSHQ\\SQLEXPRESS;Initial Catalog=DB_Hospital;Integrated Security=True;");
        public Register()
        {
            InitializeComponent();
            textBox3.Text = "Enter Password";
            textBox3.ForeColor = Color.Silver;
            textBox3.UseSystemPasswordChar = false;

            textBox2.Text = "Confirm Password";
            textBox2.ForeColor = Color.Silver;
            textBox2.UseSystemPasswordChar = false;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Hide();
        }

        private void Register_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "Enter Username")
            {
                textBox1.Text = "";
                textBox1.ForeColor = Color.Black;
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = "Enter Username";
                textBox1.ForeColor = Color.Silver;
            }
        }

        private void textBox4_Enter(object sender, EventArgs e)
        {
            if (textBox4.Text == "Enter Email")
            {
                textBox4.Text = "";
                textBox4.ForeColor = Color.Black;
            }
        }

        private void textBox4_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox4.Text))
            {
                textBox4.Text = "Enter Email";
                textBox4.ForeColor = Color.Silver;
            }
        }

        private void textBox3_Enter(object sender, EventArgs e)
        {
            if (textBox3.Text == "Enter Password")
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
                textBox3.Text = "Enter Password";
                textBox3.ForeColor = Color.Silver;
                textBox3.UseSystemPasswordChar = false;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (textBox3.Text != "Enter Password" && !string.IsNullOrWhiteSpace(textBox3.Text))
            {
                textBox3.ForeColor = Color.Black;
                textBox3.UseSystemPasswordChar = !checkBox1.Checked;
            }
            else if (textBox3.Text == "Enter Password")
            {
                textBox3.ForeColor = Color.Silver;
                textBox3.UseSystemPasswordChar = false;
            }
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (textBox2.Text == "Confirm Password")
            {
                textBox2.Text = "";
                textBox2.ForeColor = Color.Black;
                textBox2.UseSystemPasswordChar = !checkBox1.Checked;
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                textBox2.Text = "Confirm Password";
                textBox2.ForeColor = Color.Silver;
                textBox2.UseSystemPasswordChar = false;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text != "Confirm Password" && !string.IsNullOrWhiteSpace(textBox2.Text))
            {
                textBox2.ForeColor = Color.Black;
                textBox2.UseSystemPasswordChar = !checkBox1.Checked;
            }
            else if (textBox2.Text == "Confirm Password")
            {
                textBox2.ForeColor = Color.Silver;
                textBox2.UseSystemPasswordChar = false;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (textBox3.Text != "Enter Password" && !string.IsNullOrWhiteSpace(textBox3.Text))
            {
                textBox3.UseSystemPasswordChar = !checkBox1.Checked;
            }
            
            if (textBox2.Text != "Confirm Password" && !string.IsNullOrWhiteSpace(textBox2.Text))
            {
                textBox2.UseSystemPasswordChar = !checkBox1.Checked;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Input validation
            if (textBox1.Text == "Enter Username" || string.IsNullOrWhiteSpace(textBox1.Text) ||
                textBox4.Text == "Enter Email" || string.IsNullOrWhiteSpace(textBox4.Text) ||
                textBox3.Text == "Enter Password" || string.IsNullOrWhiteSpace(textBox3.Text) ||
                textBox2.Text == "Confirm Password" || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("All fields are required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ClearTextBoxes(); // Clear all TextBoxes after error
                return;
            }

            if (!textBox4.Text.Contains("@"))
            {
                MessageBox.Show("Please enter a valid email address containing '@'.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ClearTextBoxes(); // Clear all TextBoxes after error
                return;
            }

            if (textBox3.Text != textBox2.Text)
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ClearTextBoxes(); // Clear all TextBoxes after error
                return;
            }

            try
            {
                // Open the database connection
                conn.Open();

                // Check if the username or email already exists
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username OR Email = @Email";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@Username", textBox1.Text.Trim());
                checkCmd.Parameters.AddWithValue("@Email", textBox4.Text.Trim());

                int exists = (int)checkCmd.ExecuteScalar();

                if (exists > 0)
                {
                    MessageBox.Show("The username or email is already in use. Please use a different one.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ClearTextBoxes(); // Clear all TextBoxes after error
                    return;
                }

                // Insert query
                string query = "INSERT INTO Users (Username, Email, Password) VALUES (@Username, @Email, @Password)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", textBox1.Text.Trim());
                cmd.Parameters.AddWithValue("@Email", textBox4.Text.Trim());
                cmd.Parameters.AddWithValue("@Password", textBox3.Text.Trim());

                int result = cmd.ExecuteNonQuery();

                if (result > 0)
                {
                    MessageBox.Show("Registration successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearTextBoxes(); // Clear all TextBoxes after success
                    Login login = new Login();
                    login.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Registration failed. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ClearTextBoxes(); // Clear all TextBoxes after failure
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ClearTextBoxes(); // Clear all TextBoxes after error
            }
            finally
            {
                // Ensure the connection is closed
                conn.Close();
            }
        }


        private void ClearEmailTextBox()
        {
            textBox4.Text = "Enter Email";
            textBox4.ForeColor = Color.Silver;
        }

        private void ClearTextBoxes()
        {
            textBox1.Text = "Enter Username";
            textBox1.ForeColor = Color.Silver;

            textBox4.Text = "Enter Email";
            textBox4.ForeColor = Color.Silver;

            textBox3.Text = "Enter Password";
            textBox3.ForeColor = Color.Silver;
            textBox3.UseSystemPasswordChar = false;

            textBox2.Text = "Confirm Password";
            textBox2.ForeColor = Color.Silver;
            textBox2.UseSystemPasswordChar = false;
        }
    }
}
