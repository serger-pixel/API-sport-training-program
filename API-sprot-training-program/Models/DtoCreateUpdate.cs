using System.ComponentModel.DataAnnotations;

namespace API_sprot_training_program.Models
{

    public class DtoCreateUpdate
    {

        [StringLength(
            Validation.MAX_LEN_STRING,
            MinimumLength = Validation.MIN_LEN_STRING,
            ErrorMessage = $"Длина названия должна быть от 5 до 255 символов."
        )]
        public String Title { get; set; }

        [StringLength(
            Validation.MAX_LEN_STRING, 
            MinimumLength = Validation.MIN_LEN_STRING,
            ErrorMessage = $"Длина описания должна быть от 5 до 255 символов."
        )]
        public String Description { get; set; }

        [Range(
            Validation.MIN_CNT_IN_WEEK,
            Validation.MAX_CNT_IN_WEEK,
            ErrorMessage = "Продолжительность должна быть от 1 до 52 недель."
        )]
        
        public int DurationInWeek { get; set; }

        [Range(
            Validation.MIN_CNT_DURATION_IN_WEEK,
            Validation.MAX_CNT_DURATION_IN_WEEK,
            ErrorMessage = $"Количество недель должно быть от 1 до 7 дней."
        )]
        public int CntInWeek { get; set; }

    }
}
