namespace ContactModels.DTOs
{
    public class ContactResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string DDD { get; set; } = string.Empty;
        public string NumeroCelular { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        
        public string TelefoneCompleto => $"({DDD}) {NumeroCelular}";
    }
}

