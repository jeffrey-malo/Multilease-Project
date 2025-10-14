using System;
using System.Data.SqlClient;
using System.Windows;
using System.Configuration;
using System.Windows.Controls;
using System.Data;

namespace Jeffrey_Malo_MultileaseTDD
{
    public partial class frmModifyLease : Window
    {
        SqlConnection connection;
        SqlCommand cmd;
        bool leaseInfo = false;
        bool leaseFound = false;
        public frmModifyLease()
        {
            InitializeComponent();
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connString"].ConnectionString);
            FillIdList();
        }

        public void FillIdList()
        {
            // Fill LeaseID list from the Lease db table (clear first if the list already contains LeaseIDs)
            try
            {
                connection.Open();

                if (!leaseIdList.Items.IsEmpty)
                {
                    leaseIdList.Items.Clear();
                }

                cmd = new SqlCommand("sp_FindLeaseId", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader != null)
                {
                    while (reader.Read())
                    {
                        leaseIdList.Items.Add(reader["LeaseID"].ToString());
                    }
                }
                else
                {
                    MessageBox.Show("No lease found.");
                    leaseFound = false;
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

        public void Reset()
        {
            // Reset all the fields + fill the LeaseID list
            leaseFound = false;
            leaseIdList.SelectedIndex = -1;
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtVinCode.Text = "";
            txtFirstName.IsReadOnly = false;
            txtLastName.IsReadOnly = false;
            findLeaseBtn.IsEnabled = false;
            modifyLeaseBtn.IsEnabled = false;
            FillIdList();
        }

        private void modifyLeaseBtn_Click(object sender, RoutedEventArgs e)
        {
            // Create new Lease instance and add information from our already selected Lease
            // for use in the update lease form
            CurrentLease lease = new CurrentLease();

            try
            {
                connection.Open();

                cmd = new SqlCommand("sp_FindLeaseInfo", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@LeaseID", SqlDbType.VarChar, 13).Value = leaseIdList.SelectedValue;

                SqlParameter termsId1 = new SqlParameter("@TermsID", SqlDbType.VarChar, 11);
                termsId1.Direction = ParameterDirection.Output;

                cmd.Parameters.Add(termsId1);
                cmd.ExecuteScalar();

                string tID = termsId1.Value.ToString();

                if (tID != string.Empty)
                {
                    lease.LeaseID = leaseIdList.SelectedValue.ToString();
                    lease.FirstName = txtFirstName.Text;
                    lease.LastName = txtLastName.Text;
                    lease.VIN = txtVinCode.Text;
                    lease.LeaseTermsID = tID;
                }


            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }

            // Open Update lease info form
            frmUpdateLeaseInfo updateLeaseInfo = new frmUpdateLeaseInfo(lease);
            updateLeaseInfo.ShowDialog();
        }

        private void cancelModBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void findLeaseBtn_Click(object sender, RoutedEventArgs e)
        {
            // Searching by customer first & last name, find corresponding information if any
            if (findLeaseBtn.Content.ToString() == "FIND" && leaseIdList.SelectedIndex == -1)
            {
                
                // Make sure First and Last name are valid and attributed to a leaseID
                // When searching for a lease by name and no lease is indicated in the leaseID list
                try
                {
                    connection.Open();
                    cmd = new SqlCommand("sp_FindLeaseByName", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@FirstN", SqlDbType.VarChar, 30).Value = txtFirstName.Text;
                    cmd.Parameters.Add("@LastN", SqlDbType.VarChar, 30).Value = txtLastName.Text;

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        
                        leaseIdList.Items.Clear();

                        while (reader.Read())
                        {
                            leaseIdList.Items.Add(reader["LeaseID"].ToString());
                        }
                        leaseIdList.SelectedIndex = 0;
                    } else
                    {
                        connection.Close();
                        MessageBox.Show("No lease found for this customer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Reset();
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
            else if (findLeaseBtn.Content.ToString() == "RESET")
            {
                findLeaseBtn.Content = "FIND";
                Reset();
            }


        }

        private void Validate_Info(object sender, TextChangedEventArgs e)
        {
            // Check to make sure the needed textboxes are filled to make the search
            if (!string.IsNullOrEmpty(txtFirstName.Text.Trim()) &&
                !string.IsNullOrEmpty(txtLastName.Text.Trim()))

            {
                leaseInfo = true;
                findLeaseBtn.IsEnabled = true;
            }
            else
            {
                leaseInfo = false;
            }
        }

        private void leaseIdList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // make sure LeaseID has been selected from the list/found by matching customer
            if (leaseIdList.SelectedIndex == -1)
            {
                return;
            }

            leaseFound = true;
            txtFirstName.IsReadOnly = true;
            txtLastName.IsReadOnly = true;
            txtVinCode.IsReadOnly = true;
            findLeaseBtn.Content = "RESET";
            modifyLeaseBtn.IsEnabled = true;
            

            // retrieve customer + lease info from stored procedure
            try
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }

                connection.Open();

                cmd = new SqlCommand("sp_FindCustomer", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@LeaseID", SqlDbType.VarChar, 23).Value = leaseIdList.SelectedValue;
                SqlParameter fName = new SqlParameter("@FirstN", SqlDbType.VarChar, 30);
                SqlParameter lName = new SqlParameter("@LastN", SqlDbType.VarChar, 30);
                SqlParameter vinCode = new SqlParameter("@VINCode", SqlDbType.VarChar, 23);

                fName.Direction = ParameterDirection.Output;
                lName.Direction = ParameterDirection.Output;
                vinCode.Direction = ParameterDirection.Output;

                cmd.Parameters.Add(fName);
                cmd.Parameters.Add(lName);
                cmd.Parameters.Add(vinCode);
                cmd.ExecuteNonQuery();

                string FirstName = fName.Value.ToString();
                string LastName = lName.Value.ToString();
                string vin = vinCode.Value.ToString();

                if (FirstName != string.Empty && LastName != string.Empty && vin != string.Empty)
                {
                    txtFirstName.Text = FirstName;
                    txtLastName.Text = LastName;
                    txtVinCode.Text = vin;
                } else
                {
                    MessageBox.Show("LeaseID has no customer attached.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Reset();
                    txtFirstName.Focus();
                }
            }
            catch(Exception ex)
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
