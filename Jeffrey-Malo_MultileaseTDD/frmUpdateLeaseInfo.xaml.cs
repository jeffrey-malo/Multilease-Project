using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Jeffrey_Malo_MultileaseTDD
{
    public partial class frmUpdateLeaseInfo : Window
    {
        SqlConnection connection;
        SqlCommand cmd;
        CurrentLease lease;
        bool textInfo;
        bool dateInfo;
        bool numPaymentsInfo;
        int[] numPayments = { 12, 24, 36, 48 };
        public frmUpdateLeaseInfo(CurrentLease curLease)
        {
            InitializeComponent();
            lease = curLease;
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connString"].ConnectionString);
            FillLeaseFrm();
        }

        private void FillLeaseFrm()
        {
            // Fill the NumberOfPayments list (12, 24, 36 or 48 monthly payments)
            if (numberOfPaymentsList.Items.IsEmpty)
            {
                // fill the number of payments list with numPayments int[]
                for (int i = 0; i < numPayments.Length; i++)
                {
                    numberOfPaymentsList.Items.Add(numPayments[i]);
                }
            }

            // Fill already found info to the matching fields
            txtCustomerName.Text = lease.FullName;
            txtLeaseID.Text = lease.LeaseID;
            txtVinNumber.Text = lease.VIN;

            // Retrieve from db the lease information for the currently selected customer by LeaseID
            try
            {
                connection.Open();

                cmd = new SqlCommand("sp_FindLease", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@LeaseID", SqlDbType.VarChar, 13).Value = txtLeaseID.Text;

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    beginDatePicker.SelectedDate = (DateTime?)reader["BeginDate"];
                    firstPaymentDatePicker.SelectedDate = (DateTime?)reader["FirstPaymentDate"];

                    decimal amount = Decimal.Parse(reader["MonthlyPayment"].ToString());
                    txtMonthlyAmount.Text = (Math.Round(amount, 2)).ToString();

                    txtLeaseTermsID.Text = reader["LeaseTermsID"].ToString();
                    numberOfPaymentsList.SelectedValue = reader["NumberOfPayments"];
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


        private void leaseTermsBtn_Click(object sender, RoutedEventArgs e)
        {
            // Open LeaseTerms form with current Lease info
            frmLeaseTerms updateLeaseTerms = new frmLeaseTerms(lease);
            updateLeaseTerms.ShowDialog();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Validate_Info(object sender, TextChangedEventArgs e)
        {
            // Make sure text fields are filled
            if (!string.IsNullOrEmpty(txtMonthlyAmount.Text.Trim()) &&
                !string.IsNullOrEmpty(txtVinNumber.Text.Trim()) &&
                !string.IsNullOrEmpty(txtCustomerName.Text.Trim()) &&
                !string.IsNullOrEmpty(txtLeaseID.Text.Trim()) &&
                !string.IsNullOrWhiteSpace(txtLeaseTermsID.Text.Trim()))
            {
                textInfo = true;

                if (dateInfo)
                {
                    saveBtn.IsEnabled = true;
                }
            }
            else
            {
                textInfo = false;
                saveBtn.IsEnabled = false;
            }
        }

        private void Validate_Date(object sender, SelectionChangedEventArgs e)
        {
            // Make sure dates have been selected
            DateTime date;

            if (DateTime.TryParse(beginDatePicker.SelectedDate.ToString(), out date) && DateTime.TryParse(firstPaymentDatePicker.SelectedDate.ToString(), out date))
            {
                dateInfo = true;

                if (textInfo && numPaymentsInfo)
                {
                    saveBtn.IsEnabled = true;
                }
            }
            else
            {
                dateInfo = false;
                saveBtn.IsEnabled = false;
            }
        }

        private void Validate_Selection(object sender, SelectionChangedEventArgs e)
        {
            // Make sure a selection has been made in the Number of payments list
            if (numberOfPaymentsList.SelectedIndex != -1)
            {
                numPaymentsInfo = true;

                if (textInfo && dateInfo)
                {
                    saveBtn.IsEnabled = true;
                }
            } else
            {
                numPaymentsInfo = false;
                saveBtn.IsEnabled = false;
            }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            // Update Lease information in Lease db table
            if (textInfo && dateInfo && numPaymentsInfo)
            {
                try
                {
                    connection.Open();

                    cmd = new SqlCommand("sp_ModifyLease", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@VINCode", SqlDbType.VarChar, 23).Value = txtVinNumber.Text;
                    cmd.Parameters.Add("@BDate", SqlDbType.DateTime).Value = beginDatePicker.SelectedDate;
                    cmd.Parameters.Add("@FirstPayment", SqlDbType.DateTime).Value = firstPaymentDatePicker.SelectedDate;
                    cmd.Parameters.Add("@MPayment", SqlDbType.Int).Value = txtMonthlyAmount.Text;
                    cmd.Parameters.Add("@NumOfPayments", SqlDbType.Int).Value = numberOfPaymentsList.SelectedValue;
                    cmd.Parameters.Add("@LeaseID", SqlDbType.VarChar, 13).Value = txtLeaseID.Text;

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("The lease information has been successfully saved.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void frmUpdateLeaseInfo_Activated(object sender, EventArgs e)
        {
            // Set leasetermsID in case it was changed
            // after updating the lease terms
            txtLeaseTermsID.Text = lease.LeaseTermsID;
        }
    }
}
