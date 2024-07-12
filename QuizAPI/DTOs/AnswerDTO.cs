using QuizAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QuizAPI.DTOs
{
    public class AnswerDTO
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(80)]
        public string Description { get; set; } = "";

        [Required]
        public bool IsRight { get; set; }

        [JsonIgnore]
        public int? IdQuestion { get; set; }

    }
}
