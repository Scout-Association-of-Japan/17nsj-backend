﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _17nsj.Jedi.Constants;
using _17nsj.Jedi.Domains;
using _17nsj.Jedi.Models;
using _17nsj.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace _17nsj.Jedi.Pages
{
    [Authorize(Roles=UserRoleDomain.Admin + "," + UserRoleDomain.SysAdmin)]
    public class NoticeBoardDeleteModel : PageModelBase
    {
        public NoticeBoardDeleteModel(JediDbContext dbContext)
            : base(dbContext)
        {

        }

        [BindProperty]
        public NoticeModel TargetNotice { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return new NotFoundResult();

            this.PageInitializeAsync();

            var notice = await this.DBContext.NoticeBoard.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (notice == null) return new NotFoundResult();
            if (!this.IsSysAdmin || notice.CreatedBy != this.UserID) return new ForbidResult();

            this.TargetNotice = new NoticeModel();
            this.TargetNotice.Id = notice.Id;
            this.TargetNotice.Title = notice.Title;
            this.TargetNotice.Receiver = notice.Receiver;
            this.TargetNotice.Sender = notice.Sender;
            this.TargetNotice.CreatedAt = notice.CreatedAt;
            this.TargetNotice.Termination = notice.Termination;

            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            using (var tran = await this.DBContext.Database.BeginTransactionAsync())
            {
                if (TargetNotice.Id ==0) return new NotFoundResult();

                this.PageInitializeAsync();

                var notice = await this.DBContext.NoticeBoard.Where(x => x.Id == TargetNotice.Id).FirstOrDefaultAsync();
                if (notice == null)
                {
                    //対象なしエラー
                    this.MsgCategory = MsgCategoryDomain.Error;
                    this.Msg = メッセージ.選択対象なし;
                    return this.Page();
                }

                if (!this.IsSysAdmin || notice.CreatedBy != this.UserID) return new ForbidResult();

                try
                {
                    this.DBContext.NoticeBoard.Remove(notice);
                    await this.DBContext.SaveChangesAsync();
                    tran.Commit();
                    return new RedirectResult("/NoticeBoardList");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    this.MsgCategory = MsgCategoryDomain.Error;
                    this.Msg = ex.Message;
                    return this.Page();
                }
            }
        }
    }
}