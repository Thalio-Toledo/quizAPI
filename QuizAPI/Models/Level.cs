using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QuizAPI.Models
{
    [Table("Levels")]
    public class Level
    {
        [Key]
        public int? Id { get; set; }

        [Required]
        [StringLength(80)]
        public string Name { get; set; } 

        [JsonIgnore]
        public List<Question> ? Questions { get; set; } 
    }
}
