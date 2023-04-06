using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;

namespace Mwh.Sample.Web.Pages.EmployeeRazor
{
    public class IndexModel : PageModel
    {
        private readonly EmployeeContext _context;

        public IndexModel(EmployeeContext context)
        {
            _context = context;
        }

        public IList<Employee> Employee { get; set; } = default!;

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
