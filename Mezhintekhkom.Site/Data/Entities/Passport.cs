namespace Mezhintekhkom.Site.Data.Entities
{
    public class Passport
    {
        public Guid Id { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? Patronymic { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? Avatar { get; set; }
    }
}
