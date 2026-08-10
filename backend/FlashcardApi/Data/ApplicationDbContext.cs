using Microsoft.EntityFrameworkCore;
using FlashcardApi.Models;

namespace FlashcardApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    // DbSets = tables in the database
    public DbSet<User> Users { get; set; }
    public DbSet<Deck> Decks { get; set; }
    public DbSet<Flashcard> Flashcards { get; set; }
    public DbSet<StudySettings> StudySettings { get; set; }
    public DbSet<CardProgress> CardProgress { get; set; }
    public DbSet<ShareLink> ShareLinks { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // === Unique constraints ===
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        
        // === Relationships ===
        
        // Deck -> User (owner)
        modelBuilder.Entity<Deck>()
            .HasOne(d => d.Owner)
            .WithMany()
            .HasForeignKey(d => d.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Flashcard -> Deck
        modelBuilder.Entity<Flashcard>()
            .HasOne(f => f.Deck)
            .WithMany(d => d.Flashcards)
            .HasForeignKey(f => f.DeckId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // StudySettings -> User + Deck (composite unique constraint)
        modelBuilder.Entity<StudySettings>()
            .HasIndex(ss => new { ss.UserId, ss.DeckId })
            .IsUnique(); // One settings record per user per deck
        
        modelBuilder.Entity<StudySettings>()
            .HasOne(ss => ss.User)
            .WithMany()
            .HasForeignKey(ss => ss.UserId);
        
        modelBuilder.Entity<StudySettings>()
            .HasOne(ss => ss.Deck)
            .WithMany()
            .HasForeignKey(ss => ss.DeckId);
        
        // CardProgress -> User + Card (composite unique)
        modelBuilder.Entity<CardProgress>()
            .HasIndex(cp => new { cp.UserId, cp.CardId })
            .IsUnique(); // One progress record per user per card
        
        modelBuilder.Entity<CardProgress>()
            .HasOne(cp => cp.User)
            .WithMany()
            .HasForeignKey(cp => cp.UserId);
        
        modelBuilder.Entity<CardProgress>()
            .HasOne(cp => cp.Card)
            .WithMany()
            .HasForeignKey(cp => cp.CardId);
        
        // ShareLink -> Deck
        modelBuilder.Entity<ShareLink>()
            .HasIndex(sl => sl.Token)
            .IsUnique(); // Token must be unique for sharing
        
        modelBuilder.Entity<ShareLink>()
            .HasOne(sl => sl.Deck)
            .WithMany()
            .HasForeignKey(sl => sl.DeckId);
        
        // Set default values for dates (optional but useful)
        modelBuilder.Entity<Deck>()
            .Property(d => d.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        modelBuilder.Entity<Flashcard>()
            .Property(f => f.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
