using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Windows;

namespace Jeffrey_Malo_MultileaseTDD
{
    public partial class frmLogin : Window
    {
        SqlConnection connection;
        SqlCommand cmd;
        bool open = false;
        public frmLogin()
        {
            InitializeComponent();
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connString"].ConnectionString);
            txtUsername.Focus();
            open = true;
        }

        private void cancelLoginBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void frmLogin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Ask user if they really want to exit out of the application
            if (open)
            {
                if (MessageBox.Show("Are you sure you want to cancel login?", "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                connection.Open();

                // test username + password to the database
                // to make correct user
                cmd = new SqlCommand("sp_GetUsername", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Username", SqlDbType.VarChar, 10).Value = txtUsername.Text;
                cmd.Parameters.Add("@Password", SqlDbType.VarChar, 20).Value = txtPassword.Password;

                SqlDataReader reader = cmd.ExecuteReader();

                // Check if username+password exists
                if (reader.Read())
                {
                    CurrentUser user = new CurrentUser();
                    user.UserId = reader["UserId"].ToString();
                    user.Username = reader["Username"].ToString();
                    user.FirstName = reader["FirstName"].ToString();
                    user.LastName = reader["LastName"].ToString();
                    MessageBox.Show("Welcome " + user.FirstName + " " + user.LastName);

                    // Create an instance of the Lease form
                    frmLease leaseFrm = new frmLease(user);
                    leaseFrm.Show();
                    open = false;
                    this.Close();
                } else
                {
                    // user was not found in the db
                    MessageBox.Show("Login failed.", "Login error", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtUsername.Text = string.Empty;
                    txtPassword.Password = string.Empty;
                    txtUsername.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
