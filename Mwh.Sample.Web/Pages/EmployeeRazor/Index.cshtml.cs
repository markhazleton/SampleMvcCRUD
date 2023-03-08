using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mwh.Sample.Repository.Models;

namespace Mwh.Sample.Web.Pages.EmployeeRazor
{
    public class IndexModel : PageModel
    {
        private readonly Mwh.Sample.Repository.Models.EmployeeContext _context;

        public IndexModel(Mwh.Sample.Repository.Models.EmployeeContext context)
        {
            _context = context;
        }

        public IList<Employee> Employee { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Employees != null)
            {
                Employee = await _context.Employees
                .Include(e => e.Department).ToListAsync();
            }
        }
    }
}
