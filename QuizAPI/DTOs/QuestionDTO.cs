using QuizAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QuizAPI.DTOs
{
    public class QuestionDTO
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(80)]
        public string Description { get; set; } = "";

        public int IdLevel { get; set;}


        [Required]
        public List<AnswerDTO> Answers { get; set; } = new List<AnswerDTO>();
    }
}
