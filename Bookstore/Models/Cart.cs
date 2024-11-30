﻿using Bookstore.Data;
using BookStore.Models;
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

        public CartItem GetCartItem(Book book)
        {
            return _context.CartItems.SingleOrDefault(ci =>
                 ci.Book.Id == book.Id && ci.CartId == Id);
        }

        public void AddToCart(Book book, int quantity)
        {
            var cartItem = GetCartItem(book);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    Book = book,
                    Quantity = quantity,
                    CartId = Id
                };

                _context.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity;
            }
            _context.SaveChanges();
        }

        public int ReduceQuantity(Book book)
        {
            var cartItem = GetCartItem(book);
            var remainingQuantity = 0;

            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    remainingQuantity = --cartItem.Quantity;
                }
                else
                {
                    _context.CartItems.Remove(cartItem);
                }
            }
            _context.SaveChanges();

            return remainingQuantity;
        }

        public int IncreaseQuantity(Book book)
        {
            var cartItem = GetCartItem(book);
            var remainingQuantity = 0;

            if (cartItem != null)
            {
                if (cartItem.Quantity > 0)
                {
                    remainingQuantity = ++cartItem.Quantity;
                }
            }
            _context.SaveChanges();

            return remainingQuantity;
        }

        public List<CartItem> GetAllCartItems()
        {
            return CartItems ?? (CartItems = _context.CartItems.Where(ci => ci.CartId == Id)
                .Include(ci => ci.Book)
                .ToList());
        }

        public int GetCartTotal()
        {
            return _context.CartItems
                .Where(ci => ci.CartId == Id)
                .Select(ci => ci.Book.Price * ci.Quantity)
                .Sum();
        }

    }
}
