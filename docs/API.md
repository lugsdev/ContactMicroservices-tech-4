# Documentação da API - Contact Microservices

Esta documentação descreve todos os endpoints disponíveis nos microserviços de contatos, incluindo exemplos de requisições e respostas.

## Visão Geral

O sistema Contact Microservices expõe quatro APIs REST independentes, cada uma responsável por uma operação específica do CRUD:

- **ContactCreate.API** (Porta 5001): Criação de contatos
- **ContactRead.API** (Porta 5002): Leitura e consulta de contatos
- **ContactUpdate.API** (Porta 5003): Atualização de contatos
- **ContactDelete.API** (Porta 5004): Exclusão de contatos

## Modelo de Dados

### Contact

```json
{
  "id": 1,
  "nome": "João Silva",
  "ddd": "11",
  "numeroCelular": "987654321",
  "email": "joao.silva@email.com",
  "dataCriacao": "2024-01-15T10:30:00Z",
  "dataAtualizacao": "2024-01-15T11:45:00Z"
}
```

| Campo | Tipo | Descrição | Obrigatório |
|-------|------|-----------|-------------|
| `id` | integer | Identificador único do contato | Não (gerado automaticamente) |
| `nome` | string | Nome completo do contato | Sim |
| `ddd` | string | Código de área (2 dígitos) | Sim |
| `numeroCelular` | string | Número do celular (9 dígitos) | Sim |
| `email` | string | Endereço de email único | Sim |
| `dataCriacao` | datetime | Data/hora de criação (UTC) | Não (gerado automaticamente) |
| `dataAtualizacao` | datetime | Data/hora da última atualização (UTC) | Não (gerado automaticamente) |

### Validações

- **Nome**: 2-100 caracteres
- **DDD**: Exatamente 2 dígitos numéricos
- **Número Celular**: Exatamente 9 dígitos numéricos
- **Email**: Formato válido de email, único no sistema

## ContactCreate.API

### Base URL
```
http://localhost:5001 (desenvolvimento)
https://api.contatos.com/create (produção)
```

### Criar Contato

Cria um novo contato no sistema.

**Endpoint:** `POST /api/contact`

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "nome": "João Silva",
  "ddd": "11",
  "numeroCelular": "987654321",
  "email": "joao.silva@email.com"
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "nome": "João Silva",
  "ddd": "11",
  "numeroCelular": "987654321",
  "email": "joao.silva@email.com",
  "dataCriacao": "2024-01-15T10:30:00Z",
  "dataAtualizacao": null
}
```

**Response (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Nome": ["O campo Nome é obrigatório."],
    "Email": ["O formato do email é inválido."]
  }
}
```

**Response (409 Conflict):**
```json
{
  "message": "Já existe um contato com o email joao.silva@email.com"
}
```

**Exemplo com cURL:**
```bash
curl -X POST http://localhost:5001/api/contact \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "João Silva",
    "ddd": "11",
    "numeroCelular": "987654321",
    "email": "joao.silva@email.com"
  }'
```

### Health Check

Verifica o status de saúde do serviço.

**Endpoint:** `GET /api/contact/health`

**Response (200 OK):**
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## ContactRead.API

### Base URL
```
http://localhost:5002 (desenvolvimento)
https://api.contatos.com/read (produção)
```

### Buscar Contato por ID

Retorna um contato específico pelo seu ID.

**Endpoint:** `GET /api/contact/{id}`

**Parameters:**
- `id` (path, integer): ID do contato

**Response (200 OK):**
```json
{
  "id": 1,
  "nome": "João Silva",
  "ddd": "11",
  "numeroCelular": "987654321",
  "email": "joao.silva@email.com",
  "dataCriacao": "2024-01-15T10:30:00Z",
  "dataAtualizacao": null
}
```

**Response (404 Not Found):**
```json
{
  "message": "Contato com ID 999 não encontrado"
}
```

**Exemplo com cURL:**
```bash
curl http://localhost:5002/api/contact/1
```

### Listar Contatos

Retorna uma lista paginada de contatos com opção de busca.

**Endpoint:** `GET /api/contact`

