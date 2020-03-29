using Mwh.Sample.Common.Models;
using System;
using System.Collections.Generic;

namespace Mwh.Sample.Common.DataInterface
{
    public interface IDeskRepository
    {
        IEnumerable<Desk> GetAvailableDesks(DateTime date);
    }
}
