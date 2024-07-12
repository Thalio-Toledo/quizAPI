using QuizAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QuizAPI.DTOs
{
    public class LevelDTO
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(80)]
        public string Name { get; set; } = "";
    }
}