**Query Parameters:**
- `page` (integer, opcional): Número da página (padrão: 1)
- `pageSize` (integer, opcional): Itens por página (padrão: 10, máximo: 100)
- `search` (string, opcional): Termo de busca (nome, email, DDD ou número)

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": 1,
      "nome": "João Silva",
      "ddd": "11",
      "numeroCelular": "987654321",
      "email": "joao.silva@email.com",
      "dataCriacao": "2024-01-15T10:30:00Z",
      "dataAtualizacao": null
    },
    {
      "id": 2,
      "nome": "Maria Santos",
      "ddd": "21",
      "numeroCelular": "876543210",
      "email": "maria.santos@email.com",
      "dataCriacao": "2024-01-15T11:00:00Z",
      "dataAtualizacao": null
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalItems": 25,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

**Response Headers:**
```
X-Pagination-Current-Page: 1
X-Pagination-Page-Size: 10
X-Pagination-Total-Count: 25
```

**Exemplos com cURL:**
```bash
# Listar primeira página
curl http://localhost:5002/api/contact

# Listar segunda página com 5 itens
curl "http://localhost:5002/api/contact?page=2&pageSize=5"

# Buscar por termo
curl "http://localhost:5002/api/contact?search=joão"

# Buscar por DDD
curl "http://localhost:5002/api/contact?search=11"
```

### Health Check

**Endpoint:** `GET /api/contact/health`

**Response (200 OK):**
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## ContactUpdate.API

### Base URL
```
http://localhost:5003 (desenvolvimento)
https://api.contatos.com/update (produção)
```

### Atualizar Contato (Completo)

Atualiza todos os campos de um contato existente.

**Endpoint:** `PUT /api/contact/{id}`

**Parameters:**
- `id` (path, integer): ID do contato

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "nome": "João Santos",
  "ddd": "11",
  "numeroCelular": "987654321",
  "email": "joao.santos@email.com"
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "nome": "João Santos",
  "ddd": "11",
  "numeroCelular": "987654321",
  "email": "joao.santos@email.com",
  "dataCriacao": "2024-01-15T10:30:00Z",
  "dataAtualizacao": "2024-01-15T11:45:00Z"
}
```

**Response (404 Not Found):**
```json
{
  "message": "Contato com ID 999 não encontrado"
}
```

**Response (409 Conflict):**
```json
{
  "message": "Já existe outro contato com o email joao.santos@email.com"
}
```

**Exemplo com cURL:**
```bash
curl -X PUT http://localhost:5003/api/contact/1 \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "João Santos",
    "ddd": "11",
    "numeroCelular": "987654321",
    "email": "joao.santos@email.com"
  }'
```

### Atualizar Contato (Parcial)

Atualiza apenas os campos especificados de um contato existente.

**Endpoint:** `PATCH /api/contact/{id}`

**Parameters:**
- `id` (path, integer): ID do contato

**Headers:**
```
Content-Type: application/json
```

**Request Body (exemplo - apenas nome):**
```json
{
  "nome": "João Santos Silva"
}
```

**Request Body (exemplo - múltiplos campos):**
```json
{
  "nome": "João Santos Silva",
  "email": "joao.santos.silva@email.com"
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "nome": "João Santos Silva",
  "ddd": "11",
  "numeroCelular": "987654321",
  "email": "joao.santos.silva@email.com",
  "dataCriacao": "2024-01-15T10:30:00Z",
  "dataAtualizacao": "2024-01-15T12:00:00Z"
}
```

**Exemplo com cURL:**
```bash
curl -X PATCH http://localhost:5003/api/contact/1 \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "João Santos Silva"
  }'
```

### Health Check

**Endpoint:** `GET /api/contact/health`

**Response (200 OK):**
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## ContactDelete.API

### Base URL
```
http://localhost:5004 (desenvolvimento)
https://api.contatos.com/delete (produção)
```

### Excluir Contato

Exclui um contato específico do sistema.

**Endpoint:** `DELETE /api/contact/{id}`

**Parameters:**
- `id` (path, integer): ID do contato

**Response (204 No Content):**
```
(Sem conteúdo)
```

**Response (404 Not Found):**
```json
{
  "message": "Contato com ID 999 não encontrado"
}
```

**Exemplo com cURL:**
```bash
curl -X DELETE http://localhost:5004/api/contact/1
```

### Excluir Contatos em Lote

Exclui múltiplos contatos de uma vez.

**Endpoint:** `DELETE /api/contact/batch`

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "ids": [1, 2, 3, 4, 5]
}
```

**Response (200 OK):**
```json
{
  "deletedCount": 3,
  "notFoundIds": [4, 5],
  "message": "3 contatos excluídos com sucesso. 2 IDs não encontrados."
}
```

**Response (400 Bad Request):**
```json
{
  "message": "Lista de IDs não pode estar vazia"
}
```

**Exemplo com cURL:**
```bash
curl -X DELETE http://localhost:5004/api/contact/batch \
  -H "Content-Type: application/json" \
  -d '{
    "ids": [1, 2, 3]
  }'
```

### Health Check

**Endpoint:** `GET /api/contact/health`

**Response (200 OK):**
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## Códigos de Status HTTP

| Código | Descrição | Quando Ocorre |
|--------|-----------|---------------|
| 200 | OK | Operação realizada com sucesso |
| 201 | Created | Recurso criado com sucesso |
| 204 | No Content | Recurso excluído com sucesso |
| 400 | Bad Request | Dados de entrada inválidos |
| 404 | Not Found | Recurso não encontrado |
| 409 | Conflict | Conflito de dados (ex: email duplicado) |
| 500 | Internal Server Error | Erro interno do servidor |

