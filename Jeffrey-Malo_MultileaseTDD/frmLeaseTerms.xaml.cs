using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using System.Data;
using System.Windows.Controls;

namespace Jeffrey_Malo_MultileaseTDD
{
    public partial class frmLeaseTerms : Window
    {
        CurrentLease lease;
        SqlConnection connection;
        SqlCommand cmd;
        int[] leaseYears = {1, 2, 3, 4 };
        bool textInfo = false;
        bool yearsInfo = false;
        bool termsInfo;
        bool newLease = false;
        bool updated = false;

        public Terms termsInstance { get; set; }

        public frmLeaseTerms()
        {
            InitializeComponent();
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connString"].ConnectionString);
        }

        public frmLeaseTerms (bool tInfo, Terms termsInst)
        {
            InitializeComponent();
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connString"].ConnectionString);
            termsInfo = tInfo;
            termsInstance = termsInst;
            this.DataContext = termsInstance;
            newLease = true;
            FillLeaseTerms();
        }
        public frmLeaseTerms(CurrentLease curLease)
        {
            InitializeComponent();
            lease = curLease;
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connString"].ConnectionString);
            FillLeaseTerms();
            newLease = false;
        }

        private void FillLeaseTerms()
        {
            // Populate Years list with leaseYears int array
            for (int y = 0; y < leaseYears.Length; y++)
            {
                yearsList.Items.Add(leaseYears[y]);
            }

            if (!newLease)
            {
                try
                {
                    connection.Open();

                    cmd = new SqlCommand("sp_FindLeaseTerms", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@TermsID", SqlDbType.VarChar, 11).Value = lease.LeaseTermsID;

                    SqlParameter years = new SqlParameter("@Years", SqlDbType.Int);
                    SqlParameter maxKm = new SqlParameter("@MaxKM", SqlDbType.VarChar, 15);
                    SqlParameter premium = new SqlParameter("@Premium", SqlDbType.VarChar, 10);

                    years.Direction = ParameterDirection.Output;
                    maxKm.Direction = ParameterDirection.Output;
                    premium.Direction = ParameterDirection.Output;

                    cmd.Parameters.Add(years);
                    cmd.Parameters.Add(maxKm);
                    cmd.Parameters.Add(premium);

                    cmd.ExecuteNonQuery();

                    int yrs = (int)years.Value;
                    string maxK = maxKm.Value.ToString();
                    string prem = premium.Value.ToString();

                    if (yrs > 0 && maxK != string.Empty && prem != string.Empty)
                    {
                        yearsList.SelectedValue = yrs;
                        txtMaxKm.Text = maxK;
                        txtExtraKm.Text = prem;
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
            } else
            {
                if (termsInfo)
                {
                    try
                    {
                        connection.Open();

                        cmd = new SqlCommand("sp_GetLeaseTerms", connection);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@TermsID", SqlDbType.VarChar, 11).Value = termsInstance.TermsID;

                        SqlParameter yrs = new SqlParameter("@Years", SqlDbType.Int);
                        SqlParameter maxKM = new SqlParameter("@MaxKM", SqlDbType.VarChar, 15);
                        SqlParameter prem = new SqlParameter("@Premium", SqlDbType.VarChar, 10);

                        yrs.Direction = ParameterDirection.Output;
                        maxKM.Direction = ParameterDirection.Output;
                        prem.Direction = ParameterDirection.Output;

                        cmd.Parameters.Add(yrs);
                        cmd.Parameters.Add(maxKM);
                        cmd.Parameters.Add(prem);

                        cmd.ExecuteNonQuery();

                        int years = (int)yrs.Value;
                        string mKM = maxKM.Value.ToString();
                        string premium = prem.Value.ToString();

                        if (years > 0 && mKM != string.Empty && premium != string.Empty)
                        {
                            yearsList.SelectedValue = years;
                            txtMaxKm.Text = mKM;
                            txtExtraKm.Text = premium;
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
        private void cancelTermsBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Validate_Info(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtMaxKm.Text.Trim()) &&
                !string.IsNullOrEmpty(txtExtraKm.Text.Trim()))
            {
                textInfo = true;

                if (yearsInfo)
                {
                    saveTermsBtn.IsEnabled = true;
                }
            } else
            {
                textInfo = false;
                saveTermsBtn.IsEnabled = false;
            }
        }

        private void Validate_Selection(object sender, SelectionChangedEventArgs e)
        {
            if (yearsList.SelectedIndex != -1)
            {
                yearsInfo = true;

                if (textInfo)
                {
                    saveTermsBtn.IsEnabled = true;
                }
            } else
            {
                yearsInfo = false;
                saveTermsBtn.IsEnabled = false;
            }
        }

        private void saveTermsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (textInfo && yearsInfo)
            {
                if (!newLease)
                {
                    try
                    {
                        connection.Open();

                        cmd = new SqlCommand("sp_NewLeaseTerms", connection);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@Years", SqlDbType.Int).Value = yearsList.SelectedValue;
                        cmd.Parameters.Add("@MaxKm", SqlDbType.VarChar, 15).Value = txtMaxKm.Text;
                        cmd.Parameters.Add("@Premium", SqlDbType.VarChar, 10).Value = txtExtraKm.Text;

                        cmd.ExecuteNonQuery();

                        cmd = new SqlCommand("sp_GetLastTermsIDUpdateLeaseId", connection);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@LeaseId", SqlDbType.VarChar, 13).Value = lease.LeaseID;

                        SqlParameter newId = new SqlParameter("@NewTermsID", SqlDbType.VarChar, 11);
                        newId.Direction = ParameterDirection.Output;

                        cmd.Parameters.Add(newId);
                        cmd.ExecuteScalar();

                        string newTermsId = newId.Value.ToString();

                        if (newTermsId != string.Empty)
                        {
                            lease.LeaseTermsID = newTermsId;
                        }

                        MessageBox.Show("The lease terms for this lease have been updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        cancelTermsBtn.Content = "BACK";
                        updated = false;

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                } else
                {
                    try
                    {
                        connection.Open();

                        // Insert lease terms into LeaseTerms table
                        cmd = new SqlCommand("sp_NewLeaseTerms", connection);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@Years", SqlDbType.Int).Value = yearsList.SelectedValue;
                        cmd.Parameters.Add("@MaxKm", SqlDbType.VarChar, 15).Value = txtMaxKm.Text;
                        cmd.Parameters.Add("@Premium", SqlDbType.VarChar, 10).Value = txtExtraKm.Text;

                        cmd.ExecuteNonQuery();

                        // Retrieve last TermsID in LeaseTerms that was just added
                        // and set in the Terms class for use by NewLease form
                        cmd = new SqlCommand("sp_GetLastTermsID", connection);
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter termsId = new SqlParameter("@NewTermsID", SqlDbType.VarChar, 11);
                        termsId.Direction = ParameterDirection.Output;

                        cmd.Parameters.Add(termsId);
                        cmd.ExecuteNonQuery();

                        string tId = termsId.Value.ToString();

                        if (tId != string.Empty)
                        {
                            termsInstance.TermsID = tId;
                        }

                        MessageBox.Show("The lease terms for this lease have been updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        cancelTermsBtn.Content = "BACK";

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
}
