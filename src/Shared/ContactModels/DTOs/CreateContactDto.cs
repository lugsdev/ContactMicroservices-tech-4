using System.ComponentModel.DataAnnotations;

namespace ContactModels.DTOs
{
    public class CreateContactDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "DDD é obrigatório")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "DDD deve ter exatamente 2 dígitos")]
        [RegularExpression(@"^\d{2}$", ErrorMessage = "DDD deve conter apenas números")]
        public string DDD { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Número de celular é obrigatório")]
        [StringLength(9, MinimumLength = 8, ErrorMessage = "Número deve ter entre 8 e 9 dígitos")]
        [RegularExpression(@"^\d{8,9}$", ErrorMessage = "Número deve conter apenas dígitos")]
        public string NumeroCelular { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "E-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail deve ter um formato válido")]
        [StringLength(150, ErrorMessage = "E-mail deve ter no máximo 150 caracteres")]
        public string Email { get; set; } = string.Empty;
    }
}

