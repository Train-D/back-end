namespace Train_D.Models.AuthenticationModels
{
    public class ProfileModel
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public byte[]? Image { get; set; }
        public DateTime? BirthDay { get; set; }
    }
}
