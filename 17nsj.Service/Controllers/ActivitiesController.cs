﻿//----------------------------------------------------------------------
// <copyright file="ActivitiesController.cs" company="17NSJ PR Dept">
// Copyright (c) 17NSJ PR Dept. All rights reserved.
// </copyright>
// <summary>ActivitiesControllerクラス</summary>
//----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.OData.Query;
using _17nsj.DataAccess;

namespace _17nsj.Service.Controllers
{
    /// <summary>
    /// ActivitiesControllerクラス
    /// </summary>
    /// <summary>
    /// ActivitiesCategoriesControllerクラス
    /// </summary>
    [Authorize]
    [RoutePrefix("api/activities")]
    public class ActivitiesController : ControllerBase
    {
        /// <summary>
        /// 全てのアクティビティ情報を取得します。
        /// </summary>
        /// <param name="options">odataoption</param>
        /// <returns>HTTPレスポンス</returns>
        [HttpGet]
        [Route("")]
        public HttpResponseMessage Get(ODataQueryOptions<Activities> options)
        {
            if (!this.CanRead())
            {
                return this.Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            using (Entities entitiies = new Entities())
            {
                var act = entitiies.Activities.Where(e => e.IsAvailable == true).OrderByDescending(e => e.CreatedAt).ToList();
                var filterAct = options.ApplyTo(act.AsQueryable()) as IQueryable<Activities>;

                if (filterAct != null)
                {
                    act = filterAct.ToList();
                    return this.Request.CreateResponse(HttpStatusCode.OK, act);
                }
                else
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Not found");
                }
            }
        }

        /// <summary>
        /// カテゴリを指定してアクティビティ情報を取得します。
        /// </summary>
        /// <param name="category">category</param>
        /// <param name="options">odataoption</param>
        /// <returns>HTTPレスポンス</returns>
        [HttpGet]
        [Route("{category}")]
        public HttpResponseMessage Get(string category, ODataQueryOptions<Activities> options)
        {
            if (!this.CanRead())
            {
                return this.Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            using (Entities entitiies = new Entities())
            {
                var act = entitiies.Activities.Where(e => e.Category == category && e.IsAvailable == true).OrderByDescending(e => e.CreatedAt).ToList();
                var filterAct = options.ApplyTo(act.AsQueryable()) as IQueryable<Activities>;

                if (filterAct != null)
                {
                    act = filterAct.ToList();
                    return this.Request.CreateResponse(HttpStatusCode.OK, act);
                }
                else
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Not found");
                }
            }
        }

        /// <summary>
        /// カテゴリとIDを指定してアクティビティ情報を取得します。
        /// </summary>
        /// <param name="category">category</param>
        /// <param name="id">id</param>
        /// <returns>HTTPレスポンス</returns>
        [HttpGet]
        [Route("{category}/{id}")]
        public HttpResponseMessage Get(string category, int id)
        {
            if (!this.CanRead())
            {
                return this.Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            using (Entities entitiies = new Entities())
            {
                var act = entitiies.Activities.FirstOrDefault(e => e.Category == category && e.Id == id && e.IsAvailable == true);

                if (act != null)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, act);
                }
                else
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Not found");
                }
            }
        }

        /// <summary>
        /// アクティビティ情報を登録します。
        /// </summary>
        /// <param name="act">アクティビティ情報</param>
        /// <returns>HTTPレスポンス</returns>
        [HttpPost]
        [Route("")]
        public HttpResponseMessage Post([FromBody] Activities act)
        {
            // 権限チェック
            if (!this.CanWrite())
            {
                return this.Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            // オブジェクト自体のnullチェック
            if (act == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Activities object.");
            }

            // 各値のnullチェック
            var validationResult = this.ValidateActivitiesModel(act);

            if (validationResult != null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, validationResult);
            }

            // 登録
            var userId = this.UserId;
            var now = DateTime.Now;
            act.CreatedBy = userId;
            act.CreatedAt = now;
            act.UpdatedBy = userId;
            act.UpdatedAt = now;

            using (Entities entitiies = new Entities())
            using (var tran = entitiies.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    // 該当行が１行もないとMax値をとれないので行数チェック
                    var count = entitiies.Activities.Where(e => e.Category == act.Category).Count();
                    int maxId;

                    if (count == 0)
                    {
                        maxId = 0;
                    }
                    else
                    {
                        maxId = entitiies.Activities.Where(e => e.Category == act.Category).Max(e => e.Id);
                    }

                    act.Id = maxId + 1;

                    entitiies.Activities.Add(act);
                    entitiies.SaveChanges();

                    tran.Commit();
                    var message = this.Request.CreateResponse(HttpStatusCode.Created, act);
                    message.Headers.Location = new Uri(this.Request.RequestUri + "/" + act.Category + "/" + act.Id.ToString());
                    return message;
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
                }
            }
        }

        /// <summary>
        /// アクティビティ情報を更新します。
        /// </summary>
        /// <param name="category">カテゴリ</param>
        /// <param name="id">id</param>
        /// <param name="newAct">アクティビティ情報</param>
        /// <returns>HTTPレスポンス</returns>
        [HttpPatch]
        [Route("{category}/{id}")]
        public HttpResponseMessage Patch(string category, int id, [FromBody] Activities newAct)
        {
            // 権限チェック
            if (!this.IsAdmin())
            {
                return this.Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            // オブジェクト自体のnullチェック
            if (newAct == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Activities object.");
            }

            // 既存チェック
            using (Entities entitiies = new Entities())
            {
                var act = entitiies.Activities.FirstOrDefault(e => e.Category == category && e.Id == id);

                if (act == null)
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, $"The Activities {category.ToUpper()}-{id} not exists.");
                }
            }

            using (Entities entitiies = new Entities())
            using (var tran = entitiies.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    var act = entitiies.Activities.Single(e => e.Category == category && e.Id == id);
                    act.Title = newAct.Title;
                    act.Outline = newAct.Outline;
                    act.MediaURL = newAct.MediaURL;
                    act.RelationalURL = newAct.RelationalURL;
                    act.CanWaitable = newAct.CanWaitable;
                    act.IsClosed = newAct.IsClosed;
                    act.WaitingTime = newAct.WaitingTime;
                    act.UpdatedAt = DateTime.Now;
                    act.UpdatedBy = this.UserId;

                    entitiies.SaveChanges();

                    tran.Commit();
                    var message = this.Request.CreateResponse(HttpStatusCode.Created, act);
                    message.Headers.Location = new Uri(this.Request.RequestUri + "/" + act.Category + "/" + act.Id.ToString());
                    return message;
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
                }
            }
        }

        /// <summary>
        /// アクティビティ情報登録前の検証を行います。
        /// </summary>
        /// <param name="act">アクティビティ情報</param>
        /// <returns>エラーメッセージ</returns>
        private string ValidateActivitiesModel(Activities act)
        {
            if (string.IsNullOrEmpty(act.Category) || act.Category.Length != 1)
            {
                return "Invalid Category.";
            }

            if (string.IsNullOrEmpty(act.Title) || act.Title.Length > 30)
            {
                return "Invalid Title.";
            }

            if (string.IsNullOrEmpty(act.Outline) || act.Outline.Length > 500)
            {
                return "Invalid  Outline.";
            }

            return null;
        }
    }
}