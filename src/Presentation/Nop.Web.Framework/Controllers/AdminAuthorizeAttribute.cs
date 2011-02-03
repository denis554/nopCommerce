﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;

namespace Nop.Web.Framework.Controllers
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool isAdmin = false;
            //uncomment code below after we add authrorization pages (login/register)
            //TODO inject IWorkContext
            //var workContext = DependencyResolver.Current.GetService<IWorkContext>();
            //var user = workContext.CurrentCustomer;
            //if (user != null)
            //{
            //    //TODO add some helper method (for example, ICustomerManager.IsAdmin(Customer customer))
            //    isAdmin= user.CustomerRoles.Where(cr => cr.IsSystemRole && cr.SystemName == SystemCustomerRoleNames.Administrators).Any();
            //}

            //remove code below after we add authrorization pages (login/register)
            isAdmin = true;
            return isAdmin;
        }
    }
}
