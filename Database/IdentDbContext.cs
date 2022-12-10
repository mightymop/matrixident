using Microsoft.EntityFrameworkCore;
using MatrixIdent.Models;
using MatrixIdent.Services;
using log4net;
using MatrixIdent.Controllers;

namespace MatrixIdent.Database
{
    public class IdentDbContext : DbContext
    {

        private readonly ConfigService _config;

        private ILog log = LogManager.GetLogger(typeof(IdentDbContext));

        public IdentDbContext(ConfigService config) : base()
        {
            _config=config;       
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Directory.GetCurrentDirectory());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string constr = _config.getConfigurationManager().GetConnectionString("SqlServer");
                optionsBuilder.UseSqlServer(constr);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<AuthItem> AuthItems { get; set; }
        public DbSet<HashItem> HashItems { get; set; }
        public DbSet<EmailValidationRequestItem> EmailValidationItems { get; set; }
        public DbSet<MsisdnValidationRequestItem> MsisdnValidationItems { get; set; }
        public DbSet<ThreePidResponseItem> ThreePidResponseItems { get; set; }
        public DbSet<InvitationRequestItem> InvitationRequestItems { get; set; }

        public DbSet<Key> Keys { get; set; }
    }
}
