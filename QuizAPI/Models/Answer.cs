using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QuizAPI.Models
{
   
    public class Answer
    {
  
        public int Id { get; set; }


        [StringLength(80)]
        public string  Description { get; set; }

    
        public bool IsRight { get; set; }

        public int IdQuestion { get; set; }


        public Question? Question { get; set; } 



    }
}
