﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Flh.Business.Data
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class FlhContext : DbContext
    {
        public FlhContext()
            : base("name=FlhContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Admin> Admin { get; set; }
        public virtual DbSet<AdminModifyHistory> AdminModifyHistory { get; set; }
        public virtual DbSet<Area> Area { get; set; }
        public virtual DbSet<Classes> Classes { get; set; }
        public virtual DbSet<LoginHistory> LoginHistory { get; set; }
        public virtual DbSet<Trade> Trade { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<VerifyCode> VerifyCode { get; set; }
        public virtual DbSet<SmsHistory> SmsHistory { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<Advertisement> Advertisement { get; set; }
        public virtual DbSet<FollowUpRecord> FollowUpRecord { get; set; }
    }
}
