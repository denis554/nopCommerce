﻿using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Areas.Admin.Components
{
    /// <summary>
    /// Represents a view component that displays common statistics
    /// </summary>
    public class CommonStatisticsViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IHomeModelFactory _homeModelFactory;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public CommonStatisticsViewComponent(IHomeModelFactory homeModelFactory,
            IPermissionService permissionService,
            IWorkContext workContext)
        {
            this._homeModelFactory = homeModelFactory;
            this._permissionService = permissionService;
            this._workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <returns>View component result</returns>
        public IViewComponentResult Invoke()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers) ||
                !_permissionService.Authorize(StandardPermissionProvider.ManageOrders) ||
                !_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests) ||
                !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
            {
                return Content("");
            }

            //a vendor doesn't have access to this report
            if (_workContext.CurrentVendor != null)
                return Content("");

            //prepare model
            var model = _homeModelFactory.PrepareCommonStatisticsModel();

            return View(model);
        }

        #endregion
    }
}