using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace MvcFakes
{
    public class FakePrincipal : ClaimsPrincipal
    {
        private readonly FakeIdentity _identity;
        private readonly string[] _roles;

        public FakePrincipal(string name, string role)
        {
            _identity = new FakeIdentity(name);
            _roles = new string[] { role };
            AddIdentity(_identity);
        }

        public FakePrincipal(string name, string[] roles)
        {
            _identity = new FakeIdentity(name);
            _roles = roles;
            AddIdentity(_identity);
        }

        public override bool IsInRole(string role)
        {
            if (_roles == null)
                return false;
            return _roles.Contains(role);
        }

        public override IIdentity Identity { get { return _identity; } }
    }
}