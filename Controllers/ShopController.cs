using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebBH.Models;

namespace WebBH.Controllers
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }
    public class ShopController : Controller
    {
        private QuanlyBHEntities db = new QuanlyBHEntities();
        private const string CartSessionKey = "CART_SESSION";

        private List<CartItem> GetCart()
        {
            var cart = Session[CartSessionKey] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                Session[CartSessionKey] = cart;
            }
            return cart;
        }

        public ActionResult Index(int? categoryId, string search)
        {
            var products = db.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryID == categoryId.Value);

            if (!string.IsNullOrEmpty(search))
                products = products.Where(p => p.ProductName.Contains(search));

            ViewBag.Categories = db.Categories.ToList();
            return View(products.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductID == id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        public ActionResult AddToCart(int id, int qty = 1)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductID == id);

            if (item != null)
            {
                item.Quantity += qty;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    ProductImage = product.ProductImage,
                    UnitPrice = product.ProductPrice,
                    Quantity = qty
                });
            }

            return RedirectToAction("Cart");
        }

        [HttpGet]
        public ActionResult Cart()
        {
            var cart = GetCart();
            ViewBag.TotalAmount = cart.Sum(x => x.TotalPrice);
            return View(cart);
        }

        [HttpPost]
        public ActionResult UpdateCart(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductID == productId);
            if (item != null)
            {
                if (quantity <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = quantity;
            }
            return RedirectToAction("Cart");
        }

        public ActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductID == id);
            if (item != null) cart.Remove(item);
            return RedirectToAction("Cart");
        }

        [HttpGet]
        public ActionResult Checkout()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng đang trống!";
                return RedirectToAction("Index");
            }

            ViewBag.Cart = cart;
            ViewBag.TotalAmount = cart.Sum(x => x.TotalPrice);
            return View();
        }

        [HttpPost]
        public ActionResult Checkout(string customerName, string customerPhone, string customerEmail, string addressDelivery)
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng đang trống!";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(customerPhone) || string.IsNullOrWhiteSpace(addressDelivery))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ họ tên, số điện thoại và địa chỉ giao hàng.");
                ViewBag.Cart = cart;
                ViewBag.TotalAmount = cart.Sum(x => x.TotalPrice);
                return View();
            }

            Customer customer = null;
            if (Session["Username"] != null)
            {
                string uname = Session["Username"].ToString();
                customer = db.Customers.FirstOrDefault(c => c.Username == uname);
            }

            if (customer == null)
            {
                customer = new Customer
                {
                    CustomerName = customerName,
                    CustomerPhone = customerPhone,
                    CustomerEmail = customerEmail,
                    CustomerAddress = addressDelivery,
                    Username = Session["Username"]?.ToString()
                };
                db.Customers.Add(customer);
                db.SaveChanges();
            }
            else
            {
                customer.CustomerName = customerName;
                customer.CustomerPhone = customerPhone;
                customer.CustomerEmail = customerEmail;
                customer.CustomerAddress = addressDelivery;
                db.Entry(customer).State = EntityState.Modified;
            }

            var order = new Order
            {
                CustomerID = customer.CustomerID,
                OrderDate = DateTime.Now,
                TotalAmount = cart.Sum(x => x.TotalPrice),
                OrderStatus = "Chờ xác nhận",
                PaymentStatus = "Chờ thanh toán",
                AddressDelivery = addressDelivery
            };
            db.Orders.Add(order);
            db.SaveChanges();

            foreach (var item in cart)
            {
                var detail = new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };
                db.OrderDetails.Add(detail);

                var prod = db.Products.Find(item.ProductID);
                if (prod != null)
                {
                    prod.Quantity = Math.Max(0, prod.Quantity - item.Quantity);
                    prod.Sold = (prod.Sold ?? 0) + item.Quantity;
                }
            }

            db.SaveChanges();
            Session.Remove(CartSessionKey);

            return RedirectToAction("OrderSuccess", new { id = order.OrderID });
        }

        public ActionResult OrderSuccess(int? id)
        {
            if (id == null) return RedirectToAction("Index");
            var order = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null) return HttpNotFound();
            return View(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}