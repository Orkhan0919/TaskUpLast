using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskUp.Models;
using TaskUp.Utilities.Enums;

namespace TaskUp.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Board> Boards { get; set; }
        public DbSet<BoardColumn> BoardColumns { get; set; }
        public DbSet<BoardTask> BoardTasks { get; set; }
        public DbSet<BoardMember> BoardMembers { get; set; }
        public DbSet<TaskAssignee> TaskAssignees { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<TaskAttachment> TaskAttachments { get; set; }
        public DbSet<BannedUser> BannedUsers { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatAttachment> ChatAttachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // 1. BOARD CONFIGURATION
            // ==========================================
            modelBuilder.Entity<Board>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Name).IsRequired().HasMaxLength(100);
                entity.Property(b => b.Description).HasMaxLength(500);
                entity.Property(b => b.JoinCode).IsRequired().HasMaxLength(6);
                entity.Property(b => b.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(b => b.Owner)
                      .WithMany(u => u.OwnedBoards)
                      .HasForeignKey(b => b.OwnerId)
                      .OnDelete(DeleteBehavior.Restrict); 
                
                entity.HasIndex(b => b.JoinCode).IsUnique();
            });

            // ==========================================
            // 2. BOARD COLUMN CONFIGURATION
            // ==========================================
            modelBuilder.Entity<BoardColumn>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(50);
                entity.Property(c => c.Order).HasDefaultValue(0);

                entity.HasOne(c => c.Board)
                      .WithMany(b => b.Columns)
                      .HasForeignKey(c => c.BoardId)
                      .OnDelete(DeleteBehavior.Cascade); 

                entity.HasIndex(c => new { c.BoardId, c.Order });
            });

            // ==========================================
            // 3. BOARD TASK CONFIGURATION
            // ==========================================
            modelBuilder.Entity<BoardTask>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
                entity.Property(t => t.Description).HasMaxLength(1000);
                entity.Property(t => t.Priority)
                    .HasConversion<string>()
                    .HasDefaultValue(TaskPriority.Medium);                
                entity.Property(t => t.Order).HasDefaultValue(0);
                entity.Property(t => t.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(t => t.Column)
                      .WithMany(c => c.Tasks)
                      .HasForeignKey(t => t.ColumnId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(t => new { t.ColumnId, t.Order });
                entity.HasIndex(t => t.DueDate);
                entity.HasIndex(t => t.Priority);
            });

            // ==========================================
            // 4. BOARD MEMBER CONFIGURATION
            // ==========================================
            modelBuilder.Entity<BoardMember>(entity =>
            {
                entity.HasKey(bm => new { bm.BoardId, bm.UserId }); 

                entity.HasOne(bm => bm.Board)
                      .WithMany(b => b.Members)
                      .HasForeignKey(bm => bm.BoardId)
                      .OnDelete(DeleteBehavior.Cascade); 

                entity.HasOne(bm => bm.User)
                      .WithMany(u => u.JoinedBoards)
                      .HasForeignKey(bm => bm.UserId)
                      .OnDelete(DeleteBehavior.Restrict); 
                      
                entity.Property(bm => bm.Role).HasMaxLength(50).HasDefaultValue("Member");
                entity.Property(bm => bm.JoinedAt).HasDefaultValueSql("GETDATE()");
            });

            // ==========================================
            // 5. TASK ASSIGNEE CONFIGURATION
            // ==========================================
            modelBuilder.Entity<TaskAssignee>(entity =>
            {
                entity.HasKey(ta => new { ta.TaskId, ta.UserId });

                entity.HasOne(ta => ta.Task)
                      .WithMany(t => t.Assignees)
                      .HasForeignKey(ta => ta.TaskId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ta => ta.User)
                      .WithMany(u => u.AssignedTasks)
                      .HasForeignKey(ta => ta.UserId)
                      .OnDelete(DeleteBehavior.Restrict); 

                entity.Property(ta => ta.AssignedAt).HasDefaultValueSql("GETDATE()");
            });

            // ==========================================
            // 6. TASK COMMENT CONFIGURATION
            // ==========================================
            modelBuilder.Entity<TaskComment>(entity =>
            {
                entity.HasKey(tc => tc.Id);
                entity.Property(tc => tc.Content).IsRequired().HasMaxLength(1000);
                entity.Property(tc => tc.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(tc => tc.Task)
                      .WithMany(t => t.Comments)
                      .HasForeignKey(tc => tc.TaskId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tc => tc.User)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(tc => tc.UserId)
                      .OnDelete(DeleteBehavior.Restrict); 
            });

            // ==========================================
            // 7. TASK ATTACHMENT CONFIGURATION
            // ==========================================
            modelBuilder.Entity<TaskAttachment>(entity =>
            {
                entity.HasKey(ta => ta.Id);
                entity.Property(ta => ta.FileName).IsRequired().HasMaxLength(255);
                entity.Property(ta => ta.FileType).HasMaxLength(50);
                entity.Property(ta => ta.UploadedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(ta => ta.Task)
                      .WithMany(t => t.Attachments)
                      .HasForeignKey(ta => ta.TaskId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ta => ta.User)
                      .WithMany(u => u.Attachments)
                      .HasForeignKey(ta => ta.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // 8. BANNED USER CONFIGURATION
            // ==========================================
            modelBuilder.Entity<BannedUser>(entity =>
            {
                entity.HasKey(b => b.Id);
    
                entity.HasOne(b => b.Board)
                    .WithMany(b => b.BannedUsers)
                    .HasForeignKey(b => b.BoardId)
                    .OnDelete(DeleteBehavior.Cascade);
    
                entity.HasOne(b => b.User)
                    .WithMany()
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
    
                entity.HasOne(b => b.BannedByUser)
                    .WithMany()
                    .HasForeignKey(b => b.BannedBy)
                    .OnDelete(DeleteBehavior.Restrict);
          
                entity.Property(b => b.BannedAt)
                    .HasDefaultValueSql("GETDATE()");
            });

            // ==========================================
            // 9. CHAT MESSAGE CONFIGURATION
            // ==========================================
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Content).HasMaxLength(1000);
                entity.Property(m => m.UserName).IsRequired().HasMaxLength(100);
                entity.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(m => m.Board)
                      .WithMany()
                      .HasForeignKey(m => m.BoardId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.User)
                      .WithMany()
                      .HasForeignKey(m => m.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(m => m.Attachments)
                      .WithOne(a => a.ChatMessage)
                      .HasForeignKey(a => a.ChatMessageId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<ChatAttachment>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.FileName).IsRequired().HasMaxLength(255);
                entity.Property(a => a.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(a => a.ContentType).HasMaxLength(50);
                entity.Property(a => a.UploadedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}