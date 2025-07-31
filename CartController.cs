using Food_Menu_Management_System.Hubs;
using Food_Menu_Management_System.Models;
using Food_Menu_Management_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Food_Menu_Management_System.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IHubContext<CartHub> _hubContext;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IHubContext<CartHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId) 
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) 
            {
                return BadRequest("User is not authenticated.");
            }

            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            var cartItem = _context.CartItems.FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity += 1;
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = 1
                };
                _context.CartItems.Add(cartItem);
            }
            _context.SaveChanges();

            var cartCount = _context.CartItems.Where(ci => ci.CartId == cart.Id).Sum(ci => ci.Quantity);
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", User.Identity?.Name ?? "Guest", "added a product to their cart.");
            return Json(new { cartCount });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteItem(int itemId) 
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) 
            {
                return BadRequest("User is not authenticated.");
            }

            var cartItem = _context.CartItems
                .FirstOrDefault(ci => ci.Id == itemId && ci.Cart.UserId == userId);

            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity -= 1;
                }
                else
                {
                    _context.CartItems.Remove(cartItem);
                }
                _context.SaveChanges();
            }

            var cart = _context.Carts
                .Include(c => c.Items)
                .FirstOrDefault(c => c.UserId == userId);
            var cartCount = cart?.Items?.Sum(i => i.Quantity) ?? 0;
            await _hubContext.Clients.User(userId).SendAsync("UpdateCartCount", cartCount); // Await the async call
            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            var cart = _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, Items = new List<CartItem>() };
            }
            else if (cart.Items == null)
            {
                cart.Items = new List<CartItem>();
            }

            return View(cart);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult AllCarts()
        {
            var carts = _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .ToList();

            var users = _context.Users.ToList();
            ViewBag.Users = users;
            return View(carts);
        }
    }
}