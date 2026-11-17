using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using WebBH.Models;
using WebBH.Helpers;

namespace WebBH.Controllers
{
    [AdminAuthorize]
    public class ProductsController : Controller
    {
        private QuanlyBHEntities db = new QuanlyBHEntities();

        public ActionResult Index(string search, int? categoryId)
        {
            var products = db.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.ProductName.Contains(search));
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryID == categoryId.Value);
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", categoryId);
            return View(products.OrderByDescending(p => p.ProductID).ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            return View(product);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        [HttpPost]
        public ActionResult Create([Bind(Include = "ProductID,ProductName,ProductDecription,ProductPrice,ProductImage,Quantity,CategoryID")] Product product, HttpPostedFileBase uploadImage)
        {
            if (ModelState.IsValid)
            {
                if (uploadImage != null && uploadImage.ContentLength > 0)
                {
                    var fileName = System.IO.Path.GetFileName(uploadImage.FileName);
                    var folderPath = Server.MapPath("~/Image");

                    if (!System.IO.Directory.Exists(folderPath))
                    {
                        System.IO.Directory.CreateDirectory(folderPath);
                    }

                    var fullPath = System.IO.Path.Combine(folderPath, fileName);
                    uploadImage.SaveAs(fullPath);

                    product.ProductImage = fileName;
                }

                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        [HttpPost]
        public ActionResult Edit(Product product, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                var oldProduct = db.Products.AsNoTracking().FirstOrDefault(p => p.ProductID == product.ProductID);
                if (oldProduct == null) return HttpNotFound();

                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(ImageFile.FileName);
                    string folderPath = Server.MapPath("~/Images/");
                    Directory.CreateDirectory(folderPath);
                    ImageFile.SaveAs(Path.Combine(folderPath, fileName));
                    product.ProductImage = fileName;
                }
                else
                {
                    product.ProductImage = oldProduct.ProductImage;
                }

                if (product.Sold == null)
                {
                    product.Sold = oldProduct.Sold ?? 0;
                }

                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            try
            {
                db.Products.Remove(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "Không thể xóa sản phẩm này vì đã có dữ liệu liên quan (đơn hàng/chi tiết hóa đơn).");
                return View(product);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
