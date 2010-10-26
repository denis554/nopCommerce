//------------------------------------------------------------------------------
// The contents of this file are subject to the nopCommerce Public License Version 1.0 ("License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at  http://www.nopCommerce.com/License.aspx. 
// 
// Software distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. 
// See the License for the specific language governing rights and limitations under the License.
// 
// The Original Code is nopCommerce.
// The Initial Developer of the Original Code is NopSolutions.
// All Rights Reserved.
// 
// Contributor(s): _______. 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using NopSolutions.NopCommerce.BusinessLogic.Caching;
using NopSolutions.NopCommerce.BusinessLogic.Configuration.Settings;
using NopSolutions.NopCommerce.BusinessLogic.CustomerManagement;
using NopSolutions.NopCommerce.BusinessLogic.Data;
using NopSolutions.NopCommerce.BusinessLogic.Messages;
using NopSolutions.NopCommerce.BusinessLogic.Profile;
using NopSolutions.NopCommerce.BusinessLogic.Utils.Html;
using NopSolutions.NopCommerce.Common;
using NopSolutions.NopCommerce.Common.Utils;
using NopSolutions.NopCommerce.Common.Utils.Html;
using NopSolutions.NopCommerce.BusinessLogic.IoC;

namespace NopSolutions.NopCommerce.BusinessLogic.Content.Forums
{
    /// <summary>
    /// Forum manager
    /// </summary>
    public partial class ForumManager : IForumManager
    {
        #region Constants
        private const string FORUMGROUP_ALL_KEY = "Nop.forumgroup.all";
        private const string FORUMGROUP_BY_ID_KEY = "Nop.forumgroup.id-{0}";
        private const string FORUM_ALLBYFORUMGROUPID_KEY = "Nop.forum.allbyforumgroupid-{0}";
        private const string FORUM_BY_ID_KEY = "Nop.forum.id-{0}";
        private const string FORUMGROUP_PATTERN_KEY = "Nop.forumgroup.";
        private const string FORUM_PATTERN_KEY = "Nop.forum.";
        #endregion

        #region Methods

        /// <summary>
        /// Deletes a forum group
        /// </summary>
        /// <param name="forumGroupId">The forum group identifier</param>
        public void DeleteForumGroup(int forumGroupId)
        {
            var forumGroup = GetForumGroupById(forumGroupId);
            if (forumGroup == null)
                return;

            var context = ObjectContextHelper.CurrentObjectContext;
            if (!context.IsAttached(forumGroup))
                context.ForumGroups.Attach(forumGroup);
            context.DeleteObject(forumGroup);
            context.SaveChanges();

            if (this.CacheEnabled)
            {
                NopRequestCache.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
                NopRequestCache.RemoveByPattern(FORUM_PATTERN_KEY);
            }
        }

        /// <summary>
        /// Gets a forum group
        /// </summary>
        /// <param name="forumGroupId">The forum group identifier</param>
        /// <returns>Forum group</returns>
        public ForumGroup GetForumGroupById(int forumGroupId)
        {
            if (forumGroupId == 0)
                return null;

            string key = string.Format(FORUMGROUP_BY_ID_KEY, forumGroupId);
            object obj2 = NopRequestCache.Get(key);
            if (this.CacheEnabled && (obj2 != null))
            {
                return (ForumGroup)obj2;
            }

            var context = ObjectContextHelper.CurrentObjectContext;
            var query = from fg in context.ForumGroups
                        where fg.ForumGroupId == forumGroupId
                        select fg;
            var forumGroup = query.SingleOrDefault();

            if (this.CacheEnabled)
            {
                NopRequestCache.Add(key, forumGroup);
            }
            return forumGroup;
        }

        /// <summary>
        /// Gets all forum groups
        /// </summary>
        /// <returns>Forum groups</returns>
        public List<ForumGroup> GetAllForumGroups()
        {
            string key = string.Format(FORUMGROUP_ALL_KEY);
            object obj2 = NopRequestCache.Get(key);
            if (this.CacheEnabled && (obj2 != null))
            {
                return (List<ForumGroup>)obj2;
            }

            var context = ObjectContextHelper.CurrentObjectContext;
            var query = from fg in context.ForumGroups
                        orderby fg.DisplayOrder
                        select fg;
            var forumGroups = query.ToList();

            if (this.CacheEnabled)
            {
                NopRequestCache.Add(key, forumGroups);
            }
            return forumGroups;
        }

        /// <summary>
        /// Inserts a forum group
        /// </summary>
        /// <param name="forumGroup">Forum group</param>
        public void InsertForumGroup(ForumGroup forumGroup)
        {
            if (forumGroup == null)
                throw new ArgumentNullException("forumGroup");

            forumGroup.Name = CommonHelper.EnsureNotNull(forumGroup.Name);
            forumGroup.Name = CommonHelper.EnsureMaximumLength(forumGroup.Name, 200);
            forumGroup.Description = CommonHelper.EnsureNotNull(forumGroup.Description);

            var context = ObjectContextHelper.CurrentObjectContext;

            context.ForumGroups.AddObject(forumGroup);
            context.SaveChanges();

            if (this.CacheEnabled)
            {
                NopRequestCache.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
                NopRequestCache.RemoveByPattern(FORUM_PATTERN_KEY);
            }
        }

        /// <summary>
        /// Updates the forum group
        /// </summary>
        /// <param name="forumGroup">Forum group</param>
        public void UpdateForumGroup(ForumGroup forumGroup)
        {
            if (forumGroup == null)
                throw new ArgumentNullException("forumGroup");

            forumGroup.Name = CommonHelper.EnsureNotNull(forumGroup.Name);
            forumGroup.Name = CommonHelper.EnsureMaximumLength(forumGroup.Name, 200);
            forumGroup.Description = CommonHelper.EnsureNotNull(forumGroup.Description);

            var context = ObjectContextHelper.CurrentObjectContext;
            if (!context.IsAttached(forumGroup))
                context.ForumGroups.Attach(forumGroup);

            context.SaveChanges();

            if (this.CacheEnabled)
            {
                NopRequestCache.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
                NopRequestCache.RemoveByPattern(FORUM_PATTERN_KEY);
            }
        }

