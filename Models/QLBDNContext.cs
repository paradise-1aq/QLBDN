using Microsoft.EntityFrameworkCore;

namespace QLBDN.Models
{
    public partial class QlbdnContext : DbContext
    {
        public QlbdnContext()
        {
        }

        public QlbdnContext(DbContextOptions<QlbdnContext> options)
            : base(options)
        {
        }

        // === CÁC BẢNG TRONG DATABASE ===
        public virtual DbSet<Club> Clubs { get; set; } = null!;
        public virtual DbSet<Player> Players { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Match> Matches { get; set; } = null!;
        public virtual DbSet<MatchDetail> MatchDetails { get; set; } = null!;
        public virtual DbSet<Round> Rounds { get; set; } = null!;
        public virtual DbSet<Season> Seasons { get; set; } = null!;
        public virtual DbSet<TicketBooking> TicketBookings { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<MatchEvent> MatchEvents { get; set; } = null!;

        public virtual DbSet<Referee> Referees { get; set; } = null!;
        public virtual DbSet<RefereeMatch> RefereeMatches { get; set; } = null!;
        public virtual DbSet<News> News { get; set; } = null!;
        public virtual DbSet<Interaction> Interactions { get; set; } = null!;




        // === CẤU HÌNH KẾT NỐI DATABASE ===
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=localhost;Database=QLBDN;User Id=Dhuy2402;Password=huy123;TrustServerCertificate=True;"
                );
            }
        }

