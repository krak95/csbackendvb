using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;

namespace webAPIreact.Models
{
    public class MyDBcontext : DbContext
    {

        public MyDBcontext(DbContextOptions<MyDBcontext> options) : base(options)
        {
        }
        public DbSet<Production> Production { get; set; }
        public DbSet<Users> Users { get; set; }

    }
    [Table("Production")]
        public class Production
        {
            [Key]
            public required int Id_prod { get; set; }
            public string? Project { get; set; }
            public string? So { get; set; }
            public string? Device { get; set; }
            public string? Codea { get; set; }
            public string? Codeb { get; set; }
            public string? Codepr { get; set; }
            public string? Codedr { get; set; }
            public string? Codeps { get; set; }
            public string? Type0 { get; set; }
            public string? Type1 { get; set; }
            public string? Type2 { get; set; }
            public string? Type3 { get; set; }
            public string? Type4 { get; set; }
            public string? ReadyPQA { get; set; }

    }
    [Table("Users")]
    public class Users
    {
        [Key]
        public required int Idusers { get; set; }
        public string? Username { get; set; }
        public string? Fullname { get; set; }
        public int? Admin { get; set; }
        public string? Role { get; set; }

    }
}
