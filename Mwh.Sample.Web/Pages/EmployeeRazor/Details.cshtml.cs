﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;

namespace Mwh.Sample.Web.Pages.EmployeeRazor
{
    public class DetailsModel : PageModel
    {
        private readonly EmployeeContext _context;

        public DetailsModel(EmployeeContext context)
        {
            _context = context;
        }

        public Employee Employee { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Employees == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }
            else
            {
                Employee = employee;
            }
            return Page();
        }
    }
}
