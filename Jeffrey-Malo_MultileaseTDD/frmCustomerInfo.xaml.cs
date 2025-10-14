using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Jeffrey_Malo_MultileaseTDD
{
    
    public partial class frmCustomerInfo : Window
    {
        public CurrentCustomer customerInstance { get; set; }
        bool textInfo = false;
        bool provinceInfo = false;
        bool updated = false;
        string[] provinces = { "AB", "BC", "MB", "NB", "NL", "NS", "NT", "NU", "ON", "PE", "QC", "SK", "YT" };
        public frmCustomerInfo()
        {
            InitializeComponent();
            FillProvinceList();
        }

        public frmCustomerInfo (CurrentCustomer customer)
        {
            InitializeComponent();
            customerInstance = customer;
            this.DataContext = customerInstance;
            FillProvinceList();
            FillCustomerInfo();
            listProvince.SelectedIndex = 0;
        }

        private void FillProvinceList()
        {
            // Populate provinces list
            for (int p = 0; p < provinces.Length; p++)
            {
                listProvince.Items.Add(provinces[p].ToUpper());
            }
        }

        private void FillCustomerInfo()
        {
            // if a customer has already been added, populate the customer's information
            if (customerInstance != null)
            {
                txtFirstName.Text = customerInstance.FirstName;
                txtLastName.Text = customerInstance.LastName;
                txtAddress.Text = customerInstance.Address;
                txtCity.Text = customerInstance.City;
                listProvince.SelectedValue = customerInstance.Province;
                txtPostalCode.Text = customerInstance.PostalCode;
                txtPhone.Text = customerInstance.PhoneNumber;
            } else
            {
                return;
            }
        }

        private void saveCustBtn_Click(object sender, RoutedEventArgs e)
        {
            if (textInfo)
            {
                customerInstance.FirstName = txtFirstName.Text;
                customerInstance.LastName = txtLastName.Text;
                customerInstance.Address = txtAddress.Text;
                customerInstance.City = txtCity.Text;
                customerInstance.Province = listProvince.SelectedValue.ToString();
                customerInstance.PostalCode = txtPostalCode.Text;
                customerInstance.PhoneNumber = txtPhone.Text;

                MessageBox.Show("Customer information has been added.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                updated = false;
                this.Close();
            }
        }

        private void Validate_Info(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtFirstName.Text.Trim()) &&
                (!string.IsNullOrEmpty(txtLastName.Text.Trim())) &&
                (!string.IsNullOrEmpty(txtAddress.Text.Trim())) &&
                (!string.IsNullOrEmpty(txtCity.Text.Trim())) &&
                (!string.IsNullOrEmpty(txtPostalCode.Text.Trim())) &&
                (!string.IsNullOrEmpty(txtPhone.Text.Trim())))
            {
                if (provinceInfo)
                {
                    if (Regex.IsMatch(txtPostalCode.Text, @"^\s*([A-Za-z])(\d)([A-Za-z])[ ]?(\d)([A-Za-z])(\d)\s*$") && Regex.IsMatch(txtPhone.Text, @"^\s*\(?(\d{3})\)?[ -]?(\d{3})[ -]?(\d{4})\s*$"))
                    {
                        txtPostalCode.Text = Regex.Replace(txtPostalCode.Text.ToUpper(), @"^\s*([A-Za-z])(\d)([A-Za-z])[ ]?(\d)([A-Za-z])(\d)\s*$", "$1$2$3 $4$5$6").Trim();
                        txtPhone.Text = Regex.Replace(txtPhone.Text, @"^\s*\(?(\d{3})\)?[ -]?(\d{3})[ -]?(\d{4})\s*$", "($1) $2-$3").Trim();
                        textInfo = true;
                        saveCustBtn.IsEnabled = true;

                        if (txtFirstName.Text == customerInstance.FirstName &&
                    txtLastName.Text == customerInstance.LastName &&
                    txtAddress.Text == customerInstance.Address &&
                    txtCity.Text == customerInstance.City &&
                    listProvince.SelectedValue.ToString() == customerInstance.Province &&
                    txtPostalCode.Text == customerInstance.PostalCode &&
                    txtPhone.Text == customerInstance.PhoneNumber)
                        {
                            updated = false;
                        }
                        else
                        {
                            updated = true;
                        }
                    } else
                    {
                        textInfo = false;
                        saveCustBtn.IsEnabled = false;
                    } 
                }

            } else
            {
                textInfo = false;
                saveCustBtn.IsEnabled = false;
            }
        }

        private void frmCustomerInfo_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (updated)
            {
                if (MessageBox.Show("New information has not been saved and will be lost. Are you sure?", "Warning!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    this.Close();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void Validate_Selection(object sender, SelectionChangedEventArgs e)
        {
            if (listProvince.SelectedIndex != -1)
            {
                provinceInfo = true;

                if (textInfo)
                {
                    saveCustBtn.IsEnabled = true;
                }
            } else
            {
                provinceInfo = false;
                saveCustBtn.IsEnabled = false;
            }
        }
    }
}
