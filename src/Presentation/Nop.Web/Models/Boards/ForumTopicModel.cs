﻿using System.Collections.Generic;
using FluentValidation.Attributes;
using Nop.Core.Domain.Forums;
using Nop.Web.Validators.Boards;

namespace Nop.Web.Models.Boards
{
    [Validator(typeof(ForumTopicValidator))]
    public class ForumTopicModel
    {
        public ForumTopicModel()
        {
            TopicPriorities = new List<System.Web.Mvc.SelectListItem>();
        }

        public int Id { get; set; }

        public int ForumId { get; set; }

        public int TopicTypeId { get; set; }

        public string Subject { get; set; }

        public string Text { get; set; }

        public Forum Forum { get; set; }

        public bool Subscribed { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> TopicPriorities { get; set; }

        public ForumBreadcrumbModel ForumBreadcrumbModel { get; set; }

        public string PostError { get; set; }

        public bool IsCustomerAllowedToSetTopicPriority { get; set; }

        public bool IsCustomerAllowedToSubscribe { get; set; }

        public EditorType ForumEditor { get; set; }
    }
}