using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QuizAPI.Models
{
    public class Question
    {
        public int Id { get; set; }

        [StringLength(80)]
        public string Description { get; set; }

        public int IdLevel { get; set; }

        public Level? Level { get; set; }

        public List<Answer>? Answers { get; set; }
    }
    
}