        /// <summary>
        /// Deletes a forum
        /// </summary>
        /// <param name="forumId">The forum identifier</param>
        public void DeleteForum(int forumId)
        {
            if (forumId == 0)
                return;

            var context = ObjectContextHelper.CurrentObjectContext;
            context.Sp_Forums_ForumDelete(forumId);

            if (this.CacheEnabled)
            {
                NopRequestCache.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
                NopRequestCache.RemoveByPattern(FORUM_PATTERN_KEY);
            }
        }

        /// <summary>
        /// Gets a forum
        /// </summary>
        /// <param name="forumId">The forum identifier</param>
        /// <returns>Forum</returns>
        public Forum GetForumById(int forumId)
        {
            if (forumId == 0)
                return null;

            string key = string.Format(FORUM_BY_ID_KEY, forumId);
            object obj2 = NopRequestCache.Get(key);
            if (this.CacheEnabled && (obj2 != null))
            {
                return (Forum)obj2;
            }

            var context = ObjectContextHelper.CurrentObjectContext;
            var query = from f in context.Forums
                        where f.ForumId == forumId
                        select f;
            var forum = query.SingleOrDefault();

            if (this.CacheEnabled)
            {
                NopRequestCache.Add(key, forum);
            }
            return forum;
        }

        /// <summary>
        /// Gets forums by group identifier
        /// </summary>
        /// <param name="forumGroupId">The forum group identifier</param>
        /// <returns>Forums</returns>
        public List<Forum> GetAllForumsByGroupId(int forumGroupId)
        {
            string key = string.Format(FORUM_ALLBYFORUMGROUPID_KEY, forumGroupId);
            object obj2 = NopRequestCache.Get(key);
            if (this.CacheEnabled && (obj2 != null))
            {
                return (List<Forum>)obj2;
            }

            var context = ObjectContextHelper.CurrentObjectContext;
            var query = from f in context.Forums
                        orderby f.DisplayOrder
                        where f.ForumGroupId == forumGroupId
                        select f;
            var forums = query.ToList();

            if (this.CacheEnabled)
            {
                NopRequestCache.Add(key, forums);
            }
            return forums;
        }

        /// <summary>
        /// Inserts a forum
        /// </summary>
        /// <param name="forum">Forum</param>
        public void InsertForum(Forum forum)
        {
            if (forum == null)
                throw new ArgumentNullException("forum");

            forum.Name = CommonHelper.EnsureNotNull(forum.Name);
            forum.Name = CommonHelper.EnsureMaximumLength(forum.Name, 200);
            forum.Description = CommonHelper.EnsureNotNull(forum.Description);

            var context = ObjectContextHelper.CurrentObjectContext;

            context.Forums.AddObject(forum);
            context.SaveChanges();

            if (this.CacheEnabled)
            {
                NopRequestCache.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
                NopRequestCache.RemoveByPattern(FORUM_PATTERN_KEY);
            }
        }

        /// <summary>
        /// Updates the forum
        /// </summary>
        /// <param name="forum">Forum</param>
        public void UpdateForum(Forum forum)
        {
            if (forum == null)
                throw new ArgumentNullException("forum");

            forum.Name = CommonHelper.EnsureNotNull(forum.Name);
            forum.Name = CommonHelper.EnsureMaximumLength(forum.Name, 200);
            forum.Description = CommonHelper.EnsureNotNull(forum.Description);

            var context = ObjectContextHelper.CurrentObjectContext;
            if (!context.IsAttached(forum))
                context.Forums.Attach(forum);
            context.SaveChanges();

            if (this.CacheEnabled)
            {
                NopRequestCache.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
                NopRequestCache.RemoveByPattern(FORUM_PATTERN_KEY);
            }
        }

        /// <summary>
        /// Update forum stats
        /// </summary>
        /// <param name="forumId">The forum identifier</param>
        /// <returns>Forum</returns>
        public void UpdateForumStats(int forumId)
        {
            if (forumId == 0)
                return;
            
            var context = ObjectContextHelper.CurrentObjectContext;
            context.Sp_Forums_ForumUpdateCounts(forumId);

            if (this.CacheEnabled)
            {
                NopRequestCache.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
                NopRequestCache.RemoveByPattern(FORUM_PATTERN_KEY);
            }
        }

        /// <summary>
        /// Deletes a topic
        /// </summary>
        /// <param name="forumTopicId">The topic identifier</param>
        public void DeleteTopic(int forumTopicId)
        {
            var forumTopic = GetTopicById(forumTopicId);
            if (forumTopic == null)
                return;

            var context = ObjectContextHelper.CurrentObjectContext;
            if (!context.IsAttached(forumTopic))
                context.ForumTopics.Attach(forumTopic);
            context.DeleteObject(forumTopic);
            context.SaveChanges();

            if (this.CacheEnabled)
            {
                NopRequestCache.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
                NopRequestCache.RemoveByPattern(FORUM_PATTERN_KEY);
            }
        }

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="forumTopicId">The topic identifier</param>
        /// <returns>Topic</returns>
        public ForumTopic GetTopicById(int forumTopicId)
        {
            return GetTopicById(forumTopicId, false);
        }

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="forumTopicId">The topic identifier</param>
        /// <param name="increaseViews">The value indicating whether to increase topic views</param>
        /// <returns>Topic</returns>
        public ForumTopic GetTopicById(int forumTopicId, bool increaseViews)
        {
            if (forumTopicId == 0)
                return null;

            var context = ObjectContextHelper.CurrentObjectContext;
            var query = from ft in context.ForumTopics
                        where ft.ForumTopicId == forumTopicId
                        select ft;
            var forumTopic = query.SingleOrDefault();
            if (forumTopic == null)
                return null;

            if (increaseViews)
            {
                forumTopic.Views = ++forumTopic.Views;
                UpdateTopic(forumTopic);
            }

            return forumTopic;
        }