        // === CẤU HÌNH ENTITY ===
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /* ========== CLUB ========== */
            modelBuilder.Entity<Club>(entity =>
            {
                entity.ToTable("CLUB");
                entity.HasKey(e => e.ClubId);

                entity.Property(e => e.ClubId).HasColumnName("ClubID");
                entity.Property(e => e.Name).HasMaxLength(100).IsUnicode(true);
                entity.Property(e => e.HomeStadium).HasMaxLength(100).IsUnicode(true);

                entity.HasMany(c => c.Players)
                      .WithOne(p => p.Club)
                      .HasForeignKey(p => p.ClubId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            /* ========== ROLE ========== */
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("ROLE");
                entity.HasKey(e => e.RoleId);

                entity.Property(e => e.RoleId).HasColumnName("RoleID");
                entity.Property(e => e.RoleName).HasMaxLength(100).IsUnicode(true);
                entity.Property(e => e.RoleDescription).IsUnicode(true);

                entity.HasMany(r => r.Players)
                      .WithOne(p => p.Role)
                      .HasForeignKey(p => p.RoleId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            /* ========== PLAYER ========== */
            modelBuilder.Entity<Player>(entity =>
            {
                entity.ToTable("PLAYER");
                entity.HasKey(e => e.PlayerId);

                entity.Property(e => e.PlayerId).HasColumnName("PlayerID");
                entity.Property(e => e.FullName).HasMaxLength(100).IsUnicode(true);
                entity.Property(e => e.Nationality).HasMaxLength(50).IsUnicode(true);
                entity.Property(e => e.Status).HasMaxLength(20).IsUnicode(true);
                entity.Property(e => e.ShirtNumber);
                entity.Property(e => e.AvatarUrl).HasMaxLength(255).IsUnicode(false);
                entity.Property(e => e.ClubId).HasColumnName("ClubID");
                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.HasOne(p => p.Club)
                      .WithMany(c => c.Players)
                      .HasForeignKey(p => p.ClubId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Role)
                      .WithMany(r => r.Players)
                      .HasForeignKey(p => p.RoleId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            /* ========== MATCH ========== */
            modelBuilder.Entity<Match>(entity =>
            {
                entity.ToTable("MATCH");
                entity.HasKey(e => e.MatchId);

                entity.Property(e => e.MatchId).HasColumnName("MatchID");
                entity.Property(e => e.DateTime).HasColumnType("datetime");
                entity.Property(e => e.Stadium).HasMaxLength(100).IsUnicode(true);
                entity.Property(e => e.Status).HasMaxLength(50).IsUnicode(true);
                entity.Property(e => e.SeasonId).HasColumnName("SeasonID");
                entity.Property(e => e.RoundId).HasColumnName("RoundID");
            });

            /* ========== MATCH_DETAIL ========== */
            modelBuilder.Entity<MatchDetail>(entity =>
            {
                entity.ToTable("MATCH_DETAIL");

                entity.HasKey(e => new { e.MatchId, e.ClubId });

                entity.Property(e => e.MatchId).HasColumnName("MatchID");
                entity.Property(e => e.ClubId).HasColumnName("ClubID");
                entity.Property(e => e.Goals);
                entity.Property(e => e.IsWinner);
                entity.Property(e => e.IsHomeTeam);

                entity.HasOne(e => e.Match)
                      .WithMany(m => m.MatchDetails)
                      .HasForeignKey(e => e.MatchId);

                entity.HasOne(e => e.Club)
                      .WithMany(c => c.MatchDetails)
                      .HasForeignKey(e => e.ClubId);
            });

            /* ========== ROUND ========== */
            modelBuilder.Entity<Round>(entity =>
            {
                entity.ToTable("ROUND");
                entity.HasKey(e => e.RoundId);

                entity.Property(e => e.RoundId).HasColumnName("RoundID");
                entity.Property(e => e.RoundName).HasMaxLength(100).IsUnicode(true);
            });

            /* ========== SEASON ========== */
            modelBuilder.Entity<Season>(entity =>
            {
                entity.ToTable("SEASON");
                entity.HasKey(e => e.SeasonId);

                entity.Property(e => e.SeasonId).HasColumnName("SeasonID");
                entity.Property(e => e.Name).HasMaxLength(100).IsUnicode(true);
                entity.Property(e => e.StartDate).HasColumnType("datetime");
                entity.Property(e => e.EndDate).HasColumnType("datetime");
            });
            /* ========== TICKET_BOOKING ========== */

            modelBuilder.Entity<TicketBooking>(entity =>
            {
                entity.ToTable("TICKET_BOOKING");

                entity.HasKey(e => e.BookingId);

                entity.Property(e => e.BookingId).HasColumnName("BookingID");
                entity.Property(e => e.BookingDateTime).HasColumnType("datetime");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Status).HasMaxLength(50).IsUnicode(true);

                entity.Property(e => e.UserId).HasColumnName("UserID");
                entity.Property(e => e.MatchId).HasColumnName("MatchID");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.TicketBookings)
                    .HasForeignKey(e => e.UserId);

                entity.HasOne(e => e.Match)
                    .WithMany(m => m.TicketBookings)
                    .HasForeignKey(e => e.MatchId);
            });
            /* ========== USER ========== */
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("USER");

                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId).HasColumnName("UserID");
                entity.Property(e => e.Username).HasMaxLength(50).IsUnicode(true);
                entity.Property(e => e.Password).HasMaxLength(50).IsUnicode(true);
                entity.Property(e => e.Role).HasMaxLength(20).IsUnicode(true);
            });

            /* ========== MATCH_EVENT ========== */
            modelBuilder.Entity<MatchEvent>(entity =>
            {
                entity.ToTable("MATCH_EVENT");

                entity.HasKey(e => e.EventID);

                entity.Property(e => e.EventID).HasColumnName("EventID");
                entity.Property(e => e.EventType).HasMaxLength(50).IsUnicode(true);
                entity.Property(e => e.Description).IsUnicode(true);
                entity.Property(e => e.DateTime).HasColumnType("datetime");

                entity.Property(e => e.MatchID).HasColumnName("MatchID");
                entity.Property(e => e.PlayerID).HasColumnName("PlayerID");

                entity.HasOne(e => e.Match)
                    .WithMany(m => m.MatchEvents)
                    .HasForeignKey(e => e.MatchID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Player)
                    .WithMany(p => p.MatchEvents)
                    .HasForeignKey(e => e.PlayerID)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            /* ========== REFEREE ========== */
        modelBuilder.Entity<Referee>(entity =>
        {
            entity.ToTable("REFEREE");

            entity.HasKey(e => e.RefereeId);

            entity.Property(e => e.RefereeId).HasColumnName("RefereeID");
            entity.Property(e => e.FullName).HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.Level).HasMaxLength(50).IsUnicode(true);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Referees)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        /* ========== REFEREE_MATCH ========== */
        modelBuilder.Entity<RefereeMatch>(entity =>
        {
            entity.ToTable("REFEREE_MATCH");

            entity.HasKey(e => new { e.MatchId, e.RefereeId });

            entity.Property(e => e.MatchId).HasColumnName("MatchID");
            entity.Property(e => e.RefereeId).HasColumnName("RefereeID");
            entity.Property(e => e.Role).HasMaxLength(100).IsUnicode(true);

            entity.HasOne(rm => rm.Match)
                .WithMany(m => m.RefereeMatches)
                .HasForeignKey(rm => rm.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rm => rm.Referee)
                .WithMany(r => r.RefereeMatches)
                .HasForeignKey(rm => rm.RefereeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        // ========== NEWS ==========
        modelBuilder.Entity<News>(entity =>
        {
            entity.ToTable("NEWS");

            entity.HasKey(e => e.NewsId);

            entity.Property(e => e.NewsId).HasColumnName("NewsID");

            entity.Property(e => e.Title).HasColumnName("Title").HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.Content).HasColumnName("Content").IsUnicode(true);
            entity.Property(e => e.PostedDate).HasColumnName("PostedDate").HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasColumnName("ImageUrl").HasMaxLength(255);

            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(e => e.User)
                .WithMany(u => u.News)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        // ========== INTERACTION ==========

        modelBuilder.Entity<Interaction>(entity =>
        {
            entity.ToTable("INTERACTION");

            entity.HasKey(e => e.InteractionId);

            entity.Property(e => e.InteractionId).HasColumnName("InteractionID");

            entity.Property(e => e.Content).HasColumnName("Content").IsUnicode(true);
            entity.Property(e => e.Type).HasColumnName("Type").HasMaxLength(20);
            entity.Property(e => e.DateTime).HasColumnName("DateTime").HasColumnType("datetime");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.NewsId).HasColumnName("NewsID");

            entity.HasOne(i => i.User)
                .WithMany(u => u.Interactions)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(i => i.News)
                .WithMany(n => n.Interactions)
                .HasForeignKey(i => i.NewsId)
                .OnDelete(DeleteBehavior.Cascade);
        });



            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
