using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace Mwh.Sample.Web.Pages.EmployeeRazor
{
    public class CreateModel : PageModel
    {
        private readonly EmployeeContext _context;

        public CreateModel(EmployeeContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "CreatedBy");
            return Page();
        }

        [BindProperty]
        public Employee Employee { get; set; } = default!;


        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || _context.Employees == null || Employee == null)
            {
                return Page();
            }

            _context.Employees.Add(Employee);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
