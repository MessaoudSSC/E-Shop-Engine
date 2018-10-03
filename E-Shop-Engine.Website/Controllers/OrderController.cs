﻿using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using E_Shop_Engine.Domain.DomainModel;
using E_Shop_Engine.Domain.DomainModel.IdentityModel;
using E_Shop_Engine.Domain.Interfaces;
using E_Shop_Engine.Services.Data.Identity;
using E_Shop_Engine.Website.Models;
using Microsoft.AspNet.Identity;
using X.PagedList;

namespace E_Shop_Engine.Website.Controllers
{
    public class OrderController : PagingBaseController
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly AppUserManager _userManager;

        public OrderController(IRepository<Order> orderRepository, ICartRepository cartRepository, AppUserManager userManager)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _userManager = userManager;
        }

        // GET: Order
        public ActionResult Index(int? page, string sortOrder, bool descending = true)
        {
            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "Created";
            }
            int pageNumber = page ?? 1;
            string userId = HttpContext.User.Identity.GetUserId();
            AppUser user = _userManager.FindById(userId);

            IQueryable<Order> model = user.Orders.AsQueryable();
            IQueryable<OrderViewModel> mappedModel = model.ProjectTo<OrderViewModel>();

            PropertyInfo sortBy = typeof(OrderViewModel).GetProperty(sortOrder);
            mappedModel = descending ? mappedModel.OrderByDescending(x => sortBy.GetValue(x)) : mappedModel.OrderBy(x => sortBy.GetValue(x));

            IPagedList<OrderViewModel> viewModel = mappedModel.ToPagedList(pageNumber, 1);

            ViewBag.SortOrder = sortOrder;
            ViewBag.SortDescending = descending;

            viewModel = viewModel.Select(x =>
            {
                x.Created = x.Created.ToLocalTime();
                return x;
            });
            return View(viewModel);
        }

        public ActionResult Create()
        {
            OrderViewModel model = new OrderViewModel()
            {
                PaymentMethod = null
            };
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(OrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            string userId = HttpContext.User.Identity.GetUserId();
            AppUser user = _userManager.FindById(userId);
            model.OrderedCart = Mapper.Map<OrderedCart>(user.Cart);
            model.Created = DateTime.UtcNow;
            model.AppUser = user;

            if (model.OrderedCart.CartLines.Count == 0)
            {
                return View("_Error", new string[] { "Cannot order empty cart." });
            }

            _orderRepository.Create(Mapper.Map<Order>(model));
            _cartRepository.Clear(user.Cart);

            return Redirect("/Home/Index");
        }

        public ActionResult Details(int id)
        {
            Order model = _orderRepository.GetById(id);

            string userId = HttpContext.User.Identity.GetUserId();
            AppUser user = _userManager.FindById(userId);

            if (user.Orders.Contains(model))
            {
                OrderViewModel viewModel = Mapper.Map<OrderViewModel>(model);
                viewModel.Created = viewModel.Created.ToLocalTime();
                return View(viewModel);
            }
            ModelState.AddModelError("", "Order Not Found");
            return RedirectToAction("Index");
        }
    }
}