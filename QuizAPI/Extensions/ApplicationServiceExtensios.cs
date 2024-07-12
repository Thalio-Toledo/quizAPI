using QuizAPI.DTOs;
using QuizAPI.Services;

namespace QuizAPI.Extensions
{
    
    public static class ApplicationServiceExtensions
    {

        public static IServiceCollection AddAplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<QuestionService>();
            services.AddScoped<AnswereService>();
            services.AddScoped<LevelService>();
            services.AddAutoMapper(typeof(MappingProfileDTOs));

            return services;
        }
    }
}
