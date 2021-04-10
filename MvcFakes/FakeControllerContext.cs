using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;

namespace MvcFakes
{
    public class FakeControllerContext : ControllerContext
    {
        public FakeControllerContext(ControllerBase controller) : this(controller, null, null, null, null, null, null)
        {
        }

        public FakeControllerContext(ControllerBase controller, HttpCookieCollection cookies) : this(
            controller,
            null,
            null,
            null,
            null,
            cookies,
            null)
        {
        }

        public FakeControllerContext(ControllerBase controller, SessionStateItemCollection sessionItems) : this(
            controller,
            null,
            null,
            null,
            null,
            null,
            sessionItems)
        {
        }

        public FakeControllerContext(ControllerBase controller, NameValueCollection formParams) : this(
            controller,
            null,
            null,
            formParams,
            null,
            null,
            null)
        {
        }

        public FakeControllerContext(ControllerBase controller, string userName) : this(
            controller,
            userName,
            null,
            null,
            null,
            null,
            null)
        {
        }

        public FakeControllerContext(
            ControllerBase controller,
            NameValueCollection formParams,
            NameValueCollection queryStringParams) : this(
            controller,
            null,
            null,
            formParams,
            queryStringParams,
            null,
            null)
        {
        }

        public FakeControllerContext(ControllerBase controller, string userName, string[] roles) : this(
            controller,
            userName,
            roles,
            null,
            null,
            null,
            null)
        {
        }

        public FakeControllerContext(
            ControllerBase controller,
            string userName,
            string[] roles,
            NameValueCollection formParams,
            NameValueCollection queryStringParams,
            HttpCookieCollection cookies,
            SessionStateItemCollection sessionItems) : base(
            new FakeHttpContext(
                new FakePrincipal(userName, roles),
                formParams,
                queryStringParams,
                cookies,
                sessionItems),
            new RouteData(),
            controller)
        {
        }
    }
}