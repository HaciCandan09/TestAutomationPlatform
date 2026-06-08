using Microsoft.EntityFrameworkCore;
using TestAutomationPlatform.Models;

namespace TestAutomationPlatform.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Script> Scripts { get; set; }
        public DbSet<Run> Runs { get; set; }
        public DbSet<RunResult> RunResults { get; set; }

        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<TestSuite> TestSuites { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Defect> Defects { get; set; }

        public DbSet<ScheduledRun> ScheduledRuns { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);



            modelBuilder.Entity<Workspace>()
                .HasMany(w => w.TestSuites)
                .WithOne(ts => ts.Workspace)
                .HasForeignKey(ts => ts.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);

           
            //Workspace
            modelBuilder.Entity<Workspace>()
                .HasMany(w => w.Scripts)
                .WithOne(s => s.Workspace)
                .HasForeignKey(s => s.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);

            //hangfire
            modelBuilder.Entity<ScheduledRun>()
                .HasOne(sr => sr.Script)
                .WithMany()
                .HasForeignKey(sr => sr.ScriptId)
                .OnDelete(DeleteBehavior.Restrict);

            //Testsuite
            modelBuilder.Entity<TestSuite>()
                .HasMany(ts => ts.Scripts)
                .WithOne(s => s.TestSuite)
                .HasForeignKey(s => s.TestSuiteId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Category>()
                .HasOne(c => c.Workspace)
                .WithMany(w => w.Categories)
                .HasForeignKey(c => c.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TestSuite>()
                .HasOne(ts => ts.Workspace)
                .WithMany(w => w.TestSuites)
                .HasForeignKey(ts => ts.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);

            //category
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Scripts)
                .WithOne(s => s.Category)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            //defect
            modelBuilder.Entity<Defect>()
                .HasOne(d => d.RunResult)
                .WithOne(r => r.Defect)
                .HasForeignKey<Defect>(d => d.RunResultId);


        }
    }
}