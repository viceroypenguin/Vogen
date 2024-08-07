using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Vogen;

namespace UsingTypesGeneratedInTheSameProject;

/*
 * In this scenario, we want to use the EFCore value converters and comparers
 * for the value objects that were generated by Vogen in the Domain project.
 *
 * We create an `EfCoreConverters` class, and decorate it with attributes, where each attribute says which value object should have
 * a converter generated for it.
 *
 * Then, we have two ways of registering all of these converters;
 * 1. override `ConfigureConventions` and call `RegisterAllInVogenEfCoreConverters` (replacing the name after 'RegisterAllIn' with the name of your marker interface - in this example, there's two)
 * 2. call `HasVogenConversion` in `OnModelCreating`.
 */


// By having this partial marker class, Vogen will generate converters for each value object mentioned in the attributes.
// The naming of this class is later used to get the converters, or for when registering them via the generated extension
// methods like `RegisterAllInEfCoreConverters` or `HasVogenConversion`
[EfCoreConverter<Id>]
[EfCoreConverter<Name>]
[EfCoreConverter<Age>]
internal sealed partial class VogenEfCoreConverters1;

// We don't need two marker interfaces; this just demonstrates that you can break them up - see below
// on how they're used (configurationBuilder.RegisterAllInVogenEfCoreConverters2();)
[EfCoreConverter<Department>]
[EfCoreConverter<HireDate>]
internal sealed partial class VogenEfCoreConverters2;

public static class EfCoreScenario
{
    public static void Run()
    {
        AddAndSaveItems(amount: 10);
        AddAndSaveItems(amount: 10);

        PrintItems();

        return;

        static void AddAndSaveItems(int amount)
        {
            using var context = new DbContext();

            for (int i = 0; i < amount; i++)
            {
                var entity = new EmployeeEntity
                {
                    Name = Name.From("Fred #" + i),
                    Age = Age.From(42 + i),
                    Department = Department.From("Quarry"),
                    HireDate = HireDate.From(new DateOnly(1066, 12, 13))
                };

                context.Entities.Add(entity);
            }

            context.SaveChanges();
        }

        static void PrintItems()
        {
            using var ctx = new DbContext();

            var entities = ctx.Entities.ToList();
            Console.WriteLine(string.Join(Environment.NewLine, entities.Select(e => $"ID: {e.Id.Value}, Name: {e.Name}, Age: {e.Age}")));
        }
    }
}

internal class DbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<EmployeeEntity> Entities { get; set; } = default!;

    // you can use this method explicitly when creating your entities, or use SomeIdValueGenerator as shown below
    // public int GetNextMyEntityId()
    // {
    //     var maxLocalId = SomeEntities.Local.Any() ? SomeEntities.Local.Max(e => e.Id.Value) : 0;
    //     var maxSavedId = SomeEntities.Any() ? SomeEntities.Max(e => e.Id.Value) : 0;
    //     return Math.Max(maxLocalId, maxSavedId) + 1;
    // }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        
        // There are two ways of registering these, you can call the generated extension method here,
        // or register the converters individually, like below in `OnModelCreating`.
        configurationBuilder.RegisterAllInVogenEfCoreConverters1();
        configurationBuilder.RegisterAllInVogenEfCoreConverters2();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<EmployeeEntity>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(e => e.Id).HasValueGenerator<SomeIdValueGenerator>();
            
            // There are two ways of registering these, you can do them inline here,
            // or with the `RegisterAllIn[xxx]` like above in `ConfigureConventions`
            
            // b.Property(e => e.Id).HasVogenConversion();
            // b.Property(e => e.Name).HasVogenConversion();
            // b.Property(e => e.Department).HasVogenConversion();
            // b.Property(e => e.HireDate).HasVogenConversion();
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => 
        optionsBuilder.UseInMemoryDatabase("SomeDB");
}

internal class SomeIdValueGenerator : ValueGenerator<Id>
{
    public override Id Next(EntityEntry entry)
    {
        var entities = ((DbContext)entry.Context).Entities;

        var next = Math.Max(MaxFrom(entities.Local), MaxFrom(entities)) + 1;

        return Id.From(next);

        static int MaxFrom(IEnumerable<EmployeeEntity> es)
        {
            return es.Any() ? es.Max(e => e.Id.Value) : 0;
        }
    }

    public override bool GeneratesTemporaryValues => false;
}