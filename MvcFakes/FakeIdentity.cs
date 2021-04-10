using System;
using System.Security.Claims;

namespace MvcFakes
{
    public class FakeIdentity : ClaimsIdentity
    {
        private readonly string _name;

        public FakeIdentity(string userName,
            string surName = "fake",
            string givenName = "fake",
            string email = "fake@fake.com",
            string phone = "8675309",
            string epid = "999")
        {
            _name = userName;
            AddClaim(new Claim(ClaimTypes.Name, userName));
            AddClaim(new Claim(ClaimTypes.NameIdentifier, userName));
            AddClaim(new Claim(ClaimTypes.Surname, surName));
            AddClaim(new Claim(ClaimTypes.GivenName, givenName));
            AddClaim(new Claim(ClaimTypes.Email, email));
            AddClaim(new Claim(ClaimTypes.MobilePhone, phone));
            AddClaim((new Claim("EPID", epid)));
        }

        public override bool IsAuthenticated
        {
            get { return !String.IsNullOrEmpty(_name); }
        }

        public override string Name
        {
            get { return _name; }
        }
    }
}