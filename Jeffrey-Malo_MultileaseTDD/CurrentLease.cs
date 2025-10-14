
namespace Jeffrey_Malo_MultileaseTDD
{
    public class CurrentLease
    {
        public string LeaseID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string VIN { get; set; }
        public string LeaseTermsID { get; set; }
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
    }
}
