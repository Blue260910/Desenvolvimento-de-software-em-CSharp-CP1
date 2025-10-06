
# CheckPoint1 - Sistema de Gestão de Loja Online

Projeto desenvolvido em **C# (.NET 9)** para a disciplina de C# na FIAP, com foco em Entity Framework Core e ADO.NET, simulando um sistema de gestão de loja online completo, multiplataforma e didático.

![.NET](https://img.shields.io/badge/.NET-9.0-blue?style=for-the-badge&logo=dotnet)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-purple?style=for-the-badge&logo=microsoft)
![SQLite](https://img.shields.io/badge/SQLite-Database-green?style=for-the-badge&logo=sqlite)
![ADO.NET](https://img.shields.io/badge/ADO.NET-Data%20Access-orange?style=for-the-badge&logo=microsoft)

---

## 👥 Integrantes | 3ESPY

- **Victor Aranda** - RM99667
- **Felipe Cortez** - RM99750

---

## ✨ Funcionalidades

- Cadastro, listagem e atualização de **categorias**
- Cadastro, listagem e atualização de **produtos** (com controle de estoque)
- Cadastro, listagem e atualização de **clientes** (com validação de e-mail e CPF)
- Cadastro e gerenciamento de **pedidos** (múltiplos itens, controle de estoque, status, cancelamento e devolução)
- **Relatórios e consultas avançadas** (LINQ e SQL):
	- Produtos mais vendidos
	- Faturamento por cliente e por categoria
	- Pedidos por período
	- Produtos sem venda
	- Análise de vendas mensal
	- Dashboard executivo
	- Estoque baixo e produtos zerados
	- Top clientes por valor

---

## 🏗️ Arquitetura e Organização

O projeto segue uma arquitetura simples, separando responsabilidades:

- `Models/` - Entidades do domínio (Categoria, Produto, Cliente, Pedido, PedidoItem)
- `Context/CheckpointContext.cs` - Contexto do Entity Framework (mapeamento, seed, relacionamentos)
- `Services/EntityFrameworkService.cs` - Operações CRUD, consultas LINQ e relatórios usando EF Core
- `Services/AdoNetService.cs` - Relatórios e operações SQL usando ADO.NET puro
- `Enums/StatusPedido.cs` - Enumeração de status dos pedidos
- `Program.cs` - Menu principal, navegação e inicialização

---

## ⚙️ Requisitos e Instalação

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) instalado
- Visual Studio, VS Code ou Rider

### Instalação e Execução

1. Clone o repositório:
	 ```sh
	 git clone https://github.com/Blue260910/Desenvolvimento-de-software-em-CSharp-CP1.git
	 cd Desenvolvimento-de-software-em-CSharp-CP1/CheckPoint1
	 ```
2. Restaure as dependências:
	 ```sh
	 dotnet restore
	 ```
3. Compile o projeto:
	 ```sh
	 dotnet build
	 ```
4. Execute o sistema:
	 ```sh
	 dotnet run
	 ```
5. **Primeira execução:**
	 - O banco de dados `loja.db` e as tabelas serão criados automaticamente.
	 - Dados iniciais de exemplo são inseridos para facilitar testes.

---

## 💻 Exemplos de Uso

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

### Consultas LINQ Avançadas
```
╔══════════════════════════════════════╗
║         CONSULTAS LINQ AVANÇADAS    ║
╠══════════════════════════════════════╣
║ 1 - Produtos mais vendidos          ║
║ 2 - Clientes com mais pedidos       ║
║ 3 - Faturamento por categoria       ║
║ 4 - Pedidos por período             ║
║ 5 - Produtos em estoque baixo       ║
║ 6 - Análise vendas mensal           ║
║ 7 - Top clientes por valor          ║
╚══════════════════════════════════════╝
```

---

## 🛠️ Comandos Úteis

- Limpar o banco de dados (remover arquivo):
	```sh
	del loja.db # Windows
	rm loja.db  # Linux/Mac
	```
- Atualizar dependências:
	```sh
	dotnet restore
	```
- Rodar testes (se houver):
	```sh
	dotnet test
	```

---

## 🐞 Dicas e Troubleshooting

- Se ocorrer erro de conexão, verifique se o arquivo `loja.db` existe e se você tem permissão de escrita na pasta.
- Para resetar o banco, basta apagar o arquivo `loja.db` e rodar novamente.
- O projeto é multiplataforma: funciona em Windows, Linux e Mac.
- Caso precise migrar o banco manualmente, utilize:
	```sh
	dotnet ef database update
	```

---

## 🤝 Contribuindo

Pull requests são bem-vindos! Para contribuir:

1. Fork este repositório
2. Crie uma branch: `git checkout -b minha-feature`
3. Faça suas alterações e commit: `git commit -m 'Minha contribuição'`
4. Push para sua branch: `git push origin minha-feature`
5. Abra um Pull Request

---

## 📄 Licença

Este projeto é apenas para fins educacionais e não possui licença comercial.

---

## 👨‍💻 Autor

Desenvolvido por Victor Aranda - RM99667 para a disciplina de C# - FIAP 2025.
