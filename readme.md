# CheckPoint1 - Sistema de Gestão de Loja Online

Projeto desenvolvido em C# (.NET 9) para a disciplina de C# na FIAP, com foco em Entity Framework Core e ADO.NET, simulando um sistema de gestão de loja online.

## Funcionalidades

- Cadastro e listagem de categorias
- Cadastro, listagem e atualização de produtos (com controle de estoque)
- Cadastro, listagem e atualização de clientes (com validação de e-mail)
- Cadastro e gerenciamento de pedidos (com múltiplos itens, controle de estoque e status)
- Cancelamento de pedidos e devolução de estoque
- Relatórios e consultas avançadas (LINQ e SQL):
  - Produtos mais vendidos
  - Faturamento por cliente e por categoria
  - Pedidos por período
  - Produtos sem venda
  - Análise de vendas mensal
  - Dashboard executivo

## Tecnologias Utilizadas

- .NET 9
- C#
- Entity Framework Core (SQLite)
- ADO.NET (SQLite)

## Estrutura do Projeto

- `Models/` - Entidades do domínio (Categoria, Produto, Cliente, Pedido, PedidoItem)
- `Context/CheckpointContext.cs` - Contexto do Entity Framework
- `Services/EntityFrameworkService.cs` - Operações CRUD e consultas usando EF Core
- `Services/AdoNetService.cs` - Relatórios e operações usando ADO.NET puro
- `Enums/StatusPedido.cs` - Enumeração de status dos pedidos
- `Program.cs` - Menu principal e inicialização

## Como Executar

1. **Pré-requisitos:**
	- .NET 9 SDK instalado
	- Visual Studio, VS Code ou Rider

2. **Restaurar dependências:**
	```sh
	dotnet restore
	```

3. **Build do projeto:**
	```sh
	dotnet build
	```

4. **Executar:**
	```sh
	dotnet run
	```

5. **Primeira execução:**
	- O banco de dados `loja.db` e as tabelas serão criados automaticamente.
	- Dados iniciais de exemplo são inseridos para facilitar testes.

## Exemplos de Uso

### Cadastro de Categoria
```
╔══════════════════════════════════════╗
║            CATEGORIAS               ║
╠══════════════════════════════════════╣
║ 1 - Criar Categoria                 ║
║ 2 - Listar Categorias               ║
║ 0 - Voltar                          ║
╚══════════════════════════════════════╝
Escolha uma opção: 1
Nome da categoria: Eletrônicos
Categoria criada!
```

### Cadastro de Produto
```
=== CRIAR PRODUTO ===
1: Eletrônicos (Produtos: 2)
2: Roupas (Produtos: 2)
3: Livros (Produtos: 2)
Informe o ID da categoria: 1
Nome do produto: Notebook
Preço: 3500
Estoque: 5
Produto criado!
```

### Relatório de Vendas Completo (ADO.NET)
```
=== RELATÓRIO VENDAS COMPLETO (ADO.NET) ===
Pedido: PED001 | Cliente: João Silva | Data: 01/10/2025
  Produto: Notebook | Qtde: 1 | Unit: R$3.500,00 | Subtotal: R$3.500,00
  Produto: Smartphone | Qtde: 2 | Unit: R$2.000,00 | Subtotal: R$4.000,00
Pedido: PED002 | Cliente: Maria Souza | Data: 02/10/2025
  Produto: Camiseta | Qtde: 3 | Unit: R$50,00 | Subtotal: R$150,00
  Produto: Livro A | Qtde: 1 | Unit: R$40,00 | Subtotal: R$40,00
```

## Observações

- O arquivo do banco de dados (`loja.db`) é criado automaticamente na primeira execução.
- O projeto pode ser executado em Windows, Linux ou Mac.
- Para resetar o banco, basta apagar o arquivo `loja.db` e rodar novamente.

## Autor

Desenvolvido por Victor Aranda - RM99667 para a disciplina de C# - FIAP 2025.
