using System.ComponentModel.DataAnnotations;

namespace BlogAPI.ViewModels.Categories
{
    public class EditorCategoryViewModel
    {
        [Required(ErrorMessage = "O campo nome é obrigatório")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "O campo nome deve conter entre 3 a 40 caracteres")]
        public string Name { get; set; }
        [Required(ErrorMessage = "O campo slug é obrigatório")]
        public string Slug { get; set; }
    }
}
