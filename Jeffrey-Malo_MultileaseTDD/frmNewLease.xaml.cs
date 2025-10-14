using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using System.Data;
using System.Windows.Controls;

namespace Jeffrey_Malo_MultileaseTDD
{
    public partial class frmNewLease : Window
    {
        SqlConnection connection;
        SqlCommand cmd;
        bool updatedCust = false;
        int[] numPayments = { 12, 24, 36, 48 };
        bool textInfo = false;
        bool dateInfo = false;
        bool listInfo = false;
        bool termsInfo = false;
        bool updatedTerms = false;

        public CurrentCustomer customerInstance { get; set; }
        public Terms termsInstance { get; set; }
        public frmNewLease()
        {
            InitializeComponent();
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connString"].ConnectionString);
            customerInstance = new CurrentCustomer();
            termsInstance = new Terms();
            this.DataContext = customerInstance;
            this.DataContext = termsInstance;
            FillLists();
        }

        private void FillLists()
        {
            // fill the number of payments list (12, 24, 36 or 48 monthly payments)
            for (int i = 0; i < numPayments.Length; i++)
            {
                numberOfPaymentsList.Items.Add(numPayments[i]);
            }

            try
            {
                // Fill the VINs combobox with vehicle vins in Vehicles db table
                connection.Open();

                cmd = new SqlCommand("sp_FindVINS", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    vinNumberList.Items.Add(reader["VIN"].ToString());
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

        private void Reset()
        {
            // Reset the fields for a new search
            txtCustomerName.Text = "";
            txtLeaseTermsID.Text = "";
            txtMonthlyAmount.Text = "";
            beginDate.SelectedDate = DateTime.Today;
            firstPaymentDate.SelectedDate = DateTime.Today.AddDays(1);
            vinNumberList.SelectedIndex = -1;
            numberOfPaymentsList.SelectedIndex = -1;
            customerInstance = new CurrentCustomer();
            termsInstance = new Terms();

        }
        private void customerInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            // Open the add/edit customer form
            updatedCust = true;
            frmCustomerInfo customerInfo = new frmCustomerInfo(customerInstance);
            customerInfo.ShowDialog();
        }

        private void leaseTermsBtn_Click(object sender, RoutedEventArgs e)
        {
            // Open the add/edit lease terms form
            updatedTerms = true;
            frmLeaseTerms leaseTerms = new frmLeaseTerms(termsInfo, termsInstance);
            leaseTerms.ShowDialog();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void frmNewLease_Activated(object sender, EventArgs e)
        {
            // Update the Customer Name field once coming back from the customer info form
            // Then change the button text if a customer has been added
            if (updatedCust)
            {
                txtCustomerName.Text = customerInstance.FullName;
                updatedCust = false;
                if (!string.IsNullOrEmpty(txtCustomerName.Text.Trim()))
                {
                    customerInfoBtn.Content = "Edit Customer";
                } else
                {
                    customerInfoBtn.Content = "Add Customer";
                }
            }

            // Add/Edit the Lease Terms for this lease
            if (updatedTerms)
            {
                txtLeaseTermsID.Text = termsInstance.TermsID;
                updatedTerms = false;
                if (!string.IsNullOrEmpty(txtLeaseTermsID.Text.Trim())) {
                    leaseTermsBtn.Content = "Edit Lease Terms";
                } else
                {
                    leaseTermsBtn.Content = "Add Lease Terms";
                }
            }
        }

        private void Validate_Info(object sender, TextChangedEventArgs e)
        {

            // Make sure textBoxes are filled
            if (!string.IsNullOrEmpty(txtCustomerName.Text.Trim()) &&
            !string.IsNullOrEmpty(txtMonthlyAmount.Text.Trim()) &&
            !string.IsNullOrEmpty(txtLeaseTermsID.Text.Trim()))
            {
                textInfo = true;

                if (listInfo && dateInfo)
                {
                    saveBtn.IsEnabled = true;
                }
            }
            else
            {
                saveBtn.IsEnabled = false;
            }

            if (!string.IsNullOrEmpty(txtLeaseTermsID.Text.Trim()))
            {
                termsInfo = true;
            }
            else
            {
                termsInfo = false;
            }
        }

        private void Validate_Selection(object sender, SelectionChangedEventArgs e)
        {
            // Make sure comboBoxes lists have a value selected
            if (vinNumberList.SelectedIndex != -1 && numberOfPaymentsList.SelectedIndex != -1)
            {
                listInfo = true;

                if (textInfo && dateInfo)
                {
                    saveBtn.IsEnabled = true;
                }
            }
            else
            {
                saveBtn.IsEnabled = false;
            }
        }

        private void Validate_DateSelection(object sender, SelectionChangedEventArgs e)
        {
            // Make sure Dates are selected
            DateTime date;

            if (DateTime.TryParse(beginDate.SelectedDate.ToString(), out date) && DateTime.TryParse(firstPaymentDate.SelectedDate.ToString(), out date))
            {
                if (firstPaymentDate.SelectedDate >= beginDate.SelectedDate)
                {
                    dateInfo = true;

                    if (textInfo && listInfo)
                    {
                        saveBtn.IsEnabled = true;
                    }
                } else
                {
                    dateInfo = false;
                    saveBtn.IsEnabled = false;
                    MessageBox.Show("First Payment Date must be made on or after the lease Begin Date.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                dateInfo = false;
                saveBtn.IsEnabled = false;
            }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            // Everything is filled out, insert lease information to Lease table
            // insert Customer information to the Customers table + last leaseId that we just added to the Lease table 
            if (textInfo && listInfo && dateInfo && customerInfoBtn.Content.ToString() == "Edit Customer" && leaseTermsBtn.Content.ToString() == "Edit Lease Terms")
            {
                try
                {
                    // Add Lease information to Lease table
                    connection.Open();

                    cmd = new SqlCommand("sp_AddLease", connection);
                    cmd.CommandType = CommandType.StoredProcedure;


                    cmd.Parameters.Add("@BegDate", SqlDbType.DateTime).Value = beginDate.SelectedDate;
                    cmd.Parameters.Add("@FirstPaymentDt", SqlDbType.DateTime).Value = firstPaymentDate.SelectedDate;
                    cmd.Parameters.Add("@MonthPayment", SqlDbType.Int).Value = (int)Decimal.Parse(txtMonthlyAmount.Text);
                    cmd.Parameters.Add("@NumOfPayments", SqlDbType.Int).Value = numberOfPaymentsList.SelectedValue;
                    cmd.Parameters.Add("@VIN", SqlDbType.VarChar, 23).Value = vinNumberList.SelectedValue;
                    cmd.Parameters.Add("@LeaseTermsId", SqlDbType.VarChar, 11).Value = txtLeaseTermsID.Text;

                    cmd.ExecuteNonQuery();

                    // Add Customer Information to Customers Table
                    cmd = new SqlCommand("sp_AddCustomer", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@FName", SqlDbType.VarChar, 30).Value = customerInstance.FirstName;
                    cmd.Parameters.Add("@LName", SqlDbType.VarChar, 30).Value = customerInstance.LastName;
                    cmd.Parameters.Add("@Addr", SqlDbType.VarChar, 50).Value = customerInstance.Address;
                    cmd.Parameters.Add("@City", SqlDbType.VarChar, 30).Value = customerInstance.City;
                    cmd.Parameters.Add("@Prov", SqlDbType.Char, 2).Value = customerInstance.Province;
                    cmd.Parameters.Add("@PostalCode", SqlDbType.Char, 7).Value = customerInstance.PostalCode;
                    cmd.Parameters.Add("@Phone", SqlDbType.Char, 14).Value = customerInstance.PhoneNumber;

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("The Lease has been successfully added for this customer.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Reset();

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
}
