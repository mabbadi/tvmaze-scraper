using Microsoft.EntityFrameworkCore;
namespace rtl_app.Context
{

    public class RTLConsumerContext : DbContext
    {
        public DbSet<ConsumerQueueItem> ConsumerQueue { get; set; }
        public DbSet<UnprocessedEpisodeDetails> UnprocessedEpisodesDetails { get; set; }

        public string DbPath { get; }

        public RTLConsumerContext(DbContextOptions dbOptions) : base(dbOptions)
        {
        }
    }
}