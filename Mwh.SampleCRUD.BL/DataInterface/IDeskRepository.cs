using Mwh.SampleCRUD.BL.Models;
using System;
using System.Collections.Generic;

namespace Mwh.SampleCRUD.BL.DataInterface
{
    public interface IDeskRepository
    {
        IEnumerable<Desk> GetAvailableDesks(DateTime date);
    }
}
