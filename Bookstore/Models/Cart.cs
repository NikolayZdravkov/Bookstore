﻿using Bookstore.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Models
{
    public class Cart
    {
        private readonly BookstoreContext _context;

        public Cart(BookstoreContext context)
        {
            _context = context;
        }

        public string Id { get; set; }

        public List<CartItem> CartItems { get; set; }

        public static Cart GetCart(IServiceProvider services)
        {
            ISession? session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;

            var context = services.GetService<BookstoreContext>();
            string cartId = session.GetString("Id") ?? Guid.NewGuid().ToString();

            session.SetString("Id", cartId);

            return new Cart(context) { Id = cartId };
        }

        public List<CartItem> GetCartItems()
        {
            return CartItems ?? (CartItems = _context.CartItems.Where(ci => ci.CartId == Id)
                .Include(ci => ci.Book)
                .ToList());
        }

    }
}
