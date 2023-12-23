using System;
using System.Collections.Generic;
using System.ComponentModel;
using SD = System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace HeshAutorization
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }
        private SqlConnection sqlConnection = new SqlConnection(@"Data Source=DESKTOP-RNGC4SV; Initial Catalog=wslabs; Integrated Security=True");
        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();

        public void openConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Closed)
            {
                sqlConnection.Open();
            }
        }

        public void closeConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Open)
            {
                sqlConnection.Close();
            }
        }
        public SqlConnection getConnection()
        {
           return sqlConnection;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button_Aut_Click(object sender, EventArgs e)
        {
            string Login = textBox1.Text.ToString();
            string Password = textBox2.Text.ToString();

            openConnection();

            if(Proverka(Login, Password) == true)
            {
                MessageBox.Show("Авторизация успешна", "Усех");
            }
            else
            {
                MessageBox.Show("Вы ввели неверный логин или пароль", "Ошибка");
            }
            closeConnection();
        }

        private void button_Reg_Click(object sender, EventArgs e)
        {
            string loginReg = textBox3.Text.Trim();
            string passwordReg1 = textBox4.Text.ToString();
            string passwordReg2 = textBox5.Text.ToString();
            string name = textBox8.Text.ToString();
            string lastName = textBox7.Text.ToString();
            string post = textBox6.Text.ToString();

            if (passwordReg1 == passwordReg2)
            {
                Regex check = new Regex(@"[0-9a-zA-Z!@#$%^&*]{6,}");

                if (check.IsMatch(passwordReg1))
                {
                    openConnection();

                    if (CheckExistingLoginForRegistration(loginReg))
                    {
                        passwordReg1 = CreateMD5(passwordReg1);

                        using (SqlCommand sqlCommand = new SqlCommand("INSERT INTO wsUsers(login, password, Имя, Фамилия, должность) VALUES(@Login, @Password, @Name, @LastName, @Post)", sqlConnection))
                        {
                            sqlCommand.Parameters.AddWithValue("@Login", loginReg);
                            sqlCommand.Parameters.AddWithValue("@Password", passwordReg1);
                            sqlCommand.Parameters.AddWithValue("@Name", name);
                            sqlCommand.Parameters.AddWithValue("@LastName", lastName);
                            sqlCommand.Parameters.AddWithValue("@Post", post);

                            try
                            {
                                int rowsAffected = sqlCommand.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Аккаунт был создан", "Успех");
                                }
                                else
                                {
                                    MessageBox.Show("Аккаунт не создан", "Ошибка");
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка при выполнении операции: {ex.Message}", "Ошибка");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Такой логин уже существует", "Ошибка");
                    }
                }
                else
                {
                    MessageBox.Show("Пароль должен содержать: 0-9a-zA-Z!@#$%^&*", "Ошибка");
                }
            }
            else
            {
                MessageBox.Show("Вы ввели неодинаковые пароли", "Ошибка");
            }

            closeConnection();
        }
        private bool CheckExistingLoginForRegistration(string login)
        {
            DataTable table = new DataTable();
            using (SqlCommand sqlCommand = new SqlCommand("IF NOT EXISTS (SELECT login FROM wsUsers WHERE login=@Login) SELECT 1 ELSE SELECT 0", sqlConnection))
            {
                sqlCommand.Parameters.AddWithValue("@Login", login);
                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
                {
                    sqlDataAdapter.Fill(table);
                    return Convert.ToInt32(table.Rows[0][0]) == 1;
                }
            }
        }

        private bool Proverka(string login, string password)
        {
            DataTable table = new DataTable();
            string Login = login;

            using (SqlCommand sqlCommand = new SqlCommand("SELECT login, password FROM wsUsers WHERE login=@Login", sqlConnection))
            {
                sqlCommand.Parameters.AddWithValue("@Login", Login);

                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
                {
                    sqlDataAdapter.Fill(table);

                    if (table.Rows.Count > 0)
                    {
                        // Retrieve the stored password from the database
                        string storedPassword = table.Rows[0]["password"].ToString();

                        // Compare the provided password with the stored password
                        if (storedPassword.Equals(CreateMD5(password)))
                        {
                            return true; // Authentication successful
                        }
                    }
                }
            }

            return false; // Authentication failed
        }


        public static string CreateMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        private void Registration_Click(object sender, EventArgs e)
        {

        }
    }
}