## Tratamento de Erros

### Formato Padrão de Erro

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Descrição do erro",
  "status": 400,
  "detail": "Detalhes específicos do erro",
  "instance": "/api/contact",
  "errors": {
    "campo": ["Mensagem de validação"]
  }
}
```

### Erros de Validação

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Nome": ["O campo Nome é obrigatório.", "O campo Nome deve ter entre 2 e 100 caracteres."],
    "Email": ["O formato do email é inválido."],
    "DDD": ["O DDD deve ter exatamente 2 dígitos."],
    "NumeroCelular": ["O número do celular deve ter exatamente 9 dígitos."]
  }
}
```

## Rate Limiting

Atualmente não implementado, mas planejado para versões futuras:

- **Limite por IP**: 1000 requisições por hora
- **Limite por usuário**: 5000 requisições por hora
- **Headers de resposta**: `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`

## Versionamento

Atualmente na versão 1.0. Futuras versões seguirão o padrão:

- **URL**: `/api/v2/contact`
- **Header**: `Accept: application/vnd.api+json;version=2`

## Autenticação e Autorização

Atualmente não implementado, mas planejado para versões futuras:

### JWT Bearer Token

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Scopes Planejados

- `contacts:read` - Leitura de contatos
- `contacts:write` - Criação e atualização de contatos
- `contacts:delete` - Exclusão de contatos
- `contacts:admin` - Acesso administrativo completo

## Exemplos de Integração

### JavaScript/Fetch

```javascript
// Criar contato
const createContact = async (contact) => {
  const response = await fetch('http://localhost:5001/api/contact', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(contact),
  });
  
  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }
  
  return await response.json();
};

// Buscar contatos
const getContacts = async (page = 1, pageSize = 10, search = '') => {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
    ...(search && { search }),
  });
  
  const response = await fetch(`http://localhost:5002/api/contact?${params}`);
  return await response.json();
};
```

### C# HttpClient

```csharp
public class ContactApiClient
{
    private readonly HttpClient _httpClient;
    
    public ContactApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<ContactResponseDto> CreateContactAsync(CreateContactDto contact)
    {
        var json = JsonSerializer.Serialize(contact);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("http://localhost:5001/api/contact", content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ContactResponseDto>(responseJson);
    }
    
    public async Task<ContactResponseDto> GetContactAsync(int id)
    {
        var response = await _httpClient.GetAsync($"http://localhost:5002/api/contact/{id}");
        
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
            
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ContactResponseDto>(json);
    }
}
```

### Python Requests

```python
import requests
import json

class ContactApiClient:
    def __init__(self, base_url="http://localhost"):
        self.base_urls = {
            'create': f"{base_url}:5001",
            'read': f"{base_url}:5002",
            'update': f"{base_url}:5003",
            'delete': f"{base_url}:5004"
        }
    
    def create_contact(self, contact):
        response = requests.post(
            f"{self.base_urls['create']}/api/contact",
            json=contact,
            headers={'Content-Type': 'application/json'}
        )
        response.raise_for_status()
        return response.json()
    
    def get_contact(self, contact_id):
        response = requests.get(f"{self.base_urls['read']}/api/contact/{contact_id}")
        
        if response.status_code == 404:
            return None
            
        response.raise_for_status()
        return response.json()
    
    def list_contacts(self, page=1, page_size=10, search=None):
        params = {'page': page, 'pageSize': page_size}
        if search:
            params['search'] = search
            
        response = requests.get(
            f"{self.base_urls['read']}/api/contact",
            params=params
        )
        response.raise_for_status()
        return response.json()
```

## Monitoramento e Observabilidade

### Health Checks

Todos os serviços expõem endpoints de health check que podem ser utilizados por:

- **Kubernetes**: Liveness e Readiness Probes
- **Load Balancers**: Health Check Endpoints
- **Monitoring Tools**: Uptime monitoring

### Métricas

Cada serviço expõe métricas no formato Prometheus em `/metrics`:

- `http_requests_total` - Total de requisições HTTP
- `http_request_duration_seconds` - Duração das requisições
- `database_connections_active` - Conexões ativas com o banco
- `rabbitmq_messages_published_total` - Total de mensagens publicadas

### Logs Estruturados

Todos os logs seguem formato estruturado JSON:

```json
{
  "timestamp": "2024-01-15T10:30:00Z",
  "level": "Information",
  "message": "Contato criado com sucesso",
  "properties": {
    "ContactId": 1,
    "Email": "joao.silva@email.com",
    "RequestId": "abc123",
    "UserId": "user123"
  }
}
```

Esta documentação fornece uma referência completa para integração com os microserviços de contatos, incluindo todos os endpoints, formatos de dados e exemplos práticos de uso.

