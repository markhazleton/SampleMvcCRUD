using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Web.SessionState;

namespace MvcFakes
{
    public class FakeHttpSessionState : HttpSessionStateBase
    {
        private readonly SessionStateItemCollection _sessionItems;

        public FakeHttpSessionState(SessionStateItemCollection sessionItems)
        {
            _sessionItems = sessionItems;
        }

        public override object this[string name]
        {
            get { return _sessionItems[name]; }
            set { _sessionItems[name] = value; }
        }

        public override object this[int index]
        {
            get { return _sessionItems[index]; }
            set { _sessionItems[index] = value; }
        }

        public override void Abandon()
        {
            _sessionItems.Clear();
        }

        public override void Add(string name, object value)
        {
            _sessionItems[name] = value;
        }

        public override void Clear()
        {
            _sessionItems.Clear();
        }

        public override IEnumerator GetEnumerator()
        {
            return _sessionItems.GetEnumerator();
        }

        public override void Remove(string name)
        {
            _sessionItems.Remove(name);
        }

        public override int Count { get { return _sessionItems.Count; } }

        public override NameObjectCollectionBase.KeysCollection Keys { get { return _sessionItems.Keys; } }
    }
}