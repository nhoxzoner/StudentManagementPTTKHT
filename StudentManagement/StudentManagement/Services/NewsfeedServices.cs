﻿using StudentManagement.Models;
using StudentManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Migrations;
using StudentManagement.Objects;
using System.Data.Entity;
using System.Collections.ObjectModel;

namespace StudentManagement.Services
{
    public class NewsfeedServices
    {
        private static NewsfeedServices s_instance;

        public static NewsfeedServices Instance => s_instance ?? (s_instance = new NewsfeedServices());

        public Func<StudentManagementEntities> db = () => DataProvider.Instance.Database;

        #region Convert

        public Notification ConvertPostNewsfeedToNotification(NewsfeedPost post)
        {
            return new Notification()
            {
                Id = post.PostId,
                Topic = post.Topic,
                Content = post.PostText,
                Time = post.PostTime,
                IdPoster = post.IdPoster,
                IdSubjectClass = post.IdSubjectClass
            };
        }

        public NewsfeedPost ConvertNotificationToPostNewsfeed(Notification notif)
        {
            var poster = UserServices.Instance.GetUserById((Guid)notif.IdPoster);

            return new NewsfeedPost()
            {
                PostId = notif.Id,
                PostText = notif.Content,
                PostTime = notif.Time,
                IdSubjectClass = notif.IdSubjectClass,
                IdPoster = poster.Id,
                PosterName = poster.DisplayName,
                Topic = notif.Topic,
                PosterAvatar = poster.DatabaseImageTable == null ? null : poster.DatabaseImageTable.Image
            };
        }

        public NotificationComment ConvertPostCommentToNotificationComment(PostComment comment)
        {
            return new NotificationComment()
            {
                Id = comment.Id,
                Time = comment.Time,
                Content = comment.Comment,
                IdUserComment = comment.UserId,
                IdNotification = comment.PostId
            };
        }
        public Notification ConvertPostCommentToNotification(PostComment comment)
        {
            NotificationComment notificationComment = db().NotificationComments.FirstOrDefault(noticeComment=>noticeComment.Id == comment.Id);
            return new Notification()
            {
                Id = comment.Id,
                IdPoster = comment.UserId,
                Time = comment.Time,
                IdNotificationType = NotificationTypeServices.Instance.GetNotificationTypeWithTypeContent("Thông báo bình luận")?.Id,
                Topic = notificationComment.Notification.SubjectClass.Code,
                Content = comment.Comment,
                IdSubjectClass = notificationComment.Notification.IdSubjectClass,
            };
        }
        public NotificationInfo ConvertPostCommentToNotificationInfo(PostComment comment)
        {
            Notification notification = NotificationServices.Instance.FindNotificationByNotificationId(comment.Id);
            NotificationComment notificationComment = db().NotificationComments.FirstOrDefault(noticeComment=> noticeComment.Id == comment.Id);
            return new NotificationInfo()
            {
                Id = Guid.NewGuid(),
                IdNotification = notification.Id,
                IdUserReceiver = notificationComment.Notification.IdPoster,
                IsRead = false,
            };
        }

        public PostComment ConvertNotificationCommentToPostComment(NotificationComment comment)
        {
            User user = UserServices.Instance.GetUserById((Guid)comment.IdUserComment);
            return new PostComment(comment.Id, (Guid)comment.IdNotification, (Guid)comment.IdUserComment, user.DatabaseImageTable.Image, user.DisplayName, comment.Content, comment.Time);
        }

        #endregion Convert

        #region Create

        public async Task SavePostToDatabaseAsync(NewsfeedPost post)
        {
            db().Notifications.AddOrUpdate(ConvertPostNewsfeedToNotification(post));
            await db().SaveChangesAsync();
        }

        public async Task SavePostToNotificationInfoAsync(NewsfeedPost post)
        {
            var notification = NotificationServices.Instance.FindNotificationByNotificationId(post.PostId);
            var listCourseRegister = notification.SubjectClass.CourseRegisters.ToList();
            foreach (var courseRegister in listCourseRegister)
            {
                //not sent to the poster if poster is student
                if (LoginServices.CurrentUser.UserRole.Role == "Sinh viên")
                {
                    if (StudentServices.Instance.FindStudentByUserId(LoginServices.CurrentUser.Id).Id == courseRegister.IdStudent)
                        continue;
                }
                var notificationInfo = new NotificationInfo()
                {
                    Id = Guid.NewGuid(),
                    IdNotification = notification.Id,
                    IdUserReceiver = courseRegister.Student.IdUsers,
                    IsRead = false,
                };
                db().NotificationInfoes.AddOrUpdate(notificationInfo);
            }
            await db().SaveChangesAsync();

        }

        public async Task SaveCommentToDatabaseAsync(PostComment comment)
        {
            db().NotificationComments.AddOrUpdate(ConvertPostCommentToNotificationComment(comment));
            await db().SaveChangesAsync();
        }

        public async Task SaveCommentToNotification(PostComment comment)
        {
            db().Notifications.AddOrUpdate(ConvertPostCommentToNotification(comment));
            await db().SaveChangesAsync();
        }
        public async Task SaveCommentToNotificationInfo(PostComment comment)
        {
            db().NotificationInfoes.AddOrUpdate(ConvertPostCommentToNotificationInfo(comment));
            await db().SaveChangesAsync();
        }

        public async Task SaveImageToDatabaseAsync(Guid postId, string image)
        {
            var imgId = await DatabaseImageTableServices.Instance.SaveImageToDatabaseAsync(image);
            var postImage = new NotificationImage()
            {
                Id = Guid.NewGuid(),
                IdNotification = postId,
                IdDatabaseImageTable = imgId
            };

            db().NotificationImages.AddOrUpdate(postImage);
            await db().SaveChangesAsync();
        }

        #endregion Create

        #region Read

        public List<Notification> GetListNotificationOfSubjectClass(Guid? idSubjectClass)
        {
            return db().Notifications.Where(notif => notif.IdSubjectClass == idSubjectClass && notif.IdNotificationType==null).ToList();
        }

        public List<NotificationComment> GetListCommentInPost(Guid postId)
        {
            return db().NotificationComments.Where(cmt => cmt.IdNotification == postId).ToList();
        }

        public List<string> GetListImagesInPost(Guid postId)
        {
            return db().NotificationImages.Where(notifImg => notifImg.IdNotification == postId).Select(img => img.DatabaseImageTable.Image).ToList();
        }

        #endregion

        #region Delete

        public async Task<int> DeletePostAsync(Guid? id)
        {
            var notification = db().Notifications.FirstOrDefault(notif => notif.Id == id);
            db().Notifications.Remove(notification);

            // Remove comment with post
            var comments = db().NotificationComments.Where(cmt => cmt.IdNotification == id);
            db().NotificationComments.RemoveRange(comments);

            return await db().SaveChangesAsync();
        }

        public async Task<int> DeleteCommentAsync(Guid? id)
        {
            var comment = db().NotificationComments.FirstOrDefault(cmt => cmt.Id == id);
            db().NotificationComments.Remove(comment);

            return await db().SaveChangesAsync();
        }

        public async Task<int> DeleteImagesInPostAsync(Guid? id, List<string> listImages)
        {
            var images = db().NotificationImages.Where(img => img.IdNotification == id && listImages.Contains(img.DatabaseImageTable.Image));
            db().NotificationImages.RemoveRange(images);

            return await db().SaveChangesAsync();
        }

        #endregion
    }
}
