using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommanderGQL.Models{
    //It has One(Platform) to Many(Command) relationship with Commands table. 
    //The many guy (Command) has 2 properties - Platform, PlatformId (easy way to remember) while the one guy (Platform) has just one property - Commands.
    public class Platform{
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(24)]
        public string Name { get; set; }
        [MaxLength(32)]
        public string LicenseKey { get; set; }
        public ICollection<Command> Commands { get; set; } = new List<Command>(); //Navigation property
    }
}