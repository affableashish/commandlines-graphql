using Microsoft.EntityFrameworkCore;
using CommanderGQL.Models;

namespace CommanderGQL.Data{
    public class AppDbContext : DbContext{
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<Command> Commands { get; set; }

        //Give instruction to EF on how to create schema
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Perspective of Platform
            //The many guy (Command) is in 2 places compared to (Platform) in 1 place
            //One more place while specifying Foreign key. The rest are just showing the one to many relationship
            modelBuilder.Entity<Platform>() //EHWH - Order Matters. H always comes before W and it's a cycle after that - H W H
                        .HasMany(platform=>platform.Commands) //One to Many :public ICollection<Command> Commands { get; set; }
                        .WithOne(command=>command.Platform!) //Relationship with itself from command's eyes. The Platform can't be null. WithOne - because a command can have only one Platform.
                        .HasForeignKey(command=>command.PlatformId); //Weird it's in both places

            //Perspective of Command
            //The many guy (Command) is in 2 places compared to (Platform) in 1 place. 
            //One more place while specifying Foreign key. The rest are just showing the one to many relationship
            modelBuilder.Entity<Command>() //EHWH - Order Matters. H always comes before W and it's a cycle after that - H W H
                        .HasOne(command=>command.Platform) //One to Many
                        .WithMany(platform=>platform.Commands) //Relationship with itself from platform's eyes. A platform can have many commands.
                        .HasForeignKey(command=>command.PlatformId); //Weird it's in both places
        }
    }
}