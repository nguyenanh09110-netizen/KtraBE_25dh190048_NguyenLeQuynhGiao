using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WebBH.Helpers;
using WebBH.Models;

namespace WebBH.Controllers
{
    [AdminAuthorize]
    public class OrdersController : Controller
    {
        private QuanlyBHEntities db = new QuanlyBHEntities();

        public ActionResult Index(string customerName, string paymentStatus, string status, DateTime? orderDate, int page = 1)
        {
            int pageSize = 15;
            var orders = db.Orders.Include(o => o.Customer).AsQueryable();

            if (!string.IsNullOrWhiteSpace(customerName))
            {
                orders = orders.Where(o => o.Customer != null && o.Customer.CustomerName.Contains(customerName));
            }

            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                orders = orders.Where(o => o.PaymentStatus == paymentStatus);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                orders = orders.Where(o => o.OrderStatus == status);
            }

            if (orderDate.HasValue)
            {
                var targetDate = orderDate.Value.Date;
                orders = orders.Where(o => DbFunctions.TruncateTime(o.OrderDate) == targetDate);
            }

            var orderedQuery = orders.OrderByDescending(o => o.OrderID);

            int totalItems = orderedQuery.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (page < 1) page = 1;
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var pagedOrders = orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CustomerName = customerName;
            ViewBag.PaymentStatus = paymentStatus;
            ViewBag.CurrentStatus = status;
            ViewBag.OrderDate = orderDate.HasValue ? orderDate.Value.ToString("yyyy-MM-dd") : "";
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(pagedOrders);
        }

        public ActionResult Details(int? id)
        {
            return DetailedOrder(id);
        }

        public ActionResult DetailedOrder(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var order = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null) return HttpNotFound();

            return View(order);
        }

        public ActionResult UpdateStatus(int id)
        {
            var order = db.Orders.Find(id);
            if (order == null) return HttpNotFound();
            return View(order);
        }

        [HttpPost]
        public ActionResult UpdateStatus(int OrderId, string PaymentStatus, string OrderStatus)
        {
            var order = db.Orders.Find(OrderId);
            if (order == null) return HttpNotFound();

            if (!string.IsNullOrWhiteSpace(PaymentStatus))
            {
                order.PaymentStatus = PaymentStatus;
            }

            if (!string.IsNullOrWhiteSpace(OrderStatus))
            {
                order.OrderStatus = OrderStatus;
            }

            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("DetailedOrder", new { id = OrderId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}