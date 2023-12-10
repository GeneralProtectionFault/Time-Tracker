using Godot;
using System;
using Microsoft.EntityFrameworkCore;
using System.IO;



public partial class TrackerContext : DbContext
{
    public DbSet<WorkTask> Tasks{ get; set; }
    public string DbPath;


    protected override void OnConfiguring(DbContextOptionsBuilder Options) => Options.UseSqlite($"Data Source={DbPath}");
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkTask>(
            wt =>
            {
                // Define the primary key
                wt.HasKey(nameof(WorkTask.JobNumber), nameof(WorkTask.TaskNumber), nameof(WorkTask.Date));

                wt.Property(c => c.TimeElapsed).HasColumnType("Time");
                wt.Property(c => c.Date).HasColumnType("Date");
            }
        );

        //modelBuilder.Entity<WorkTask>().HasKey(nameof(WorkTask.JobNumber), nameof(WorkTask.TaskNumber));
    }


    public TrackerContext()
    {
        var WorkingDirectory = Directory.GetCurrentDirectory();
		DbPath = Path.Combine(WorkingDirectory, "TimeTracker.db");
    }
    
}
