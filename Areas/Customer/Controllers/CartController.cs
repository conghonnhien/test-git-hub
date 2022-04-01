using DienTu.Data;
using DienTu.Models;
using DienTu.Models.ViewModels;
using DienTu.Service;
using DienTu.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DienTu.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        

        [BindProperty]
        public OrderDetailsCart detailsCart { get; set; }
        public CartController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
            
        }
        public async Task<IActionResult> Index()
        {
            detailsCart = new OrderDetailsCart() // them tieu de cua the
            {
                OrderHeader = new Models.OrderHeader(),
            };

            detailsCart.OrderHeader.OrderTotal = 0; // don dat hang bang 0

            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var cart = _db.ShoppingCart.Where(a => a.ApplicationUserId == claim.Value);//cart do nguoi dung chon

            if(cart!=null)
            {
                detailsCart.listCart = cart.ToList();
            }

            foreach(var list in detailsCart.listCart)
            {
                list.MenuItem = await _db.MenuItem.FirstOrDefaultAsync(a => a.Id == list.MenuItemId);
                detailsCart.OrderHeader.OrderTotal = detailsCart.OrderHeader.OrderTotal + (list.MenuItem.Price * list.Count);

                //list.MenuItem.Description = SD.ConvertToRawHtml(list.MenuItem.Description);
                //if(list.MenuItem.Description.Length>100)
                //{
                //    list.MenuItem.Description = list.MenuItem.Description.Substring(0, 99) + "...";
                //}
            }
            detailsCart.OrderHeader.OrderTotalOriginal = detailsCart.OrderHeader.OrderTotal;
            if (HttpContext.Session.GetString(SD.ssCouponCode)!=null)
            {
                detailsCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupon.Where(c => c.Name.ToLower() == detailsCart.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                detailsCart.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, detailsCart.OrderHeader.OrderTotalOriginal);
            }

            
            return View(detailsCart);
        }

        public IActionResult AddCoupon()
        {
            if(detailsCart.OrderHeader.CouponCode==null)
            {
                detailsCart.OrderHeader.CouponCode = "";
            }
            HttpContext.Session.SetString(SD.ssCouponCode, detailsCart.OrderHeader.CouponCode);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveCoupon()
        {
            HttpContext.Session.SetString(SD.ssCouponCode, string.Empty);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(a => a.Id == cartId);
            cart.Count++;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(a => a.Id == cartId);
            if(cart.Count==1)
            {
                _db.ShoppingCart.Remove(cart);
                await _db.SaveChangesAsync();

                var cnt = _db.ShoppingCart.Where(a => a.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);
            }
            else
            {
                cart.Count--;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(a => a.Id == cartId);
            _db.ShoppingCart.Remove(cart);

            await _db.SaveChangesAsync();

            //var cnt = _db.ShoppingCart.Where(a => a.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
            //HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);

            var cnt = _db.ShoppingCart.Where(a => a.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
            HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Summary ()
        {
            detailsCart = new OrderDetailsCart()
            {
                OrderHeader = new OrderHeader()
            };
            detailsCart.OrderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ApplicationUser applicationUser = await _db.ApplicationUser.Where(a => a.Id == claim.Value).FirstOrDefaultAsync();

            var cart = _db.ShoppingCart.Where(a => a.ApplicationUserId == claim.Value);

            if(cart!=null)
            {
                detailsCart.listCart = cart.ToList();
            }
            foreach(var list in detailsCart.listCart)
            {
                list.MenuItem = await _db.MenuItem.Where(a => a.Id == list.MenuItemId).FirstOrDefaultAsync();
                detailsCart.OrderHeader.OrderTotal = detailsCart.OrderHeader.OrderTotal + (list.MenuItem.Price * list.Count);
            }
            detailsCart.OrderHeader.OrderTotalOriginal = detailsCart.OrderHeader.OrderTotal;
            detailsCart.OrderHeader.PickUpName = applicationUser.Name;
            detailsCart.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
            detailsCart.OrderHeader.PickUpTime = DateTime.Now;

            if(HttpContext.Session.GetString(SD.ssCouponCode)!=null)
            {
                detailsCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupon.Where(a => a.Name.ToLower() == detailsCart.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                detailsCart.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb,detailsCart.OrderHeader.OrderTotalOriginal);
            }
            return View(detailsCart);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult>SummaryPost(string stripeEmail, string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            detailsCart.listCart = await _db.ShoppingCart.Where(a => a.ApplicationUserId == claim.Value).ToListAsync();

            detailsCart.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            detailsCart.OrderHeader.OrderDate = DateTime.Now;
            detailsCart.OrderHeader.UserId = claim.Value;
            detailsCart.OrderHeader.Status = SD.PaymentStatusPending;
            detailsCart.OrderHeader.PickUpTime = Convert.ToDateTime(detailsCart.OrderHeader.PickUpTime.ToShortTimeString() + " " + detailsCart.OrderHeader.PickUpDate.ToShortDateString());
            List<OrderDetails> orderDetailsList = new List<OrderDetails>();
            _db.OrderHeader.Add(detailsCart.OrderHeader);
            await _db.SaveChangesAsync();

            detailsCart.OrderHeader.OrderTotalOriginal = 0;
            foreach(var item in detailsCart.listCart)
            {
                item.MenuItem = await _db.MenuItem.Where(a => a.Id == item.MenuItemId).FirstOrDefaultAsync();

                OrderDetails orderDetails = new OrderDetails
                {
                    MenuItemId = item.MenuItemId,
                    OrderId = detailsCart.OrderHeader.Id,
                    Description = item.MenuItem.Guarantee,
                    Name = item.MenuItem.Name,
                    Price = item.MenuItem.Price,
                    Count = item.Count,
                };

                detailsCart.OrderHeader.OrderTotalOriginal += orderDetails.Count * orderDetails.Price;
                _db.OrderDetails.Add(orderDetails);
            }

            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                detailsCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupon.Where(a => a.Name.ToLower() == detailsCart.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                detailsCart.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, detailsCart.OrderHeader.OrderTotalOriginal);
            }
            else
            {
                detailsCart.OrderHeader.OrderTotal = detailsCart.OrderHeader.OrderTotalOriginal;
            }

            detailsCart.OrderHeader.CouponCodeDiscount = detailsCart.OrderHeader.OrderTotalOriginal - detailsCart.OrderHeader.OrderTotal;
            _db.ShoppingCart.RemoveRange(detailsCart.listCart);//xoa gio hang

            HttpContext.Session.SetInt32(SD.ssShoppingCartCount,0);
            await _db.SaveChangesAsync();

            var options = new ChargeCreateOptions
            {
                Amount = Convert.ToInt32(detailsCart.OrderHeader.OrderTotal*100),
                Currency = "usd",
                Description = "Order ID : " + detailsCart.OrderHeader.Id,
                Source = stripeToken,
            };

            var service = new ChargeService();
            Charge charge = service.Create(options);

            if(charge.BalanceTransactionId ==null)
            {
                detailsCart.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }
            else
            {
                detailsCart.OrderHeader.TransactionId = charge.BalanceTransactionId;
            }

            if(charge.Status.ToLower()=="succeeded")
            {
                var tienthanhtoan = await _db.OrderHeader.Where(a => a.UserId == claim.Value).OrderByDescending(x => x.Id).FirstAsync();
                double a = tienthanhtoan.OrderTotal;


                await _emailSender.SendEmailAsync(_db.Users.Where(a => a.Id == claim.Value).FirstOrDefault().Email, "DienTu-Bạn đã thanh toán cho đơn hàng mã :" + detailsCart.OrderHeader.Id.ToString(), "Đơn hàng được thanh toán thành công với số tiền :" + a.ToString() + "và đang được chuẩn bị. Quí khách có thể theo dõi trạng thái đơn hàng ở mục lịch sử đặt hàng. Xin cảm ơn, mọi thắc mắc xin liên hệ: 0934936206");





                detailsCart.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                detailsCart.OrderHeader.Status = SD.StatusSubmitted;
                //await _emailSender.SendEmailAsync(_db.Users.Where(a => a.Id == claim.Value).FirstOrDefault().Email, "DienTu-Bạn đã thanh toán cho đơn hàng mã :" + detailsCart.OrderHeader.Id.ToString(), "Đơn hàng được thanh toán thành công với số tiền :"  + a + "và đang được chuẩn bị. Quí khách có thể theo dõi trạng thái đơn hàng ở mục lịch sử đặt hàng. Xin cảm ơn, mọi thắc mắc xin liên hệ: 0934936206");
            }
            else
            {
                detailsCart.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }
            await _db.SaveChangesAsync();

            //return RedirectToAction("NotificationPayment");
            //return RedirectToAction("Index", "Homme");
            return RedirectToAction("Confirm", "Order", new { id = detailsCart.OrderHeader.Id });
        }

        public async  Task<IActionResult> NotificationPayment()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            
            var tienthanhtoan = await _db.OrderHeader.Where(a => a.UserId == claim.Value).OrderByDescending(x =>x.Id).FirstAsync();
            ViewBag.OrderTotal = tienthanhtoan.OrderTotal;

            //return RedirectToAction("Confirm", "Order", new { id = detailsCart.OrderHeader.Id });
            return View(detailsCart);
        }

        

    }
}
