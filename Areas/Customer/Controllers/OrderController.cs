using DienTu.Data;
using DienTu.Models;
using DienTu.Models.ViewModels;
using DienTu.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DienTu.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        private int PageSize = 20;
        public OrderController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }

        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = await _db.OrderHeader.Include(a => a.ApplicationUser).FirstOrDefaultAsync(a => a.Id == id && a.UserId == claim.Value),
                OrderDetails = await _db.OrderDetails.Where(a => a.OrderId == id).ToListAsync()
            };

            return View(orderDetailsViewModel);
        }


        [Authorize]
        public async Task<IActionResult> OrderHistory(int productPage=1)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderListViewModel orderListVM = new OrderListViewModel()
            {
                Orders  = new List<OrderDetailsViewModel>(),
            };

            //List<OrderDetailsViewModel> orderList = new List<OrderDetailsViewModel>();

            List<OrderHeader> orderHeaderList = await _db.OrderHeader.Include(a => a.ApplicationUser)
                                                 .Where(a => a.UserId == claim.Value).ToListAsync();

            foreach (OrderHeader item in orderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = await _db.OrderDetails.Where(a => a.OrderId == item.Id).ToListAsync(),
                };
                orderListVM.Orders.Add(individual);
            }

            var count = orderListVM.Orders.Count;
            orderListVM.Orders = orderListVM.Orders.OrderByDescending(a => a.OrderHeader.Id)
                                                   .Skip((productPage - 1) * PageSize).Take(PageSize).ToList();

            orderListVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItem = count,
                urlParam = "/Customer/Order/OrderHistory?productPage:"
            };

            return View(orderListVM);
        }
        

        public async Task<IActionResult> GetOrderDetails(int Id)
        {
            OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = await _db.OrderHeader.Where(a => a.Id == Id).FirstOrDefaultAsync(),
                OrderDetails= await _db.OrderDetails.Where(a=>a.OrderId==Id).ToListAsync(),
            };
            orderDetailsViewModel.OrderHeader.ApplicationUser = await _db.ApplicationUser
                                                               .Where(a => a.Id == orderDetailsViewModel.OrderHeader.UserId)
                                                               .FirstOrDefaultAsync();
            return PartialView("_IndividualOrderDetails", orderDetailsViewModel);
        }

        public IActionResult getOrderStutus(int Id)
        {
            return PartialView("_OrderStatus", _db.OrderHeader.Where(a => a.Id == Id).FirstOrDefault().Status);
        }

        [Authorize(Roles =SD.ManagerUser)]
        public async Task<IActionResult> ManageOrder()
        {
            List<OrderDetailsViewModel> orderDetailsVM = new List<OrderDetailsViewModel>();

            List<OrderHeader> orderHeadersList = await _db.OrderHeader
                                               .Where(a => a.Status == SD.StatusSubmitted || a.Status == SD.StatusInProcess)
                                               .OrderByDescending(a => a.PickUpTime).ToListAsync();

            foreach(OrderHeader item in orderHeadersList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = await _db.OrderDetails.Where(a => a.OrderId == item.Id).ToListAsync(),
                };
                orderDetailsVM.Add(individual);
            }
            return View(orderDetailsVM.OrderBy(a => a.OrderHeader.PickUpTime).ToList());
        }

        [Authorize]
        public async Task<IActionResult> History(int id)
        {

            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            List<OrderDetailsViewModel> orderDetailsVM = new List<OrderDetailsViewModel>();
            OrderListViewModel orderListVM = new OrderListViewModel()
            {
                Orders = new List<OrderDetailsViewModel>(),
            };

            List<OrderHeader> orderHeaderList = await _db.OrderHeader.Include(a => a.ApplicationUser)
                                                     .Where(a => a.UserId == claim.Value && a.Id == id).ToListAsync();

            foreach (OrderHeader item in orderHeaderList)
            {
                OrderDetailsViewModel indiviual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = await _db.OrderDetails.Where(a => a.OrderId == item.Id && a.OrderId == id).ToListAsync(),
                };
                orderDetailsVM.Add(indiviual);
            }
            return View(orderDetailsVM);
        }
        [Authorize(Roles =SD.ManagerUser)]
        public async Task<IActionResult> OrderPrepare(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusInProcess;
            await _db.SaveChangesAsync();

            await _emailSender.SendEmailAsync(_db.Users.Where(a => a.Id == orderHeader.UserId).FirstOrDefault().Email, "DienTu- Đơn hàng mã :" + orderHeader.Id.ToString(), " đang đóng gói. Xin cảm ơn, mọi thắc mắc xin liên hệ: 0934936206");

            return RedirectToAction("ManageOrder", "Order");
        }


        [Authorize(Roles = SD.ManagerUser)]
        public async Task<IActionResult> StatusShip(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusShip;
            await _db.SaveChangesAsync();

            await _emailSender.SendEmailAsync(_db.Users.Where(a => a.Id == orderHeader.UserId).FirstOrDefault().Email, "DienTu- Đơn hàng bạn đang ship :" + orderHeader.Id.ToString(), " Xin cảm ơn, mọi thắc mắc xin liên hệ: 0934936206");

            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize(Roles = SD.ManagerUser)]
        public async Task<IActionResult> OrderCancel(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusCancelled;
            await _db.SaveChangesAsync();

            await _emailSender.SendEmailAsync(_db.Users.Where(a => a.Id == orderHeader.UserId).FirstOrDefault().Email, "DienTu-Bạn đã huỷ đơn hàng :" + orderHeader.Id.ToString(), "Đơn hàng đã được huỷ. Xin cảm ơn, mọi thắc mắc xin liên hệ: 0934936206");
            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize(Roles =SD.ManagerUser)]
        public async Task<IActionResult> OrderPickup(int productPage = 1)
        {
            OrderListViewModel orderListVM = new OrderListViewModel()
            {
                Orders = new List<OrderDetailsViewModel>(),
            };
            StringBuilder param = new StringBuilder();
            param.Append("/Customer/Order/OrderPickUp?productPage=:");
          

            List<OrderHeader> orderHeadersList = new List<OrderHeader>();         
            orderHeadersList = await _db.OrderHeader.Include(a => a.ApplicationUser)
                                     .Where(a => a.Status == SD.StatusShip).ToListAsync();
            foreach (OrderHeader item in orderHeadersList)
                {
                    OrderDetailsViewModel individual = new OrderDetailsViewModel
                    {
                        OrderHeader = item,
                        OrderDetails = await _db.OrderDetails.Where(a => a.OrderId == item.Id).ToListAsync(),
                    };
                    orderListVM.Orders.Add(individual);
                }
            var count = orderListVM.Orders.Count;
            orderListVM.Orders = orderListVM.Orders.OrderByDescending(a => a.OrderHeader.Id)
                                                   .Skip((productPage - 1) * PageSize).Take(PageSize).ToList();

            orderListVM.PagingInfo = new PagingInfo
           {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItem = count,
                urlParam = param.ToString(),
            };
            return View(orderListVM);
            
        }
        //[Authorize(Roles = SD.ManagerUser)]
        //public async Task<IActionResult> OrderPickup(int productPage = 1, string searchEmail = null, string searchPhone = null, string searchName = null)
        //{
        //    OrderListViewModel orderListVM = new OrderListViewModel()
        //    {
        //        Orders = new List<OrderDetailsViewModel>(),
        //    };
        //    StringBuilder param = new StringBuilder();
        //    param.Append("/Customer/Order/OrderPickUp?productPage=:");
        //    param.Append("&searchName=");

        //    if (searchName != null)
        //    {
        //        param.Append(searchName);
        //    }
        //    param.Append("&searchEmail=");
        //    if (searchEmail != null)
        //    {
        //        param.Append(searchEmail);
        //    }
        //    param.Append("&searchPhone=");
        //    if (searchPhone != null)
        //    {
        //        param.Append(searchPhone);
        //    }


        //    //List<OrderHeader> orderHeadersList = await _db.OrderHeader.Include(a => a.ApplicationUser)
        //    //                                          .Where(a => a.Status == SD.StatusShip).ToListAsync();

        //    //foreach (OrderHeader item in orderHeadersList)
        //    //{
        //    //    OrderDetailsViewModel individual = new OrderDetailsViewModel
        //    //    {
        //    //        OrderHeader = item,
        //    //        OrderDetails = await _db.OrderDetails.Where(a => a.OrderId == item.Id).ToListAsync(),
        //    //    };
        //    //    orderListVM.Orders.Add(individual);
        //    //}

        //    //var count = orderListVM.Orders.Count;
        //    //orderListVM.Orders = orderListVM.Orders.OrderByDescending(a => a.OrderHeader.Id)
        //    //                                       .Skip((productPage - 1) * PageSize).Take(PageSize).ToList();

        //    //orderListVM.PagingInfo = new PagingInfo
        //    //{
        //    //    CurrentPage = productPage,
        //    //    ItemsPerPage = PageSize,
        //    //    TotalItem = count,
        //    //    urlParam = param.ToString(),
        //    //};

        //    List<OrderHeader> orderHeadersList = new List<OrderHeader>();
        //    if (searchName != null || searchEmail != null || searchPhone != null)
        //    {
        //        var user = new ApplicationUser();

        //        if (searchName != null)
        //        {
        //            orderHeadersList = await _db.OrderHeader.Include(a => a.ApplicationUser)
        //                                    .Where(a => a.PickUpName.ToLower().Contains(searchName.ToLower()))
        //                                    .OrderByDescending(a => a.OrderDate).ToListAsync();
        //        }
        //        else
        //        {
        //            if (searchEmail != null)
        //            {
        //                user = await _db.ApplicationUser.Where(a => a.Email.ToLower().Contains(searchEmail.ToLower())).FirstOrDefaultAsync();
        //                orderHeadersList = await _db.OrderHeader.Include(a => a.ApplicationUser)
        //                                        .Where(a => a.UserId == user.Id).OrderByDescending(a => a.OrderDate).ToListAsync();

        //            }
        //            else
        //            {
        //                if (searchPhone != null)
        //                {
        //                    orderHeadersList = await _db.OrderHeader.Include(a => a.ApplicationUser)
        //                                           .Where(a => a.PhoneNumber.Contains(searchPhone))
        //                                           .OrderByDescending(a => a.OrderDate).ToListAsync();
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        orderHeadersList = await _db.OrderHeader.Include(a => a.ApplicationUser)
        //                                         .Where(a => a.Status == SD.StatusShip).ToListAsync();
        //    }
        //    foreach (OrderHeader item in orderHeadersList)
        //    {
        //        OrderDetailsViewModel individual = new OrderDetailsViewModel
        //        {
        //            OrderHeader = item,
        //            OrderDetails = await _db.OrderDetails.Where(a => a.OrderId == item.Id).ToListAsync(),
        //        };
        //        orderListVM.Orders.Add(individual);
        //    }
        //    var count = orderListVM.Orders.Count;
        //    orderListVM.Orders = orderListVM.Orders.OrderByDescending(a => a.OrderHeader.Id)
        //                                           .Skip((productPage - 1) * PageSize).Take(PageSize).ToList();

        //    orderListVM.PagingInfo = new PagingInfo
        //    {
        //        CurrentPage = productPage,
        //        ItemsPerPage = PageSize,
        //        TotalItem = count,
        //        urlParam = param.ToString(),
        //    };
        //    return View(orderListVM);

        //}


        [Authorize(Roles =SD.ManagerUser)]
        [HttpPost]
        [ActionName("OrderPickup")]
        public async Task<IActionResult> OrderPickUpStatus(int orderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(orderId);
            orderHeader.Status = SD.StatusCompleted;
            await _db.SaveChangesAsync();

            await _emailSender.SendEmailAsync(_db.Users.Where(a => a.Id == orderHeader.UserId).FirstOrDefault().Email, "DienTu- Đơn hàng mã :" + orderHeader.Id.ToString(), " đã hoàn thành. Xin cảm ơn, mọi thắc mắc xin liên hệ: 0934936206");
            return RedirectToAction("OrderPickUp", "Order");
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
