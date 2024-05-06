using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LearnEnglish.Models
{
    public class Course
    {
        public int Id { get; set; }

        [MaxLength(100)]
        [Required(ErrorMessage = "Название курса обязательно")]
        [Display(Name = "Название курса")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Описание обязательно")]
        [Display(Name = "Описание")]
        public string Description { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        [Required(ErrorMessage = "Картинка обязательна")]
        [Display(Name = "Лого курса")]
        public string ImgPath { get; set; }
    }
}
