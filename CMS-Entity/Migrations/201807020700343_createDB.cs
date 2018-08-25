namespace CMS_Entity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class createDB : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CMS_Account",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 60, unicode: false),
                        Sequence = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        Account = c.String(nullable: false, maxLength: 100, unicode: false),
                        Password = c.String(nullable: false, maxLength: 250, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedBy = c.String(maxLength: 60, unicode: false),
                        UpdatedDate = c.DateTime(),
                        UpdatedBy = c.String(maxLength: 60, unicode: false),
                        CreatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CMS_Employee",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 60, unicode: false),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        LastName = c.String(nullable: false, maxLength: 20),
                        Employee_Address = c.String(maxLength: 250),
                        Employee_Phone = c.String(nullable: false, maxLength: 11, unicode: false),
                        Employee_Email = c.String(nullable: false, maxLength: 250, unicode: false),
                        Employee_IDCard = c.String(maxLength: 50, unicode: false),
                        BirthDate = c.DateTime(),
                        Password = c.String(nullable: false, maxLength: 250, unicode: false),
                        ImageURL = c.String(maxLength: 250, unicode: false),
                        IsSupperAdmin = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedBy = c.String(maxLength: 60, unicode: false),
                        UpdatedDate = c.DateTime(),
                        UpdatedBy = c.String(maxLength: 60, unicode: false),
                        CreatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CMS_GroupKey",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 60, unicode: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Sequence = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        CreatedBy = c.String(maxLength: 60, unicode: false),
                        CreatedDate = c.DateTime(),
                        UpdatedBy = c.String(maxLength: 60, unicode: false),
                        UpdatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.CMS_R_GroupKey_KeyWord",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 60, unicode: false),
                        GroupKeyID = c.String(nullable: false, maxLength: 60, unicode: false),
                        KeyWordID = c.String(nullable: false, maxLength: 60, unicode: false),
                        Status = c.Int(nullable: false),
                        CreatedBy = c.String(maxLength: 60, unicode: false),
                        CreatedDate = c.DateTime(),
                        UpdatedBy = c.String(maxLength: 60, unicode: false),
                        UpdatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.CMS_KeyWord", t => t.KeyWordID)
                .ForeignKey("dbo.CMS_GroupKey", t => t.GroupKeyID)
                .Index(t => t.GroupKeyID)
                .Index(t => t.KeyWordID);
            
            CreateTable(
                "dbo.CMS_KeyWord",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 60, unicode: false),
                        KeyWord = c.String(nullable: false, maxLength: 100),
                        Sequence = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        CreatedBy = c.String(maxLength: 60, unicode: false),
                        CreatedDate = c.DateTime(),
                        UpdatedBy = c.String(maxLength: 60, unicode: false),
                        UpdatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.CMS_R_KeyWord_Pin",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 60, unicode: false),
                        KeyWordID = c.String(nullable: false, maxLength: 60, unicode: false),
                        PinID = c.String(nullable: false, maxLength: 60, unicode: false),
                        Status = c.Int(nullable: false),
                        CreatedBy = c.String(maxLength: 60, unicode: false),
                        CreatedDate = c.DateTime(),
                        UpdatedBy = c.String(maxLength: 60, unicode: false),
                        UpdatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.CMS_Pin", t => t.PinID)
                .ForeignKey("dbo.CMS_KeyWord", t => t.KeyWordID)
                .Index(t => t.KeyWordID)
                .Index(t => t.PinID);
            
            CreateTable(
                "dbo.CMS_Pin",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 60, unicode: false),
                        Link = c.String(maxLength: 2000),
                        Domain = c.String(nullable: false, maxLength: 100),
                        Repin_count = c.Int(),
                        ImageUrl = c.String(maxLength: 2000),
                        Created_At = c.DateTime(),
                        Status = c.Int(nullable: false),
                        CreatedBy = c.String(maxLength: 60, unicode: false),
                        CreatedDate = c.DateTime(),
                        UpdatedBy = c.String(maxLength: 60, unicode: false),
                        UpdatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.CMS_Log",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 60, unicode: false),
                        Decription = c.String(maxLength: 100, unicode: false),
                        CreatedDate = c.DateTime(),
                        JsonContent = c.String(maxLength: 4000, unicode: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CMS_R_GroupKey_KeyWord", "GroupKeyID", "dbo.CMS_GroupKey");
            DropForeignKey("dbo.CMS_R_KeyWord_Pin", "KeyWordID", "dbo.CMS_KeyWord");
            DropForeignKey("dbo.CMS_R_KeyWord_Pin", "PinID", "dbo.CMS_Pin");
            DropForeignKey("dbo.CMS_R_GroupKey_KeyWord", "KeyWordID", "dbo.CMS_KeyWord");
            DropIndex("dbo.CMS_R_KeyWord_Pin", new[] { "PinID" });
            DropIndex("dbo.CMS_R_KeyWord_Pin", new[] { "KeyWordID" });
            DropIndex("dbo.CMS_R_GroupKey_KeyWord", new[] { "KeyWordID" });
            DropIndex("dbo.CMS_R_GroupKey_KeyWord", new[] { "GroupKeyID" });
            DropTable("dbo.CMS_Log");
            DropTable("dbo.CMS_Pin");
            DropTable("dbo.CMS_R_KeyWord_Pin");
            DropTable("dbo.CMS_KeyWord");
            DropTable("dbo.CMS_R_GroupKey_KeyWord");
            DropTable("dbo.CMS_GroupKey");
            DropTable("dbo.CMS_Employee");
            DropTable("dbo.CMS_Account");
        }
    }
}