        /// <summary>
        /// Gets all topics
        /// </summary>
        /// <param name="forumId">The forum group identifier</param>
        /// <param name="userId">The user identifier</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchType">Search type</param>
        /// <param name="limitDays">Limit by the last number days; 0 to load all topics</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="totalRecords">Total records</param>
        /// <returns>Topics</returns>
        public List<ForumTopic> GetAllTopics(int forumId,
            int userId, string keywords, ForumSearchTypeEnum searchType,
            int limitDays, int pageSize,
            int pageIndex, out int totalRecords)
        {
            if (pageSize <= 0)
                pageSize = 10;
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            if (pageIndex < 0)
                pageIndex = 0;
            if (pageIndex == int.MaxValue)
                pageIndex = int.MaxValue - 1;

            DateTime? limitDate = null;
            if (limitDays > 0)
            {
                limitDate = DateTime.UtcNow.AddDays(-limitDays);
            }

            var context = ObjectContextHelper.CurrentObjectContext;
            ObjectParameter totalRecordsParameter = new ObjectParameter("TotalRecords", typeof(int));
            var forumTopics = context.Sp_Forums_TopicLoadAll(forumId,
                userId, keywords, (int)searchType, limitDate,
                pageIndex, pageSize, totalRecordsParameter).ToList();
            totalRecords = Convert.ToInt32(totalRecordsParameter.Value);
            return forumTopics;
        }
        
        /// <summary>
        /// Gets active topics
        /// </summary>
        /// <param name="forumId">The forum group identifier</param>
        /// <param name="topicCount">Topic count. 0 if you want to get all topics</param>
        /// <returns>Topics</returns>
        public List<ForumTopic> GetActiveTopics(int forumId, int topicCount)
        {
            var context = ObjectContextHelper.CurrentObjectContext;
            var forumTopics = context.Sp_Forums_TopicLoadActive(forumId, topicCount).ToList();
            return forumTopics;
        }
        
        /// <summary>
        /// Inserts a topic
        /// </summary>
        /// <param name="forumTopic">Forum topic</param>
        /// <param name="sendNotifications">A value indicating whether to send notifications to users</param>
        public void InsertTopic(ForumTopic forumTopic, bool sendNotifications)
        {
            if (forumTopic == null)
                throw new ArgumentNullException("forumTopic");

            forumTopic.Subject = CommonHelper.EnsureNotNull(forumTopic.Subject);
            forumTopic.Subject = forumTopic.Subject.Trim();

            if (String.IsNullOrEmpty(forumTopic.Subject))
                throw new NopException("Topic subject cannot be empty");

            if (this.TopicSubjectMaxLength > 0)
            {
                if (forumTopic.Subject.Length > this.TopicSubjectMaxLength)
                    forumTopic.Subject = forumTopic.Subject.Substring(0, this.TopicSubjectMaxLength);
            }

            forumTopic.Subject = CommonHelper.EnsureMaximumLength(forumTopic.Subject, 450);

            var context = ObjectContextHelper.CurrentObjectContext;
            
            context.ForumTopics.AddObject(forumTopic);
            context.SaveChanges();

            if (this.CacheEnabled)
            {
                NopRequestCache.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
                NopRequestCache.RemoveByPattern(FORUM_PATTERN_KEY);
            }

            if (sendNotifications)
            {
                var forum = forumTopic.Forum;
                var subscriptions = GetAllSubscriptions(0, forum.ForumId, 0, int.MaxValue, 0);

                foreach (var subscription in subscriptions)
                {
                    if (subscription.UserId == forumTopic.UserId)
                        continue;

                    IoCFactory.Resolve<IMessageManager>().SendNewForumTopicMessage(subscription.User, 
                        forumTopic, forum, NopContext.Current.WorkingLanguage.LanguageId);
                }
            }
        }

        /// <summary>
        /// Updates the topic
        /// </summary>
        /// <param name="forumTopic">Forum topic</param>
        public void UpdateTopic(ForumTopic forumTopic)
        {
            if (forumTopic == null)
                throw new ArgumentNullException("forumTopic");

            forumTopic.Subject = CommonHelper.EnsureNotNull(forumTopic.Subject);
            forumTopic.Subject = forumTopic.Subject.Trim();

            if (String.IsNullOrEmpty(forumTopic.Subject))
                throw new NopException("Topic subject cannot be empty");

            if (this.TopicSubjectMaxLength > 0)
            {
                if (forumTopic.Subject.Length > this.TopicSubjectMaxLength)
                    forumTopic.Subject = forumTopic.Subject.Substring(0, this.TopicSubjectMaxLength);
            }

            forumTopic.Subject = CommonHelper.EnsureMaximumLength(forumTopic.Subject, 450);
            
            var context = ObjectContextHelper.CurrentObjectContext;
            if (!context.IsAttached(forumTopic))
                context.ForumTopics.Attach(forumTopic);
            
            context.SaveChanges();
            
            if (this.CacheEnabled)
            {
                NopRequestCache.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
                NopRequestCache.RemoveByPattern(FORUM_PATTERN_KEY);
            }
        }

        /// <summary>
        /// Moves the topic
        /// </summary>
        /// <param name="forumTopicId">The forum topic identifier</param>
        /// <param name="newForumId">New forum identifier</param>
        /// <returns>Moved topic</returns>
        public ForumTopic MoveTopic(int forumTopicId, int newForumId)
        {
            var forumTopic = GetTopicById(forumTopicId);
            if (forumTopic == null)
                return forumTopic;

            if (this.IsUserAllowedToMoveTopic(NopContext.Current.User, forumTopic))
            {
                int previousForumId = forumTopic.ForumId;
                var newForum = GetForumById(newForumId);

                if (newForum != null)
                {
                    if (previousForumId != newForumId)
                    {
                        forumTopic.ForumId = newForum.ForumId;
                        forumTopic.UpdatedOn = DateTime.UtcNow;
                        UpdateTopic(forumTopic);

                        //update forum stats
                        UpdateForumStats(previousForumId);
                        UpdateForumStats(newForumId);
                    }
                }
            }
            return forumTopic;
        }

