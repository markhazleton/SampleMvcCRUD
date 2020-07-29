using System.Collections.Generic;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    public class ErrorResource
    {
        public bool Success => false;

        public List<string> Messages { get; private set; }

        public ErrorResource(List<string> messages) { Messages = messages ?? new List<string>(); }

        public ErrorResource(string message)
        {
            Messages = new List<string>();

            if(!string.IsNullOrWhiteSpace(message))
            {
                Messages.Add(message);
            }
        }
    }
}
