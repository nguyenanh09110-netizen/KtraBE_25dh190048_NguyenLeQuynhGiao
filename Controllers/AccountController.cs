using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBH.Models;

namespace WebBH.Controllers
{
    public class AccountController : Controller
    {
        private QuanlyBHEntities db = new QuanlyBHEntities();

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return View();
            }

            var user = db.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user == null)
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu.";
                return View();
            }

            Session["Username"] = user.Username;
            Session["Role"] = string.IsNullOrEmpty(user.UserRole) ? "Customer" : user.UserRole;

            if (Session["Role"].ToString() == "Admin")
            {
                return RedirectToAction("Index", "Products");
            }

            return RedirectToAction("Index", "Shop");
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string username, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập tên đăng nhập và mật khẩu.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu nhập lại không khớp.";
                return View();
            }

            var existed = db.Users.Any(u => u.Username == username);
            if (existed)
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                return View();
            }

            var newUser = new User
            {
                Username = username,
                Password = password,
                UserRole = "User"
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            Session["Username"] = newUser.Username;
            Session["Role"] = newUser.UserRole;

            return RedirectToAction("Index", "Shop");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult MyOrder(string search, string status = "Tất cả đơn", bool showMore = false)
        {
            if (Session["Username"] == null)
                return RedirectToAction("Login", "Account");

            string username = Session["Username"].ToString();
            var myOrders = db.Orders.Include("OrderDetails.Product").Include("Customer")
                                    .Where(o => o.Customer.Username == username);

            if (!myOrders.Any() && string.IsNullOrEmpty(search))
            {
                ViewBag.HasAnyOrder = false;
                return View(new List<Order>());
            }
            ViewBag.HasAnyOrder = true;

            if (!string.IsNullOrEmpty(search))
            {
                myOrders = myOrders.Where(o => o.OrderID.ToString().Contains(search)
                                          || o.OrderDetails.Any(d => d.Product.ProductName.Contains(search)));
            }

            if (!string.IsNullOrEmpty(status) && status != "Tất cả đơn")
            {
                myOrders = myOrders.Where(o => o.OrderStatus == status);
            }

            int limit = showMore ? 50 : 2;
            var result = myOrders.OrderByDescending(o => o.OrderDate).Take(limit).ToList();

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.ShowMore = showMore;

            return View(result);
        }

        public ActionResult Rebuy(int productId)
        {
            var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
            var existing = cart.FirstOrDefault(x => x.ProductID == productId);
            if (existing != null)
            {
                existing.Quantity += 1;
            }
            else
            {
                var prod = db.Products.Find(productId);
                if (prod != null)
                {
                    cart.Add(new CartItem
                    {
                        ProductID = prod.ProductID,
                        ProductName = prod.ProductName,
                        UnitPrice = prod.ProductPrice,
                        Quantity = 1
                    });
                }
            }
            Session["Cart"] = cart;
            return RedirectToAction("Cart", "Shop");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}