        /// <summary>
        /// Deletes a post
        /// </summary>
        /// <param name="forumPostId">The post identifier</param>
        public void DeletePost(int forumPostId)
        {
            var forumPost = GetPostById(forumPostId);
            if (forumPost == null)
                return;

            int forumTopicId = forumPost.TopicId;
             
            //delete topic if it was the first post
            bool deleteTopic = false;
            var forumTopic = this.GetTopicById(forumTopicId);
            if (forumTopic != null)
            {
                ForumPost firstPost = forumTopic.FirstPost;
                if (firstPost != null && firstPost.ForumPostId == forumPostId)
                {
                    deleteTopic = true;
                }
            }

            var context = ObjectContextHelper.CurrentObjectContext;
            if (!context.IsAttached(forumPost))
                context.ForumPosts.Attach(forumPost);
            context.DeleteObject(forumPost);
            context.SaveChanges();

            if (deleteTopic)
            {
                DeleteTopic(forumTopicId);
            }

            if (this.CacheEnabled)
            {
                NopRequestCache.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
                NopRequestCache.RemoveByPattern(FORUM_PATTERN_KEY);
            }
        }

        /// <summary>
        /// Gets a post
        /// </summary>
        /// <param name="forumPostId">The post identifier</param>
        /// <returns>Post</returns>
        public ForumPost GetPostById(int forumPostId)
        {
            if (forumPostId == 0)
                return null;

            var context = ObjectContextHelper.CurrentObjectContext;
            var query = from fp in context.ForumPosts
                        where fp.ForumPostId == forumPostId
                        select fp;
            var forumPost = query.SingleOrDefault();

            return forumPost;
        }

        /// <summary>
        /// Gets all posts
        /// </summary>
        /// <param name="forumTopicId">The forum topic identifier</param>
        /// <param name="userId">The user identifier</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="totalRecords">Total records</param>
        /// <returns>Posts</returns>
        public List<ForumPost> GetAllPosts(int forumTopicId,
            int userId, string keywords, int pageSize, int pageIndex, out int totalRecords)
        {
            return GetAllPosts(forumTopicId, userId, keywords, true,
                pageSize, pageIndex, out totalRecords);
        }

        /// <summary>
        /// Gets all posts
        /// </summary>
        /// <param name="forumTopicId">The forum topic identifier</param>
        /// <param name="userId">The user identifier</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="ascSort">Sort order</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="totalRecords">Total records</param>
        /// <returns>Posts</returns>
        public List<ForumPost> GetAllPosts(int forumTopicId, int userId,
            string keywords, bool ascSort, int pageSize, int pageIndex, out int totalRecords)
        {
            if (pageSize <= 0)
                pageSize = 10;
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            if (pageIndex < 0)
                pageIndex = 0;
            if (pageIndex == int.MaxValue)
                pageIndex = int.MaxValue - 1;

            var context = ObjectContextHelper.CurrentObjectContext;
            ObjectParameter totalRecordsParameter = new ObjectParameter("TotalRecords", typeof(int));
            var forumPosts = context.Sp_Forums_PostLoadAll(forumTopicId,
                userId, keywords, ascSort, pageIndex, pageSize, totalRecordsParameter).ToList();
            totalRecords = Convert.ToInt32(totalRecordsParameter.Value);
            return forumPosts;
        }

        /// <summary>
        /// Inserts a post
        /// </summary>
        /// <param name="forumPost">The forum post</param>
        /// <param name="sendNotifications">A value indicating whether to send notifications to users</param>
        public void InsertPost(ForumPost forumPost, bool sendNotifications)
        {
            if (forumPost == null)
                throw new ArgumentNullException("forumPost");

            forumPost.Text = CommonHelper.EnsureNotNull(forumPost.Text);
            forumPost.Text = forumPost.Text.Trim();

            if (String.IsNullOrEmpty(forumPost.Text))
                throw new NopException("Text cannot be empty");

            if (this.PostMaxLength > 0)
            {
                if (forumPost.Text.Length > this.PostMaxLength)
                    forumPost.Text = forumPost.Text.Substring(0, this.PostMaxLength);
            }

            forumPost.IPAddress = CommonHelper.EnsureNotNull(forumPost.IPAddress);
            forumPost.IPAddress = CommonHelper.EnsureMaximumLength(forumPost.IPAddress, 100);

            var context = ObjectContextHelper.CurrentObjectContext;

            context.ForumPosts.AddObject(forumPost);
            context.SaveChanges();

            if (this.CacheEnabled)
            {
                NopRequestCache.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
                NopRequestCache.RemoveByPattern(FORUM_PATTERN_KEY);
            }

            if (sendNotifications)
            {
                var forumTopic = forumPost.Topic;
                var forum = forumTopic.Forum;
                var subscriptions = GetAllSubscriptions(0, 0, 
                    forumTopic.ForumTopicId, int.MaxValue, 0);
                
                foreach (ForumSubscription subscription in subscriptions)
                {
                    if (subscription.UserId == forumPost.UserId)
                        continue;

                    IoCFactory.Resolve<IMessageManager>().SendNewForumPostMessage(subscription.User,
                        forumPost, forumTopic, forum, 
                        NopContext.Current.WorkingLanguage.LanguageId);
                }
            }
        }

        /// <summary>
        /// Updates the post
        /// </summary>
        /// <param name="forumPost">The forum post</param>
        public void UpdatePost(ForumPost forumPost)
        {
            if (forumPost == null)
                throw new ArgumentNullException("forumPost");
            
            forumPost.Text = CommonHelper.EnsureNotNull(forumPost.Text);
            forumPost.Text = forumPost.Text.Trim();

            if (String.IsNullOrEmpty(forumPost.Text))
                throw new NopException("Text cannot be empty");

            if (this.PostMaxLength > 0)
            {
                if (forumPost.Text.Length > this.PostMaxLength)
                    forumPost.Text = forumPost.Text.Substring(0, this.PostMaxLength);
            }

            forumPost.IPAddress = CommonHelper.EnsureNotNull(forumPost.IPAddress);
            forumPost.IPAddress = CommonHelper.EnsureMaximumLength(forumPost.IPAddress, 100);            

            var context = ObjectContextHelper.CurrentObjectContext;
            if (!context.IsAttached(forumPost))
                context.ForumPosts.Attach(forumPost);

            context.SaveChanges();

            if (this.CacheEnabled)
            {
                NopRequestCache.RemoveByPattern(FORUMGROUP_PATTERN_KEY);
                NopRequestCache.RemoveByPattern(FORUM_PATTERN_KEY);
            }
        }

