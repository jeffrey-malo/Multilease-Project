using System.ComponentModel;

namespace Jeffrey_Malo_MultileaseTDD
{
    public class Terms
    {
        public string TermsID { get; set; }

        public string TermsIDProperty
        {
            get => TermsID;
            set
            {
                if (TermsID != value)
                {
                    TermsID = value;
                    OnPropertyChanged(nameof(TermsIDProperty));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
