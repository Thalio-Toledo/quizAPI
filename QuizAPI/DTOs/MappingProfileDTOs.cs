using AutoMapper;
using QuizAPI.Models;

namespace QuizAPI.DTOs
{
    public class MappingProfileDTOs : Profile
    {
        public MappingProfileDTOs()
        {
            CreateMap<Question, QuestionDTO>().ReverseMap();
            CreateMap<Level, LevelDTO>().ReverseMap();
            CreateMap<Answer, AnswerDTO>().ReverseMap();
        }
    }
}