        /// <summary>
        /// Deletes a private message
        /// </summary>
        /// <param name="forumPrivateMessageId">The private message identifier</param>
        public void DeletePrivateMessage(int forumPrivateMessageId)
        {
            var privateMessage = GetPrivateMessageById(forumPrivateMessageId);
            if (privateMessage == null)
                return;

            var context = ObjectContextHelper.CurrentObjectContext;
            if (!context.IsAttached(privateMessage))
                context.PrivateMessages.Attach(privateMessage);
            context.DeleteObject(privateMessage);
            context.SaveChanges();
        }

        /// <summary>
        /// Gets a private message
        /// </summary>
        /// <param name="forumPrivateMessageId">The private message identifier</param>
        /// <returns>Private message</returns>
        public PrivateMessage GetPrivateMessageById(int forumPrivateMessageId)
        {
            if (forumPrivateMessageId == 0)
                return null;

            var context = ObjectContextHelper.CurrentObjectContext;
            var query = from pm in context.PrivateMessages
                        where pm.PrivateMessageId == forumPrivateMessageId
                        select pm;
            var privateMessage = query.SingleOrDefault();

            return privateMessage;
        }

        /// <summary>
        /// Gets private messages
        /// </summary>
        /// <param name="fromUserId">The user identifier who sent the message</param>
        /// <param name="toUserId">The user identifier who should receive the message</param>
        /// <param name="isRead">A value indicating whether loaded messages are read. false - to load not read messages only, 1 to load read messages only, null to load all messages</param>
        /// <param name="isDeletedByAuthor">A value indicating whether loaded messages are deleted by author. false - messages are not deleted by author, null to load all messages</param>
        /// <param name="isDeletedByRecipient">A value indicating whether loaded messages are deleted by recipient. false - messages are not deleted by recipient, null to load all messages</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Private messages</returns>
        public PagedList<PrivateMessage> GetAllPrivateMessages(int fromUserId,
            int toUserId, bool? isRead, bool? isDeletedByAuthor, bool? isDeletedByRecipient,
            string keywords, int pageIndex, int pageSize)
        {
            if (pageIndex < 0)
                pageIndex = 0;
            if (pageIndex == int.MaxValue)
                pageIndex = int.MaxValue - 1;

            if (pageSize <= 0)
                pageSize = 10;
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var context = ObjectContextHelper.CurrentObjectContext;
            var query = from pm in context.PrivateMessages
                        where
                        (fromUserId == 0 || fromUserId == pm.FromUserId) &&
                        (toUserId == 0 || toUserId == pm.ToUserId) &&
                        (!isRead.HasValue || isRead.Value == pm.IsRead) &&
                        (!isDeletedByAuthor.HasValue || isDeletedByAuthor.Value == pm.IsDeletedByAuthor) &&
                        (!isDeletedByRecipient.HasValue || isDeletedByRecipient.Value == pm.IsDeletedByRecipient) && 
                        (String.IsNullOrEmpty(keywords) || pm.Subject.Contains(keywords)) &&
                        (String.IsNullOrEmpty(keywords) || pm.Text.Contains(keywords))
                        orderby pm.CreatedOn descending
                        select pm;
            var privateMessages = new PagedList<PrivateMessage>(query, pageIndex, pageSize);
            
            return privateMessages;
        }

        /// <summary>
        /// Inserts a private message
        /// </summary>
        /// <param name="privateMessage">Private message</param>
        public void InsertPrivateMessage(PrivateMessage privateMessage)
        {
            if (privateMessage == null)
                throw new ArgumentNullException("privateMessage");
            
            privateMessage.Subject = CommonHelper.EnsureNotNull(privateMessage.Subject);
            privateMessage.Subject = privateMessage.Subject.Trim();
            if (String.IsNullOrEmpty(privateMessage.Subject))
                throw new NopException("Subject cannot be empty");
            if (this.PMSubjectMaxLength > 0)
            {
                if (privateMessage.Subject.Length > this.PMSubjectMaxLength)
                    privateMessage.Subject = privateMessage.Subject.Substring(0, this.PMSubjectMaxLength);
            }
            
            privateMessage.Text = CommonHelper.EnsureNotNull(privateMessage.Text);
            privateMessage.Text = privateMessage.Text.Trim();
            if (String.IsNullOrEmpty(privateMessage.Text))
                throw new NopException("Text cannot be empty");

            privateMessage.Subject = CommonHelper.EnsureMaximumLength(privateMessage.Subject, 450);

            if (this.PMTextMaxLength > 0)
            {
                if (privateMessage.Text.Length > this.PMTextMaxLength)
                    privateMessage.Text = privateMessage.Text.Substring(0, this.PMTextMaxLength);
            }

            Customer customerTo = IoCFactory.Resolve<ICustomerManager>().GetCustomerById(privateMessage.ToUserId);
            if (customerTo == null)
                throw new NopException("Recipient could not be loaded");

            var context = ObjectContextHelper.CurrentObjectContext;

            context.PrivateMessages.AddObject(privateMessage);
            context.SaveChanges();
            
            //UI notification
            customerTo.NotifiedAboutNewPrivateMessages = false;
            //Email notification
            if (this.NotifyAboutPrivateMessages)
            {
                IoCFactory.Resolve<IMessageManager>().SendPrivateMessageNotification(privateMessage, NopContext.Current.WorkingLanguage.LanguageId);
            }
        }

