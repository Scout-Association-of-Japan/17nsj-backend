﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはテンプレートから生成されました。
//
//     このファイルを手動で変更すると、アプリケーションで予期しない動作が発生する可能性があります。
//     このファイルに対する手動の変更は、コードが再生成されると上書きされます。
// </auto-generated>
//------------------------------------------------------------------------------

namespace _17nsj.DataAccess
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Entities : DbContext
    {
        public Entities()
            : base("name=Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<MobileAppConfig> MobileAppConfig { get; set; }
        public virtual DbSet<Activities> Activities { get; set; }
        public virtual DbSet<ActivityCategories> ActivityCategories { get; set; }
        public virtual DbSet<Movies> Movies { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<News> News { get; set; }
        public virtual DbSet<NewsCategories> NewsCategories { get; set; }
        public virtual DbSet<Newspapers> Newspapers { get; set; }
    }
}
