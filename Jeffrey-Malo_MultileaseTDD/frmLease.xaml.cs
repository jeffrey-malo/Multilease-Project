using System.Windows;

namespace Jeffrey_Malo_MultileaseTDD
{
    public partial class frmLease : Window
    {
        CurrentUser user;
        bool open = false;
        public frmLease(CurrentUser cUser)
        {
            InitializeComponent();
            open = true;
            user = cUser;
            txtUsername.Text = user.Username;
        }

        private void newLeaseBtn_Click(object sender, RoutedEventArgs e)
        {
            // Creating a new lease
            frmNewLease addLease = new frmNewLease();
            addLease.ShowDialog();
        }

        private void modifyLeaseBtn_Click(object sender, RoutedEventArgs e)
        {
            // Modifying an existing lease
            frmModifyLease modifyLease = new frmModifyLease();
            modifyLease.ShowDialog();
        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Ask user if they really want to exit out of the application
            if (open)
            {
                if (MessageBox.Show("Are you sure? Exiting will close the application.", "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