        /// <summary>
        /// Updates the private message
        /// </summary>
        /// <param name="privateMessage">Private message</param>
        public void UpdatePrivateMessage(PrivateMessage privateMessage)
        {
            if (privateMessage == null)
                throw new ArgumentNullException("privateMessage");

            privateMessage.Subject = CommonHelper.EnsureNotNull(privateMessage.Subject);
            privateMessage.Subject = privateMessage.Subject.Trim();
            if (String.IsNullOrEmpty(privateMessage.Subject))
                throw new NopException("Subject cannot be empty");
            if (this.PMSubjectMaxLength > 0)
            {
                if (privateMessage.Subject.Length > this.PMSubjectMaxLength)
                    privateMessage.Subject = privateMessage.Subject.Substring(0, this.PMSubjectMaxLength);
            }

            privateMessage.Text = CommonHelper.EnsureNotNull(privateMessage.Text);
            privateMessage.Text = privateMessage.Text.Trim();
            if (String.IsNullOrEmpty(privateMessage.Text))
                throw new NopException("Text cannot be empty");
            if (this.PMTextMaxLength > 0)
            {
                if (privateMessage.Text.Length > this.PMTextMaxLength)
                    privateMessage.Text = privateMessage.Text.Substring(0, this.PMTextMaxLength);
            }

            privateMessage.Subject = CommonHelper.EnsureMaximumLength(privateMessage.Subject, 450);

            if (privateMessage.IsDeletedByAuthor && privateMessage.IsDeletedByRecipient)
            {
                DeletePrivateMessage(privateMessage.PrivateMessageId);
            }
            else
            {
                var context = ObjectContextHelper.CurrentObjectContext;
                if (!context.IsAttached(privateMessage))
                    context.PrivateMessages.Attach(privateMessage);

                context.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes a forum subscription
        /// </summary>
        /// <param name="forumSubscriptionId">The forum subscription identifier</param>
        public void DeleteSubscription(int forumSubscriptionId)
        {
            var forumSubscription = GetSubscriptionById(forumSubscriptionId);
            if (forumSubscription == null)
                return;

            var context = ObjectContextHelper.CurrentObjectContext;
            if (!context.IsAttached(forumSubscription))
                context.ForumSubscriptions.Attach(forumSubscription);
            context.DeleteObject(forumSubscription);
            context.SaveChanges();
        }

        /// <summary>
        /// Gets a forum subscription
        /// </summary>
        /// <param name="forumSubscriptionId">The forum subscription identifier</param>
        /// <returns>Forum subscription</returns>
        public ForumSubscription GetSubscriptionById(int forumSubscriptionId)
        {
            if (forumSubscriptionId == 0)
                return null;

            var context = ObjectContextHelper.CurrentObjectContext;
            var query = from fs in context.ForumSubscriptions
                        where fs.ForumSubscriptionId == forumSubscriptionId
                        select fs;
            var forumSubscription = query.SingleOrDefault();

            return forumSubscription;
        }

        /// <summary>
        /// Gets forum subscriptions
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <param name="forumId">The forum identifier</param>
        /// <param name="topicId">The topic identifier</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="pageIndex">Page index</param>
        /// <returns>Forum subscriptions</returns>
        public List<ForumSubscription> GetAllSubscriptions(int userId, 
            int forumId, int topicId, int pageSize, int pageIndex)
        {
            int totalRecords = 0;
            return GetAllSubscriptions(userId, forumId, topicId, pageSize, 
                pageIndex, out totalRecords);
        }

        /// <summary>
        /// Gets forum subscriptions
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <param name="forumId">The forum identifier</param>
        /// <param name="topicId">The topic identifier</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="totalRecords">Total records</param>
        /// <returns>Forum subscriptions</returns>
        public List<ForumSubscription> GetAllSubscriptions(int userId, int forumId,
            int topicId, int pageSize, int pageIndex, out int totalRecords)
        {
            if (pageSize <= 0)
                pageSize = 10;
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            if (pageIndex < 0)
                pageIndex = 0;
            if (pageIndex == int.MaxValue)
                pageIndex = int.MaxValue - 1;

            var context = ObjectContextHelper.CurrentObjectContext;
            ObjectParameter totalRecordsParameter = new ObjectParameter("TotalRecords", typeof(int));
            var forumSubscriptions = context.Sp_Forums_SubscriptionLoadAll(userId,
                forumId, topicId, pageIndex, pageSize, totalRecordsParameter).ToList();
            totalRecords = Convert.ToInt32(totalRecordsParameter.Value);

            return forumSubscriptions;
        }

        /// <summary>
        /// Inserts a forum subscription
        /// </summary>
        /// <param name="forumSubscription">Forum subscription</param>
        public void InsertSubscription(ForumSubscription forumSubscription)
        {
            if (forumSubscription == null)
                throw new ArgumentNullException("forumSubscription");

            var context = ObjectContextHelper.CurrentObjectContext;

            context.ForumSubscriptions.AddObject(forumSubscription);
            context.SaveChanges();
        }

        /// <summary>
        /// Updates the forum subscription
        /// </summary>
        /// <param name="forumSubscription">Forum subscription</param>
        public void UpdateSubscription(ForumSubscription forumSubscription)
        {
            if (forumSubscription == null)
                throw new ArgumentNullException("forumSubscription");

            var context = ObjectContextHelper.CurrentObjectContext;
            if (!context.IsAttached(forumSubscription))
                context.ForumSubscriptions.Attach(forumSubscription);

            context.SaveChanges();
        }

        /// <summary>
        /// Check whether user is allowed to create new topics
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="forum">Forum</param>
        /// <returns>True if allowed, otherwise false</returns>
        public bool IsUserAllowedToCreateTopic(Customer customer, Forum forum)
        {
            if (forum == null)
                return false;

            if (customer == null)
                return false;

            if(customer.IsGuest && !AllowGuestsToCreateTopics)
                return false;

            if (customer.IsForumModerator)
                return true;

            return true;
        }

        /// <summary>
        /// Check whether user is allowed to edit topic
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="topic">Topic</param>
        /// <returns>True if allowed, otherwise false</returns>
        public bool IsUserAllowedToEditTopic(Customer customer, ForumTopic topic)
        {
            if (topic == null)
                return false;

            if (customer == null)
                return false;

            if (customer.IsGuest)
                return false;

            if (customer.IsForumModerator)
                return true;

            if (this.AllowCustomersToEditPosts)
            {
                bool ownTopic = customer.CustomerId == topic.UserId;
                return ownTopic;
            }

            return false;
        }

        /// <summary>
        /// Check whether user is allowed to move topic
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="topic">Topic</param>
        /// <returns>True if allowed, otherwise false</returns>
        public bool IsUserAllowedToMoveTopic(Customer customer, ForumTopic topic)
        {
            if (topic == null)
                return false;

            if (customer == null)
                return false;

            if (customer.IsGuest)
                return false;

            if (customer.IsForumModerator)
                return true;

            return false;
        }

        /// <summary>
        /// Check whether user is allowed to delete topic
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="topic">Topic</param>
        /// <returns>True if allowed, otherwise false</returns>
        public bool IsUserAllowedToDeleteTopic(Customer customer, ForumTopic topic)
        {
            if (topic == null)
                return false;

            if (customer == null)
                return false;

            if (customer.IsGuest)
                return false;

            if (customer.IsForumModerator)
                return true;

            if (this.AllowCustomersToDeletePosts)
            {
                bool ownTopic = customer.CustomerId == topic.UserId;
                return ownTopic;
            }

            return false;
        }

        /// <summary>
        /// Check whether user is allowed to create new post
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="topic">Topic</param>
        /// <returns>True if allowed, otherwise false</returns>
        public bool IsUserAllowedToCreatePost(Customer customer, ForumTopic topic)
        {
            if (topic == null)
                return false;

            if (customer == null)
                return false;

            if(customer.IsGuest && !AllowGuestsToCreatePosts)
                return false;

            if (customer.IsForumModerator)
                return true;

            return true;
        }

        /// <summary>
        /// Check whether user is allowed to edit post
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="post">Topic</param>
        /// <returns>True if allowed, otherwise false</returns>
        public bool IsUserAllowedToEditPost(Customer customer, ForumPost post)
        {
            if (post == null)
                return false;

            if (customer == null)
                return false;

            if (customer.IsGuest)
                return false;

            if (customer.IsForumModerator)
                return true;

            if (this.AllowCustomersToEditPosts)
            {
                bool ownPost = customer.CustomerId == post.UserId;
                return ownPost;
            }

            return false;
        }

        /// <summary>
        /// Check whether user is allowed to delete post
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="post">Topic</param>
        /// <returns>True if allowed, otherwise false</returns>
        public bool IsUserAllowedToDeletePost(Customer customer, ForumPost post)
        {
            if (post == null)
                return false;

            if (customer == null)
                return false;

            if (customer.IsGuest)
                return false;

            if (customer.IsForumModerator)
                return true;

            if (this.AllowCustomersToDeletePosts)
            {
                bool ownPost = customer.CustomerId == post.UserId;
                return ownPost;
            }

            return false;
        }

        /// <summary>
        /// Check whether user is allowed to set topic priority
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>True if allowed, otherwise false</returns>
        public bool IsUserAllowedToSetTopicPriority(Customer customer)
        {
            if (customer == null)
                return false;

            if (customer.IsGuest)
                return false;

            if (customer.IsForumModerator)
                return true;

            return false;
        }

        /// <summary>
        /// Check whether user is allowed to watch topics
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>True if allowed, otherwise false</returns>
        public bool IsUserAllowedToSubscribe(int customerId)
        {
            var customer = IoCFactory.Resolve<ICustomerManager>().GetCustomerById(customerId);
            return IsUserAllowedToSubscribe(customer);
        }

        /// <summary>
        /// Check whether user is allowed to watch topics
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>True if allowed, otherwise false</returns>
        public bool IsUserAllowedToSubscribe(Customer customer)
        {
            if (customer == null)
                return false;

            if (customer.IsGuest)
                return false;

            return true;
        }

        /// <summary>
        /// Formats the posts text
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>Formatted text</returns>
        public string FormatPostText(string text)
        {
            if (String.IsNullOrEmpty(text))
                return string.Empty;

            switch (this.ForumEditor)
            {
                case EditorTypeEnum.SimpleTextBox:
                    {
                        text = HtmlHelper.FormatText(text, false, true, false, false, false, false);
                    }
                    break;
                case EditorTypeEnum.BBCodeEditor:
                    {
                        text = HtmlHelper.FormatText(text, false, true, false, true, false, false);
                    }
                    break;
                case EditorTypeEnum.HtmlEditor:
                    {
                        text = HtmlHelper.FormatText(text, false, false, true, false, false, false);
                    }
                    break;
                default:
                    break;
            }

            return text;
        }

        /// <summary>
        /// Formats the signature text
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>Formatted text</returns>
        public string FormatSignatureText(string text)
        {
            if (String.IsNullOrEmpty(text))
                return string.Empty;

            text = HtmlHelper.FormatText(text, false, true, false, false, false, false);
            return text;
        }

        /// <summary>
        /// Formats the private message text
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>Formatted text</returns>
        public string FormatPrivateMessageText(string text)
        {
            if (String.IsNullOrEmpty(text))
                return string.Empty;

            text = HtmlHelper.FormatText(text, false, true, false, true, false, false);

            return text;
        }

        /// <summary>
        /// Strips topic subject
        /// </summary>
        /// <param name="subject">Subject</param>
        /// <returns>Formatted subject</returns>
        public string StripTopicSubject(string subject)
        {
            if (String.IsNullOrEmpty(subject))
                return subject;
            int strippedTopicMaxLength = IoCFactory.Resolve<ISettingManager>().GetSettingValueInteger("Forums.StrippedTopicMaxLength", 45);
            if (strippedTopicMaxLength > 0)
            {
                if (subject.Length > strippedTopicMaxLength)
                {
                    int index = subject.IndexOf(" ", strippedTopicMaxLength);
                    if (index > 0)
                    {
                        subject = subject.Substring(0, index);
                        subject += "...";
                    }
                }
            }

            return subject;
        }

        /// <summary>
        /// Calculates topic page index by post identifier
        /// </summary>
        /// <param name="forumTopicId">Topic identifier</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="postId">Post identifier</param>
        /// <returns>Page index</returns>
        public int CalculateTopicPageIndex(int forumTopicId, int pageSize, int postId)
        {
            int pageIndex = 0;
            int totalRecords = 0;
            var forumPosts = GetAllPosts(forumTopicId, 0, 
                string.Empty, true, int.MaxValue, 0, out totalRecords);

            for (int i = 0; i < totalRecords; i++)
            {
                if (forumPosts[i].ForumPostId == postId)
                {
                    if (pageSize > 0)
                    {
                        pageIndex = i/pageSize;
                    }
                }
            }

            return pageIndex;
        }

        #endregion
        
        #region Properties

        /// <summary>
        /// Gets a value indicating whether cache is enabled
        /// </summary>
        public bool CacheEnabled
        {
            get
            {
                return IoCFactory.Resolve<ISettingManager>().GetSettingValueBoolean("Cache.ForumManager.CacheEnabled");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether forums are enabled
        /// </summary>
        public bool ForumsEnabled
        {
            get
            {
                bool result = IoCFactory.Resolve<ISettingManager>().GetSettingValueBoolean("Forums.ForumsEnabled");
                return result;
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Forums.ForumsEnabled", value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether relative date and time formatting is enabled  (e.g. 2 hours ago, a month ago)
        /// </summary>
        public bool RelativeDateTimeFormattingEnabled
        {
            get
            {
                bool result = IoCFactory.Resolve<ISettingManager>().GetSettingValueBoolean("Forums.RelativeDateTimeFormattingEnabled", false);
                return result;
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Forums.RelativeDateTimeFormattingEnabled", value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to edit posts that they created.
        /// </summary>
        public bool AllowCustomersToEditPosts
        {
            get
            {
                bool result = IoCFactory.Resolve<ISettingManager>().GetSettingValueBoolean("Forums.CustomersAllowedToEditPosts");
                return result;
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Forums.CustomersAllowedToEditPosts", value.ToString());
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to manage theirs subscriptions
        /// </summary>
        public bool AllowCustomersToManageSubscriptions
        {
            get
            {
                return IoCFactory.Resolve<ISettingManager>().GetSettingValueBoolean("Forums.CustomersAllowedToManageSubscriptions");
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Forums.CustomersAllowedToManageSubscriptions", value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the guests are allowed to create posts.
        /// </summary>
        public bool AllowGuestsToCreatePosts
        {
            get
            {
                return IoCFactory.Resolve<ISettingManager>().GetSettingValueBoolean("Forums.GuestsAllowedToCreatePosts");
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Forums.GuestsAllowedToCreatePosts", value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the guests are allowed to create topics.
        /// </summary>
        public bool AllowGuestsToCreateTopics
        {
            get
            {
                return IoCFactory.Resolve<ISettingManager>().GetSettingValueBoolean("Forums.GuestsAllowedToCreateTopics");
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Forums.GuestsAllowedToCreateTopics", value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to delete posts that they created.
        /// </summary>
        public bool AllowCustomersToDeletePosts
        {
            get
            {
                bool result = IoCFactory.Resolve<ISettingManager>().GetSettingValueBoolean("Forums.CustomersAllowedToDeletePosts");
                return result;
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Forums.CustomersAllowedToDeletePosts", value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets maximum length of topic subject
        /// </summary>
        public int TopicSubjectMaxLength
        {
            get
            {
                int result = IoCFactory.Resolve<ISettingManager>().GetSettingValueInteger("Forums.TopicSubjectMaxLength");
                return result;
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Forums.TopicSubjectMaxLength", value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets maximum length of post
        /// </summary>
        public int PostMaxLength
        {
            get
            {
                int result = IoCFactory.Resolve<ISettingManager>().GetSettingValueInteger("Forums.PostMaxLength");
                return result;
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Forums.PostMaxLength", value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the page size for topics in forums
        /// </summary>
        public int TopicsPageSize
        {
            get
            {
                int result = IoCFactory.Resolve<ISettingManager>().GetSettingValueInteger("Forums.TopicsPageSize");
                return result;
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Forums.TopicsPageSize", value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the page size for posts in topics
        /// </summary>
        public int PostsPageSize
        {
            get
            {
                int result = IoCFactory.Resolve<ISettingManager>().GetSettingValueInteger("Forums.PostsPageSize");
                return result;
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Forums.PostsPageSize", value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the page size for search result
        /// </summary>
        public int SearchResultsPageSize
        {
            get
            {
                int result = IoCFactory.Resolve<ISettingManager>().GetSettingValueInteger("Forums.SearchResultsPageSize");
                return result;
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Forums.SearchResultsPageSize", value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the page size for latest user post
        /// </summary>
        public int LatestUserPostsPageSize
        {
            get
            {
                int result = IoCFactory.Resolve<ISettingManager>().GetSettingValueInteger("Forums.LatestUserPostsPageSize");
                return result;
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Forums.LatestUserPostsPageSize", value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show customers forum post count
        /// </summary>
        public bool ShowCustomersPostCount
        {
            get
            {
                bool result = IoCFactory.Resolve<ISettingManager>().GetSettingValueBoolean("Forums.ShowCustomersPostCount");
                return result;
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Forums.ShowCustomersPostCount", value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets a forum editor type
        /// </summary>
        public EditorTypeEnum ForumEditor
        {
            get
            {
                int forumEditorTypeId = IoCFactory.Resolve<ISettingManager>().GetSettingValueInteger("Forums.EditorType");
                return (EditorTypeEnum)Enum.ToObject(typeof(EditorTypeEnum), forumEditorTypeId);
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Forums.EditorType", ((int)value).ToString());
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to specify signature.
        /// </summary>
        public bool SignaturesEnabled
        {
            get
            {
                bool result = IoCFactory.Resolve<ISettingManager>().GetSettingValueBoolean("Forums.SignatureEnabled");
                return result;
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Forums.SignatureEnabled", value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether private messages are allowed
        /// </summary>
        public bool AllowPrivateMessages
        {
            get
            {
                bool result = IoCFactory.Resolve<ISettingManager>().GetSettingValueBoolean("Messaging.AllowPM");
                return result;
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Messaging.AllowPM", value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a customer should be notified about new private messages
        /// </summary>
        public bool NotifyAboutPrivateMessages
        {
            get
            {
                bool result = IoCFactory.Resolve<ISettingManager>().GetSettingValueBoolean("Messaging.NotifyAboutPrivateMessages");
                return result;
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Messaging.NotifyAboutPrivateMessages", value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets maximum length of pm subject
        /// </summary>
        public int PMSubjectMaxLength
        {
            get
            {
                int result = IoCFactory.Resolve<ISettingManager>().GetSettingValueInteger("Messaging.PMSubjectMaxLength");
                return result;
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Messaging.PMSubjectMaxLength", value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets maximum length of pm message
        /// </summary>
        public int PMTextMaxLength
        {
            get
            {
                int result = IoCFactory.Resolve<ISettingManager>().GetSettingValueInteger("Messaging.PMTextMaxLength");
                return result;
            }
            set
            {
                IoCFactory.Resolve<ISettingManager>().SetParam("Messaging.PMTextMaxLength", value.ToString());
            }
        }

        #endregion
    }
}
