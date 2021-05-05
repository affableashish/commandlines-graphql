using System.ComponentModel.DataAnnotations;

namespace CommanderGQL.Models
{
    //It has Many(Command) to One(Platform) relationship with Platform table. 
    //The many guy (Command) has 2 properties - Platform, PlatformId (easy way to remember) while the one guy (Platform) has just one property - Commands.
    public class Command
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(64)]
        public string HowTo { get; set; }
        [Required]
        [MaxLength(64)]
        public string CommandLine { get; set; }
        [Required]
        public int PlatformId { get; set; } //Foreign key FOR Platform table
        public Platform Platform { get; set; } //This is navigation property and used to retrieve Platform using the .Include extension method.
    }
}