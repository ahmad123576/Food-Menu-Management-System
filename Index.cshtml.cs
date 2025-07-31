using Food_Menu_Management_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Food_Menu_Management_System.Pages.Admin.Carts
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public List<Food_Menu_Management_System.Models.Cart> AllCarts { get; set; } = new();



        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            AllCarts = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .ToListAsync();
        }
    }
}
