using Microsoft.EntityFrameworkCore;
using QuizAPI.Models;

namespace QuizAPI.Context
{
    public class QuizDbContext : DbContext
    {
        public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options)
        {
        }
        public DbSet<Level> Levels { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configuração de chave estrangeira Pergunta -> Nivel
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Level)
                .WithMany(l => l.Questions)
                .HasForeignKey(q => q.IdLevel);

            // Configuração de chave estrangeira Resposta -> Pergunta
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.IdQuestion);


        }
    }

}
