using Microsoft.AspNetCore.Identity;

namespace Mezhintekhkom.Site.Data.Entities
{
    public class User : IdentityUser
    {
        public Passport Passport { get; set; }
    }
}
