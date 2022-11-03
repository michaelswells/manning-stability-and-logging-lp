using Microsoft.EntityFrameworkCore;

using RobotsInc.Inspections.API.I;
using RobotsInc.Inspections.Models;
using RobotsInc.Inspections.Models.Security;

using ArticulatedRobot = RobotsInc.Inspections.Models.ArticulatedRobot;
using AutomatedGuidedVehicle = RobotsInc.Inspections.Models.AutomatedGuidedVehicle;
using Customer = RobotsInc.Inspections.Models.Customer;
using Inspection = RobotsInc.Inspections.Models.Inspection;
using Note = RobotsInc.Inspections.Models.Note;
using Robot = RobotsInc.Inspections.Models.Robot;

namespace RobotsInc.Inspections.Repositories;

public class InspectionsDbContext : DbContext
{
    protected InspectionsDbContext()
    {
    }

    public InspectionsDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<User> Users
        => Set<User>();

    public DbSet<Claim> Claims
        => Set<Claim>();

    public DbSet<Customer> Customers
        => Set<Customer>();

    public DbSet<Robot> Robots
        => Set<Robot>();

    public DbSet<ArticulatedRobot> ArticulatedRobots
        => Set<ArticulatedRobot>();

    public DbSet<AutomatedGuidedVehicle> AutomatedGuidedVehicles
        => Set<AutomatedGuidedVehicle>();

    public DbSet<Inspection> Inspections
        => Set<Inspection>();

    public DbSet<Note> Notes
        => Set<Note>();

    public DbSet<Photo> Photos
        => Set<Photo>();

    public DbSet<TModel> GetDbSet<TModel>()
        where TModel : class
        => Set<TModel>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique(true);

        modelBuilder
            .Entity<Robot>()
            .Property("Discriminator")
            .HasMaxLength(30);
    }

    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder
            .Properties<NavigationType>()
            .HaveConversion<string>()
            .HaveMaxLength(10);

        configurationBuilder
            .Properties<ChargingType>()
            .HaveConversion<string>()
            .HaveMaxLength(22);

        configurationBuilder
            .Properties<InspectionState>()
            .HaveConversion<string>()
            .HaveMaxLength(8);

        configurationBuilder
            .Properties<ImportanceLevel>()
            .HaveConversion<string>()
            .HaveMaxLength(6);
    }
}
