namespace Train_D.DTO.UserDtos
{
    public class UserDTO
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string PhoneNumber { get; set; }
        //public byte[]? Image { get; set; }
        public DateTime? BirthDay { get; set; }
    }
}
