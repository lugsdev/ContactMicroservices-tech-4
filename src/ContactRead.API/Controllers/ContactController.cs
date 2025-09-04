using Microsoft.AspNetCore.Mvc;
using ContactModels;
using ContactModels.DTOs;

namespace ContactRead.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ILogger<ContactController> _logger;

        public ContactController(ILogger<ContactController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ContactResponseDto>> GetContact(int id)
        {
            try
            {
                _logger.LogInformation("Buscando contato com ID: {Id}", id);

                if (id <= 0)
                {
                    return BadRequest("ID deve ser maior que zero");
                }

                // TODO: Buscar no banco de dados via Dapper
                // Por enquanto, simular um contato
                var contact = new Contact
                {
                    Id = id,
                    Nome = "João Silva",
                    DDD = "11",
                    NumeroCelular = "987654321",
                    Email = "joao.silva@email.com",
                    DataCriacao = DateTime.UtcNow.AddDays(-10)
                };

                if (contact == null)
                {
                    _logger.LogWarning("Contato com ID {Id} não encontrado", id);
                    return NotFound($"Contato com ID {id} não encontrado");
                }

                var response = new ContactResponseDto
                {
                    Id = contact.Id,
                    Nome = contact.Nome,
                    DDD = contact.DDD,
                    NumeroCelular = contact.NumeroCelular,
                    Email = contact.Email,
                    DataCriacao = contact.DataCriacao,
                    DataAtualizacao = contact.DataAtualizacao
                };

                _logger.LogInformation("Contato encontrado: {Nome}", contact.Nome);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar contato com ID {Id}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContactResponseDto>>> GetAllContacts(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            try
            {
                _logger.LogInformation("Buscando contatos - Página: {Page}, Tamanho: {PageSize}, Busca: {Search}", 
                    page, pageSize, search);

                if (page <= 0) page = 1;
                if (pageSize <= 0 || pageSize > 100) pageSize = 10;

                // TODO: Buscar no banco de dados via Dapper com paginação e filtro
                // Por enquanto, simular uma lista de contatos
                var contacts = new List<Contact>
                {
                    new Contact { Id = 1, Nome = "João Silva", DDD = "11", NumeroCelular = "987654321", Email = "joao.silva@email.com", DataCriacao = DateTime.UtcNow.AddDays(-10) },
                    new Contact { Id = 2, Nome = "Maria Santos", DDD = "21", NumeroCelular = "876543210", Email = "maria.santos@email.com", DataCriacao = DateTime.UtcNow.AddDays(-5) },
                    new Contact { Id = 3, Nome = "Pedro Oliveira", DDD = "31", NumeroCelular = "765432109", Email = "pedro.oliveira@email.com", DataCriacao = DateTime.UtcNow.AddDays(-2) }
                };

                // Aplicar filtro de busca se fornecido
                if (!string.IsNullOrWhiteSpace(search))
                {
                    contacts = contacts.Where(c => 
                        c.Nome.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        c.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        c.DDD.Contains(search) ||
                        c.NumeroCelular.Contains(search)
                    ).ToList();
                }

                // Aplicar paginação
                var totalItems = contacts.Count;
                var pagedContacts = contacts
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = pagedContacts.Select(c => new ContactResponseDto
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    DDD = c.DDD,
                    NumeroCelular = c.NumeroCelular,
                    Email = c.Email,
                    DataCriacao = c.DataCriacao,
                    DataAtualizacao = c.DataAtualizacao
                });

                Response.Headers.Add("X-Total-Count", totalItems.ToString());
                Response.Headers.Add("X-Page", page.ToString());
                Response.Headers.Add("X-Page-Size", pageSize.ToString());

                _logger.LogInformation("Retornando {Count} contatos de um total de {Total}", 
                    pagedContacts.Count, totalItems);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar contatos");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { Status = "Healthy", Service = "ContactRead.API", Timestamp = DateTime.UtcNow });
        }
    }
}

