using System.ComponentModel;

namespace Jeffrey_Malo_MultileaseTDD
{
    public class CurrentCustomer
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }

        public string FirstNameProperty
        {
            get => FirstName;
            set
            {
                if (FirstName != value)
                {
                    FirstName = value;
                    OnPropertyChanged(nameof(FirstNameProperty));
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        public string LastNameProperty
        {
            get => LastName;
            set
            {
                if (LastName != value)
                {
                    LastName = value;
                    OnPropertyChanged(nameof(LastNameProperty));
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        public string FullName => $"{FirstName} {LastName}";

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
