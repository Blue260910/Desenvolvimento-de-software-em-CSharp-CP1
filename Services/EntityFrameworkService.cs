using Microsoft.EntityFrameworkCore;
using CheckPoint1.Models;
using System.Text.RegularExpressions;

namespace CheckPoint1.Services;

// Serviço para operações de banco de dados usando Entity Framework Core (CRUD, LINQ e relatórios).
public class EntityFrameworkService
{
    #region Propriedades e Construtor

    private readonly CheckpointContext _context;

    // Inicializa o contexto do Entity Framework
    public EntityFrameworkService()
    {
        _context = new CheckpointContext();
    }

    #endregion
    #region CRUD de Categorias
    // CRUD para categorias

    // Cria uma nova categoria (validação básica)
    public void CriarCategoria()
    {
        Console.WriteLine("=== CRIAR CATEGORIA ===");
        
        Console.Write("Nome da categoria: ");
        var nome = Console.ReadLine();
        
        // Validação obrigatória do nome
        if (string.IsNullOrWhiteSpace(nome))
        {
            Console.WriteLine("Nome é obrigatório!");
            return;
        }
        
        Console.Write("Descrição (opcional): ");
        var descricao = Console.ReadLine();
        
        // Criação da entidade categoria
        var categoria = new Categoria
        {
            Nome = nome,
            Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao,
            DataCriacao = DateTime.Now
        };
        
        try
        {
            _context.Categorias.Add(categoria);
            _context.SaveChanges();
            Console.WriteLine($"Categoria '{nome}' criada com sucesso! ID: {categoria.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar categoria: {ex.Message}");
        }
    }

    // Lista categorias com contagem de produtos (Include para performance)
    public void ListarCategorias()
    {
        Console.WriteLine("=== CATEGORIAS ===");
        
        // Query com Include para carregar produtos relacionados
        var categorias = _context.Categorias
            .Include(c => c.Produtos)
            .OrderBy(c => c.Nome)
            .ToList();
        
        if (!categorias.Any())
        {
            Console.WriteLine("Nenhuma categoria encontrada.");
            return;
        }
        
        // Formatação em tabela para melhor visualização
        Console.WriteLine(new string('-', 70));
        Console.WriteLine("| {0,-5} | {1,-20} | {2,-25} | {3,-10} |", 
            "ID", "Nome", "Descrição", "Produtos");
        Console.WriteLine(new string('-', 70));
        
        foreach (var categoria in categorias)
        {
            Console.WriteLine("| {0,-5} | {1,-20} | {2,-25} | {3,-10} |",
                categoria.Id,
                categoria.Nome,
                categoria.Descricao?.Substring(0, Math.Min(categoria.Descricao.Length, 25)) ?? "",
                categoria.Produtos.Count);
        }
        Console.WriteLine(new string('-', 70));
    }

    #endregion

    #region CRUD de Produtos
    // CRUD para produtos

    // Cria novo produto (validações de categoria e dados obrigatórios)
    public void CriarProduto()
    {
        Console.WriteLine("=== CRIAR PRODUTO ===");
        
        // Buscar e exibir categorias disponíveis
        var categorias = _context.Categorias.OrderBy(c => c.Nome).ToList();
        
        if (!categorias.Any())
        {
            Console.WriteLine("Nenhuma categoria encontrada! Crie uma categoria primeiro.");
            return;
        }
        
        Console.WriteLine("Categorias disponíveis:");
        foreach (var cat in categorias)
        {
            Console.WriteLine($"{cat.Id} - {cat.Nome}");
        }
        
        Console.Write("ID da categoria: ");
        if (!int.TryParse(Console.ReadLine(), out int categoriaId))
        {
            Console.WriteLine("ID inválido!");
            return;
        }
        
        // Validação de existência da categoria
        if (!_context.Categorias.Any(c => c.Id == categoriaId))
        {
            Console.WriteLine("Categoria não encontrada!");
            return;
        }
        
        Console.Write("Nome do produto: ");
        var nome = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(nome))
        {
            Console.WriteLine("Nome é obrigatório!");
            return;
        }
        
        Console.Write("Descrição (opcional): ");
        var descricao = Console.ReadLine();
        
        // Validação de preço
        Console.Write("Preço: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal preco) || preco <= 0)
        {
            Console.WriteLine("Preço inválido!");
            return;
        }
        
        // Validação de estoque
        Console.Write("Estoque: ");
        if (!int.TryParse(Console.ReadLine(), out int estoque) || estoque < 0)
        {
            Console.WriteLine("Estoque inválido!");
            return;
        }
        
        // Criação da entidade produto
        var produto = new Produto
        {
            Nome = nome,
            Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao,
            Preco = preco,
            Estoque = estoque,
            CategoriaId = categoriaId,
            DataCriacao = DateTime.Now,
            Ativo = true
        };
        
        try
        {
            _context.Produtos.Add(produto);
            _context.SaveChanges();
            Console.WriteLine($"Produto '{nome}' criado com sucesso! ID: {produto.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar produto: {ex.Message}");
        }
    }

    // Lista produtos ativos com categoria (Include para performance)
    public void ListarProdutos()
    {
        Console.WriteLine("=== PRODUTOS ===");
        
        // Query com Include para carregar categoria relacionada
        var produtos = _context.Produtos
            .Include(p => p.Categoria)
            .Where(p => p.Ativo)
            .OrderBy(p => p.Nome)
            .ToList();
        
        if (!produtos.Any())
        {
            Console.WriteLine("Nenhum produto encontrado.");
            return;
        }
        
        // Formatação em tabela
        Console.WriteLine(new string('-', 100));
        Console.WriteLine("| {0,-5} | {1,-20} | {2,-15} | {3,-10} | {4,-8} | {5,-15} |", 
            "ID", "Nome", "Categoria", "Preço", "Estoque", "Data Criação");
        Console.WriteLine(new string('-', 100));
        
        foreach (var produto in produtos)
        {
            Console.WriteLine("| {0,-5} | {1,-20} | {2,-15} | {3,-10:C} | {4,-8} | {5,-15:dd/MM/yyyy} |",
                produto.Id,
                produto.Nome.Length > 20 ? produto.Nome.Substring(0, 17) + "..." : produto.Nome,
                produto.Categoria.Nome.Length > 15 ? produto.Categoria.Nome.Substring(0, 12) + "..." : produto.Categoria.Nome,
                produto.Preco,
                produto.Estoque,
                produto.DataCriacao);
        }
        Console.WriteLine(new string('-', 100));
    }

    // Atualiza dados de um produto existente (atualização parcial)

    /// <summary>
    /// Atualiza dados de um produto existente
    /// Permite atualização parcial dos campos
    /// </summary>
    public void AtualizarProduto()
    {
        Console.WriteLine("=== ATUALIZAR PRODUTO ===");
        
        // Exibir produtos para seleção
        ListarProdutos();
        
        Console.Write("Digite o ID do produto para atualizar: ");
        if (!int.TryParse(Console.ReadLine(), out int produtoId))
        {
            Console.WriteLine("ID inválido!");
            return;
        }
        
        // Buscar produto por ID
        var produto = _context.Produtos.FirstOrDefault(p => p.Id == produtoId && p.Ativo);
        
        if (produto == null)
        {
            Console.WriteLine("Produto não encontrado!");
            return;
        }
        
        // Atualização parcial - só altera se usuário informar novo valor
        Console.WriteLine($"Produto atual: {produto.Nome}");
        Console.Write($"Novo nome (atual: {produto.Nome}): ");
        var novoNome = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(novoNome))
            produto.Nome = novoNome;
        
        Console.Write($"Nova descrição (atual: {produto.Descricao ?? "Sem descrição"}): ");
        var novaDescricao = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(novaDescricao))
            produto.Descricao = novaDescricao;
        
        Console.Write($"Novo preço (atual: {produto.Preco:C}): ");
        if (decimal.TryParse(Console.ReadLine(), out decimal novoPreco) && novoPreco > 0)
            produto.Preco = novoPreco;
        
        Console.Write($"Novo estoque (atual: {produto.Estoque}): ");
        if (int.TryParse(Console.ReadLine(), out int novoEstoque) && novoEstoque >= 0)
            produto.Estoque = novoEstoque;
        
        try
        {
            _context.SaveChanges();
            Console.WriteLine("Produto atualizado com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao atualizar produto: {ex.Message}");
        }
    }

    #endregion
    #region CRUD de Clientes
    // CRUD para clientes

    // Cria novo cliente (validação de email único e CPF)
    public void CriarCliente()
    {
        Console.WriteLine("=== CRIAR CLIENTE ===");
        
        Console.Write("Nome do cliente: ");
        var nome = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(nome))
        {
            Console.WriteLine("Nome é obrigatório!");
            return;
        }
        
        Console.Write("Email: ");
        var email = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("Email é obrigatório!");
            return;
        }
        
        // Validação de email único no banco
        if (_context.Clientes.Any(c => c.Email == email))
        {
            Console.WriteLine("Email já cadastrado!");
            return;
        }
        
        Console.Write("CPF (somente números): ");
        var cpf = Console.ReadLine();
        
        // Validação e formatação do CPF - apenas números
        if (!string.IsNullOrWhiteSpace(cpf))
        {
            cpf = Regex.Replace(cpf, @"[^0-9]", ""); // Remove tudo que não é número
            if (cpf.Length != 11)
            {
                Console.WriteLine("CPF deve ter 11 dígitos!");
                return;
            }
        }
        
        // Campos opcionais
        Console.Write("Telefone (opcional): ");
        var telefone = Console.ReadLine();
        
        Console.Write("Endereço (opcional): ");
        var endereco = Console.ReadLine();
        
        Console.Write("Cidade (opcional): ");
        var cidade = Console.ReadLine();
        
        Console.Write("Estado (opcional): ");
        var estado = Console.ReadLine();
        
        Console.Write("CEP (opcional): ");
        var cep = Console.ReadLine();
        
        // Criação da entidade cliente
        var cliente = new Cliente
        {
            Nome = nome,
            Email = email,
            CPF = string.IsNullOrWhiteSpace(cpf) ? null : cpf,
            Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone,
            Endereco = string.IsNullOrWhiteSpace(endereco) ? null : endereco,
            Cidade = string.IsNullOrWhiteSpace(cidade) ? null : cidade,
            Estado = string.IsNullOrWhiteSpace(estado) ? null : estado,
            CEP = string.IsNullOrWhiteSpace(cep) ? null : cep,
            DataCadastro = DateTime.Now,
            Ativo = true
        };
        
        try
        {
            _context.Clientes.Add(cliente);
            _context.SaveChanges();
            Console.WriteLine($"Cliente '{nome}' criado com sucesso! ID: {cliente.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar cliente: {ex.Message}");
        }
    }

    // Lista clientes ativos com contagem de pedidos (Include para performance)

    /// <summary>
    /// Lista todos os clientes ativos com contagem de pedidos
    /// Utiliza Include para carregar pedidos relacionados
    /// </summary>
    public void ListarClientes()
    {
        Console.WriteLine("=== CLIENTES ===");
        
        // Query com Include para carregar pedidos relacionados
        var clientes = _context.Clientes
            .Include(c => c.Pedidos)
            .Where(c => c.Ativo)
            .OrderBy(c => c.Nome)
            .ToList();
        
        if (!clientes.Any())
        {
            Console.WriteLine("Nenhum cliente encontrado.");
            return;
        }
        
        // Formatação em tabela
        Console.WriteLine(new string('-', 120));
        Console.WriteLine("| {0,-5} | {1,-20} | {2,-25} | {3,-15} | {4,-8} | {5,-15} | {6,-8} |", 
            "ID", "Nome", "Email", "Telefone", "Pedidos", "Data Cadastro", "Estado");
        Console.WriteLine(new string('-', 120));
        
        foreach (var cliente in clientes)
        {
            Console.WriteLine("| {0,-5} | {1,-20} | {2,-25} | {3,-15} | {4,-8} | {5,-15:dd/MM/yyyy} | {6,-8} |",
                cliente.Id,
                cliente.Nome.Length > 20 ? cliente.Nome.Substring(0, 17) + "..." : cliente.Nome,
                cliente.Email.Length > 25 ? cliente.Email.Substring(0, 22) + "..." : cliente.Email,
                cliente.Telefone ?? "",
                cliente.Pedidos.Count,
                cliente.DataCadastro,
                cliente.Estado ?? "");
        }
        Console.WriteLine(new string('-', 120));
    }

    // Atualiza dados de cliente existente (validação de email único)
    public void AtualizarCliente()
    {
        Console.WriteLine("=== ATUALIZAR CLIENTE ===");
        
        // Exibir clientes para seleção
        ListarClientes();
        
        Console.Write("Digite o ID do cliente para atualizar: ");
        if (!int.TryParse(Console.ReadLine(), out int clienteId))
        {
            Console.WriteLine("ID inválido!");
            return;
        }
        
        // Buscar cliente por ID
        var cliente = _context.Clientes.FirstOrDefault(c => c.Id == clienteId && c.Ativo);
        
        if (cliente == null)
        {
            Console.WriteLine("Cliente não encontrado!");
            return;
        }
        
        Console.WriteLine($"Cliente atual: {cliente.Nome}");
        
        // Atualização parcial dos campos
        Console.Write($"Novo nome (atual: {cliente.Nome}): ");
        var novoNome = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(novoNome))
            cliente.Nome = novoNome;
        
        Console.Write($"Novo email (atual: {cliente.Email}): ");
        var novoEmail = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(novoEmail))
        {
            // Validação de email único (exceto o próprio cliente)
            if (_context.Clientes.Any(c => c.Email == novoEmail && c.Id != clienteId))
            {
                Console.WriteLine("Email já cadastrado para outro cliente!");
                return;
            }
            cliente.Email = novoEmail;
        }
        
        Console.Write($"Novo telefone (atual: {cliente.Telefone ?? "Não informado"}): ");
        var novoTelefone = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(novoTelefone))
            cliente.Telefone = novoTelefone;
        
        Console.Write($"Novo endereço (atual: {cliente.Endereco ?? "Não informado"}): ");
        var novoEndereco = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(novoEndereco))
            cliente.Endereco = novoEndereco;
        
        Console.Write($"Nova cidade (atual: {cliente.Cidade ?? "Não informado"}): ");
        var novaCidade = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(novaCidade))
            cliente.Cidade = novaCidade;
        
        Console.Write($"Novo estado (atual: {cliente.Estado ?? "Não informado"}): ");
        var novoEstado = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(novoEstado))
            cliente.Estado = novoEstado;
        
        try
        {
            _context.SaveChanges();
            Console.WriteLine("Cliente atualizado com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao atualizar cliente: {ex.Message}");
        }
    }

    #endregion
    #region CRUD de Pedidos
    // CRUD para pedidos

    // Cria pedido completo com múltiplos itens (validação de estoque e número automático)

    /// <summary>
    /// Cria um pedido completo com múltiplos itens
    /// Inclui validação de estoque e geração automática de número
    /// </summary>
    public void CriarPedido()
    {
        Console.WriteLine("=== CRIAR PEDIDO ===");
        
        // Buscar e exibir clientes disponíveis
        var clientes = _context.Clientes.Where(c => c.Ativo).OrderBy(c => c.Nome).ToList();
        
        if (!clientes.Any())
        {
            Console.WriteLine("Nenhum cliente encontrado! Cadastre um cliente primeiro.");
            return;
        }
        
        Console.WriteLine("Clientes disponíveis:");
        foreach (var cli in clientes)
        {
            Console.WriteLine($"{cli.Id} - {cli.Nome} ({cli.Email})");
        }
        
        Console.Write("ID do cliente: ");
        if (!int.TryParse(Console.ReadLine(), out int clienteId))
        {
            Console.WriteLine("ID inválido!");
            return;
        }
        
        // Validar existência do cliente
        var cliente = _context.Clientes.FirstOrDefault(c => c.Id == clienteId && c.Ativo);
        if (cliente == null)
        {
            Console.WriteLine("Cliente não encontrado!");
            return;
        }
        
        // Geração automática do número do pedido baseado na data/hora
        var numeroPedido = $"PED{DateTime.Now:yyyyMMddHHmmss}";
        
        // Criação do pedido master
        var pedido = new Pedido
        {
            NumeroPedido = numeroPedido,
            DataPedido = DateTime.Now,
            Status = StatusPedido.Pendente,
            ClienteId = clienteId,
            ValorTotal = 0,
            Itens = new List<PedidoItem>()
        };
        
        _context.Pedidos.Add(pedido);
        _context.SaveChanges(); // Salvar para obter o ID do pedido
        
        decimal valorTotal = 0;
        bool continuarAdicionando = true;
        
        // Loop para adicionar múltiplos itens ao pedido
        while (continuarAdicionando)
        {
            // Buscar produtos com estoque disponível
            var produtos = _context.Produtos
                .Include(p => p.Categoria)
                .Where(p => p.Ativo && p.Estoque > 0)
                .OrderBy(p => p.Nome)
                .ToList();
            
            if (!produtos.Any())
            {
                Console.WriteLine("Nenhum produto com estoque disponível!");
                break;
            }
            
            Console.WriteLine("\nProdutos disponíveis:");
            foreach (var prod in produtos)
            {
                Console.WriteLine($"{prod.Id} - {prod.Nome} - {prod.Preco:C} (Estoque: {prod.Estoque})");
            }
            
            Console.Write("ID do produto (0 para finalizar): ");
            if (!int.TryParse(Console.ReadLine(), out int produtoId) || produtoId == 0)
            {
                continuarAdicionando = false;
                continue;
            }
            
            var produto = produtos.FirstOrDefault(p => p.Id == produtoId);
            if (produto == null)
            {
                Console.WriteLine("Produto não encontrado ou sem estoque!");
                continue;
            }
            
            Console.Write("Quantidade: ");
            if (!int.TryParse(Console.ReadLine(), out int quantidade) || quantidade <= 0)
            {
                Console.WriteLine("Quantidade inválida!");
                continue;
            }
            
            // Validação crítica de estoque disponível
            if (quantidade > produto.Estoque)
            {
                Console.WriteLine($"Estoque insuficiente! Disponível: {produto.Estoque}");
                continue;
            }
            
            // Criação do item do pedido
            var item = new PedidoItem
            {
                PedidoId = pedido.Id,
                ProdutoId = produtoId,
                Quantidade = quantidade,
                PrecoUnitario = produto.Preco
            };
            
            _context.PedidoItens.Add(item);
            
            // Atualização do estoque (redução)
            produto.Estoque -= quantidade;
            
            valorTotal += item.Subtotal;
            Console.WriteLine($"Item adicionado! Subtotal: {item.Subtotal:C}");
        }
        
        // Atualização do valor total do pedido
        pedido.ValorTotal = valorTotal;
        
        try
        {
            _context.SaveChanges();
            Console.WriteLine($"Pedido {numeroPedido} criado com sucesso! Valor total: {valorTotal:C}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar pedido: {ex.Message}");
        }
    }

    // Lista pedidos com detalhes completos (Include aninhado)

        public void ListarPedidos()
        {
            Console.WriteLine("=== PEDIDOS ===");
            
            var pedidos = _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                .OrderByDescending(p => p.DataPedido)
                .ToList();
            
            if (!pedidos.Any())
            {
                Console.WriteLine("Nenhum pedido encontrado.");
                return;
            }
            
            foreach (var pedido in pedidos)
            {
                Console.WriteLine(new string('=', 80));
                Console.WriteLine($"Pedido: {pedido.NumeroPedido} | Cliente: {pedido.Cliente.Nome}");
                Console.WriteLine($"Data: {pedido.DataPedido:dd/MM/yyyy HH:mm} | Status: {pedido.Status}");
                Console.WriteLine($"Valor Total: {pedido.ValorTotal:C}");
                Console.WriteLine("Itens:");
                
                foreach (var item in pedido.Itens)
                {
                    Console.WriteLine($"  - {item.Produto.Nome} | Qtd: {item.Quantidade} | Preço: {item.PrecoUnitario:C} | Subtotal: {item.Subtotal:C}");
                }
                Console.WriteLine();
            }
        }

        public void AtualizarStatusPedido()
        {
            Console.WriteLine("=== ATUALIZAR STATUS PEDIDO ===");
            
            var pedidos = _context.Pedidos
                .Include(p => p.Cliente)
                .Where(p => p.Status != StatusPedido.Cancelado)
                .OrderByDescending(p => p.DataPedido)
                .ToList();
            
            if (!pedidos.Any())
            {
                Console.WriteLine("Nenhum pedido disponível para atualização.");
                return;
            }
            
            Console.WriteLine("Pedidos disponíveis:");
            foreach (var ped in pedidos)
            {
                Console.WriteLine($"{ped.Id} - {ped.NumeroPedido} | {ped.Cliente.Nome} | Status: {ped.Status}");
            }
            
            Console.Write("ID do pedido: ");
            if (!int.TryParse(Console.ReadLine(), out int pedidoId))
            {
                Console.WriteLine("ID inválido!");
                return;
            }
            
            var pedido = pedidos.FirstOrDefault(p => p.Id == pedidoId);
            if (pedido == null)
            {
                Console.WriteLine("Pedido não encontrado!");
                return;
            }
            
            // Mostrar status disponíveis
            Console.WriteLine("Status disponíveis:");
            foreach (StatusPedido status in Enum.GetValues<StatusPedido>())
            {
                if (status != StatusPedido.Cancelado) // Cancelamento é feito em método específico
                {
                    Console.WriteLine($"{(int)status} - {status}");
                }
            }
            
            Console.Write("Novo status: ");
            if (!int.TryParse(Console.ReadLine(), out int novoStatusInt))
            {
                Console.WriteLine("Status inválido!");
                return;
            }
            
            if (!Enum.IsDefined(typeof(StatusPedido), novoStatusInt) || novoStatusInt == (int)StatusPedido.Cancelado)
            {
                Console.WriteLine("Status inválido!");
                return;
            }
            
            var novoStatus = (StatusPedido)novoStatusInt;
            
            // Validar transições válidas
            var statusAtual = pedido.Status;
            var transicoesValidas = new Dictionary<StatusPedido, StatusPedido[]>
            {
                { StatusPedido.Pendente, new[] { StatusPedido.Confirmado } },
                { StatusPedido.Confirmado, new[] { StatusPedido.EmAndamento } },
                { StatusPedido.EmAndamento, new[] { StatusPedido.Entregue } },
                { StatusPedido.Entregue, new StatusPedido[] { } } // Não pode mudar de entregue
            };
            
            if (transicoesValidas.ContainsKey(statusAtual) && 
                !transicoesValidas[statusAtual].Contains(novoStatus))
            {
                Console.WriteLine($"Transição inválida! De {statusAtual} não é possível ir para {novoStatus}");
                return;
            }
            
            pedido.Status = novoStatus;
            
            try
            {
                _context.SaveChanges();
                Console.WriteLine($"Status do pedido {pedido.NumeroPedido} atualizado para {novoStatus}!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar status: {ex.Message}");
            }
        }

        public void CancelarPedido()
        {
            Console.WriteLine("=== CANCELAR PEDIDO ===");
            
            var pedidos = _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                .Where(p => p.Status == StatusPedido.Pendente || p.Status == StatusPedido.Confirmado)
                .OrderByDescending(p => p.DataPedido)
                .ToList();
            
            if (!pedidos.Any())
            {
                Console.WriteLine("Nenhum pedido disponível para cancelamento.");
                return;
            }
            
            Console.WriteLine("Pedidos disponíveis para cancelamento:");
            foreach (var ped in pedidos)
            {
                Console.WriteLine($"{ped.Id} - {ped.NumeroPedido} | {ped.Cliente.Nome} | Status: {ped.Status} | Valor: {ped.ValorTotal:C}");
            }
            
            Console.Write("ID do pedido: ");
            if (!int.TryParse(Console.ReadLine(), out int pedidoId))
            {
                Console.WriteLine("ID inválido!");
                return;
            }
            
            var pedido = pedidos.FirstOrDefault(p => p.Id == pedidoId);
            if (pedido == null)
            {
                Console.WriteLine("Pedido não encontrado ou não pode ser cancelado!");
                return;
            }
            
            Console.WriteLine($"Pedido: {pedido.NumeroPedido}");
            Console.WriteLine($"Cliente: {pedido.Cliente.Nome}");
            Console.WriteLine($"Valor: {pedido.ValorTotal:C}");
            Console.WriteLine("Itens que terão estoque devolvido:");
            
            foreach (var item in pedido.Itens)
            {
                Console.WriteLine($"- {item.Produto.Nome}: {item.Quantidade} unidades");
            }
            
            Console.Write("Confirma o cancelamento? (S/N): ");
            if (Console.ReadLine()?.ToUpper() != "S")
            {
                Console.WriteLine("Cancelamento abortado!");
                return;
            }
            
            try
            {
                // Devolver estoque dos produtos
                foreach (var item in pedido.Itens)
                {
                    item.Produto.Estoque += item.Quantidade;
                }
                
                // Atualizar status do pedido
                pedido.Status = StatusPedido.Cancelado;
                
                _context.SaveChanges();
                Console.WriteLine($"Pedido {pedido.NumeroPedido} cancelado com sucesso!");
                Console.WriteLine("Estoque dos produtos foi devolvido.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao cancelar pedido: {ex.Message}");
            }
        }

    // ========== CONSULTAS LINQ AVANÇADAS ========== 

        #endregion
    #region Consultas LINQ Avançadas
    // Consultas LINQ avançadas: análises e relatórios de negócio

    // Menu para consultas LINQ avançadas
        public void ConsultasAvancadas()
        {
            Console.WriteLine("=== CONSULTAS LINQ ===");
            Console.WriteLine("1. Produtos mais vendidos");
            Console.WriteLine("2. Clientes com mais pedidos");
            Console.WriteLine("3. Faturamento por categoria");
            Console.WriteLine("4. Pedidos por período");
            Console.WriteLine("5. Produtos em estoque baixo");
            Console.WriteLine("6. Análise vendas mensal");
            Console.WriteLine("7. Top clientes por valor");

            var opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1": ProdutosMaisVendidos(); break;
                case "2": ClientesComMaisPedidos(); break;
                case "3": FaturamentoPorCategoria(); break;
                case "4": PedidosPorPeriodo(); break;
                case "5": ProdutosEstoqueBaixo(); break;
                case "6": AnaliseVendasMensal(); break;
                case "7": TopClientesPorValor(); break;
            }
        }

    // Produtos mais vendidos (GroupBy, Sum, OrderBy, Include aninhado)
        private void ProdutosMaisVendidos()
        {
            Console.WriteLine("=== PRODUTOS MAIS VENDIDOS ===");
            
            var produtosMaisVendidos = _context.PedidoItens
                .Include(pi => pi.Produto)
                    .ThenInclude(p => p.Categoria)
                .ToList() // Força execução no cliente para trabalhar com decimais
                .GroupBy(pi => new { pi.ProdutoId, pi.Produto.Nome, CategoriaNome = pi.Produto.Categoria.Nome })
                .Select(g => new
                {
                    ProdutoId = g.Key.ProdutoId,
                    NomeProduto = g.Key.Nome,
                    Categoria = g.Key.CategoriaNome,
                    QuantidadeVendida = g.Sum(pi => pi.Quantidade),
                    ValorTotal = g.Sum(pi => pi.Quantidade * pi.PrecoUnitario)
                })
                .OrderByDescending(x => x.QuantidadeVendida)
                .Take(10)
                .ToList();
            
            if (!produtosMaisVendidos.Any())
            {
                Console.WriteLine("Nenhuma venda encontrada.");
                return;
            }
            
            Console.WriteLine(new string('-', 80));
            Console.WriteLine("| {0,-25} | {1,-15} | {2,-8} | {3,-15} |", 
                "Produto", "Categoria", "Qtd Vend", "Valor Total");
            Console.WriteLine(new string('-', 80));
            
            foreach (var produto in produtosMaisVendidos)
            {
                Console.WriteLine("| {0,-25} | {1,-15} | {2,-8} | {3,-15:C} |",
                    produto.NomeProduto.Length > 25 ? produto.NomeProduto.Substring(0, 22) + "..." : produto.NomeProduto,
                    produto.Categoria.Length > 15 ? produto.Categoria.Substring(0, 12) + "..." : produto.Categoria,
                    produto.QuantidadeVendida,
                    produto.ValorTotal);
            }
            Console.WriteLine(new string('-', 80));
        }

    // Clientes com mais pedidos (análise e ticket médio)

        private void ClientesComMaisPedidos()
        {
            Console.WriteLine("=== CLIENTES COM MAIS PEDIDOS ===");
            
            var clientesComMaisPedidos = _context.Clientes
                .Include(c => c.Pedidos)
                .Where(c => c.Ativo)
                .ToList() // Força execução no cliente para trabalhar com decimais
                .Select(c => new
                {
                    ClienteId = c.Id,
                    Nome = c.Nome,
                    Email = c.Email,
                    QuantidadePedidos = c.Pedidos.Count,
                    ValorTotal = c.Pedidos.Sum(p => p.ValorTotal),
                    TicketMedio = c.Pedidos.Any() ? c.Pedidos.Average(p => p.ValorTotal) : 0
                })
                .OrderByDescending(x => x.QuantidadePedidos)
                .Take(10)
                .ToList();
            
            if (!clientesComMaisPedidos.Any())
            {
                Console.WriteLine("Nenhum cliente encontrado.");
                return;
            }
            
            Console.WriteLine(new string('-', 90));
            Console.WriteLine("| {0,-25} | {1,-8} | {2,-15} | {3,-12} |", 
                "Cliente", "Pedidos", "Valor Total", "Ticket Médio");
            Console.WriteLine(new string('-', 90));
            
            foreach (var cliente in clientesComMaisPedidos)
            {
                Console.WriteLine("| {0,-25} | {1,-8} | {2,-15:C} | {3,-12:C} |",
                    cliente.Nome.Length > 25 ? cliente.Nome.Substring(0, 22) + "..." : cliente.Nome,
                    cliente.QuantidadePedidos,
                    cliente.ValorTotal,
                    cliente.TicketMedio);
            }
            Console.WriteLine(new string('-', 90));
        }

        private void FaturamentoPorCategoria()
        {
            Console.WriteLine("=== FATURAMENTO POR CATEGORIA ===");
            
            var faturamentoPorCategoria = _context.PedidoItens
                .Include(pi => pi.Produto)
                    .ThenInclude(p => p.Categoria)
                .ToList() // Força execução no cliente para trabalhar com decimais
                .GroupBy(pi => pi.Produto.Categoria)
                .Select(g => new
                {
                    Categoria = g.Key.Nome,
                    QuantidadeProdutosVendidos = g.Sum(pi => pi.Quantidade),
                    ValorTotalVendido = g.Sum(pi => pi.Quantidade * pi.PrecoUnitario),
                    TicketMedio = g.Average(pi => pi.Quantidade * pi.PrecoUnitario)
                })
                .OrderByDescending(x => x.ValorTotalVendido)
                .ToList();
            
            if (!faturamentoPorCategoria.Any())
            {
                Console.WriteLine("Nenhuma venda encontrada.");
                return;
            }
            
            Console.WriteLine(new string('-', 80));
            Console.WriteLine("| {0,-15} | {1,-10} | {2,-15} | {3,-12} |", 
                "Categoria", "Qtd Vendida", "Valor Total", "Ticket Médio");
            Console.WriteLine(new string('-', 80));
            
            foreach (var categoria in faturamentoPorCategoria)
            {
                Console.WriteLine("| {0,-15} | {1,-10} | {2,-15:C} | {3,-12:C} |",
                    categoria.Categoria.Length > 15 ? categoria.Categoria.Substring(0, 12) + "..." : categoria.Categoria,
                    categoria.QuantidadeProdutosVendidos,
                    categoria.ValorTotalVendido,
                    categoria.TicketMedio);
            }
            Console.WriteLine(new string('-', 80));
        }

        private void PedidosPorPeriodo()
        {
            Console.WriteLine("=== PEDIDOS POR PERÍODO ===");
            
            Console.Write("Data início (dd/MM/yyyy): ");
            if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dataInicio))
            {
                Console.WriteLine("Data inválida!");
                return;
            }
            
            Console.Write("Data fim (dd/MM/yyyy): ");
            if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dataFim))
            {
                Console.WriteLine("Data inválida!");
                return;
            }
            
            if (dataInicio > dataFim)
            {
                Console.WriteLine("Data início deve ser menor que data fim!");
                return;
            }
            
            var pedidosPorPeriodo = _context.Pedidos
                .Where(p => p.DataPedido.Date >= dataInicio.Date && 
                           p.DataPedido.Date <= dataFim.Date &&
                           p.Status != StatusPedido.Cancelado)
                .ToList() // Força execução no cliente para trabalhar com decimais
                .GroupBy(p => p.DataPedido.Date)
                .Select(g => new
                {
                    Data = g.Key,
                    QuantidadePedidos = g.Count(),
                    ValorTotal = g.Sum(p => p.ValorTotal),
                    TicketMedio = g.Average(p => p.ValorTotal)
                })
                .OrderBy(x => x.Data)
                .ToList();
            
            if (!pedidosPorPeriodo.Any())
            {
                Console.WriteLine("Nenhum pedido encontrado no período.");
                return;
            }
            
            Console.WriteLine(new string('-', 70));
            Console.WriteLine("| {0,-12} | {1,-8} | {2,-15} | {3,-12} |", 
                "Data", "Pedidos", "Valor Total", "Ticket Médio");
            Console.WriteLine(new string('-', 70));
            
            foreach (var dia in pedidosPorPeriodo)
            {
                Console.WriteLine("| {0,-12:dd/MM/yyyy} | {1,-8} | {2,-15:C} | {3,-12:C} |",
                    dia.Data,
                    dia.QuantidadePedidos,
                    dia.ValorTotal,
                    dia.TicketMedio);
            }
            Console.WriteLine(new string('-', 70));
            
            var totalPedidos = pedidosPorPeriodo.Sum(x => x.QuantidadePedidos);
            var valorTotalPeriodo = pedidosPorPeriodo.Sum(x => x.ValorTotal);
            Console.WriteLine($"Total do período: {totalPedidos} pedidos - {valorTotalPeriodo:C}");
        }

        private void ProdutosEstoqueBaixo()
        {
            Console.WriteLine("=== PRODUTOS EM ESTOQUE BAIXO ===");
            
            var produtosEstoqueBaixo = _context.Produtos
                .Include(p => p.Categoria)
                .Where(p => p.Ativo && p.Estoque < 20)
                .OrderBy(p => p.Estoque)
                .Select(p => new
                {
                    p.Id,
                    p.Nome,
                    Categoria = p.Categoria.Nome,
                    p.Estoque,
                    p.Preco,
                    ValorEstoque = p.Estoque * p.Preco
                })
                .ToList();
            
            if (!produtosEstoqueBaixo.Any())
            {
                Console.WriteLine("Nenhum produto com estoque baixo encontrado.");
                return;
            }
            
            Console.WriteLine(new string('-', 90));
            Console.WriteLine("| {0,-5} | {1,-20} | {2,-15} | {3,-7} | {4,-10} | {5,-12} |", 
                "ID", "Produto", "Categoria", "Estoque", "Preço", "Valor Estoque");
            Console.WriteLine(new string('-', 90));
            
            foreach (var produto in produtosEstoqueBaixo)
            {
                Console.WriteLine("| {0,-5} | {1,-20} | {2,-15} | {3,-7} | {4,-10:C} | {5,-12:C} |",
                    produto.Id,
                    produto.Nome.Length > 20 ? produto.Nome.Substring(0, 17) + "..." : produto.Nome,
                    produto.Categoria.Length > 15 ? produto.Categoria.Substring(0, 12) + "..." : produto.Categoria,
                    produto.Estoque,
                    produto.Preco,
                    produto.ValorEstoque);
            }
            Console.WriteLine(new string('-', 90));
            
            var totalValorParado = produtosEstoqueBaixo.Sum(p => p.ValorEstoque);
            Console.WriteLine($"Valor total parado em estoque baixo: {totalValorParado:C}");
        }

        private void AnaliseVendasMensal()
        {
            Console.WriteLine("=== ANÁLISE VENDAS MENSAL ===");
            
            var vendasMensais = _context.Pedidos
                .Include(p => p.Itens)
                .Where(p => p.Status != StatusPedido.Cancelado)
                .ToList() // Força execução no cliente para trabalhar com decimais
                .GroupBy(p => new { p.DataPedido.Year, p.DataPedido.Month })
                .Select(g => new
                {
                    Ano = g.Key.Year,
                    Mes = g.Key.Month,
                    QuantidadeVendida = g.Sum(p => p.Itens.Sum(i => i.Quantidade)),
                    Faturamento = g.Sum(p => p.ValorTotal)
                })
                .OrderBy(x => x.Ano).ThenBy(x => x.Mes)
                .ToList();
            
            if (!vendasMensais.Any())
            {
                Console.WriteLine("Nenhuma venda encontrada.");
                return;
            }
            
            Console.WriteLine(new string('-', 80));
            Console.WriteLine("| {0,-10} | {1,-12} | {2,-15} | {3,-15} |", 
                "Mês/Ano", "Qtd Vendida", "Faturamento", "Crescimento");
            Console.WriteLine(new string('-', 80));
            
            decimal faturamentoAnterior = 0;
            bool primeiro = true;
            
            foreach (var venda in vendasMensais)
            {
                var crescimento = primeiro ? 0 : 
                    faturamentoAnterior == 0 ? 0 : 
                    ((venda.Faturamento - faturamentoAnterior) / faturamentoAnterior) * 100;
                
                Console.WriteLine("| {0,-10} | {1,-12} | {2,-15:C} | {3,-15:F1}% |",
                    $"{venda.Mes:00}/{venda.Ano}",
                    venda.QuantidadeVendida,
                    venda.Faturamento,
                    crescimento);
                
                faturamentoAnterior = venda.Faturamento;
                primeiro = false;
            }
            Console.WriteLine(new string('-', 80));
        }

        private void TopClientesPorValor()
        {
            Console.WriteLine("=== TOP CLIENTES POR VALOR ===");
            
            var topClientes = _context.Clientes
                .Include(c => c.Pedidos)
                .Where(c => c.Ativo && c.Pedidos.Any())
                .ToList() // Força execução no cliente para trabalhar com decimais
                .Select(c => new
                {
                    c.Id,
                    c.Nome,
                    c.Email,
                    ValorTotal = c.Pedidos
                        .Where(p => p.Status != StatusPedido.Cancelado)
                        .Sum(p => p.ValorTotal),
                    QuantidadePedidos = c.Pedidos
                        .Count(p => p.Status != StatusPedido.Cancelado)
                })
                .Where(x => x.ValorTotal > 0)
                .OrderByDescending(x => x.ValorTotal)
                .Take(10)
                .ToList();
            
            if (!topClientes.Any())
            {
                Console.WriteLine("Nenhum cliente com compras encontrado.");
                return;
            }
            
            Console.WriteLine(new string('-', 90));
            Console.WriteLine("| {0,-5} | {1,-25} | {2,-8} | {3,-15} | {4,-12} |", 
                "Pos", "Cliente", "Pedidos", "Valor Total", "Ticket Médio");
            Console.WriteLine(new string('-', 90));
            
            for (int i = 0; i < topClientes.Count; i++)
            {
                var cliente = topClientes[i];
                var ticketMedio = cliente.QuantidadePedidos > 0 ? cliente.ValorTotal / cliente.QuantidadePedidos : 0;
                
                Console.WriteLine("| {0,-5} | {1,-25} | {2,-8} | {3,-15:C} | {4,-12:C} |",
                    i + 1,
                    cliente.Nome.Length > 25 ? cliente.Nome.Substring(0, 22) + "..." : cliente.Nome,
                    cliente.QuantidadePedidos,
                    cliente.ValorTotal,
                    ticketMedio);
            }
            Console.WriteLine(new string('-', 90));
        }

        // ========== RELATÓRIOS GERAIS ==========

        #endregion
    #region Relatórios Gerenciais
    // Relatórios gerenciais: dashboards e análises executivas

    // Menu para relatórios gerenciais
        public void RelatoriosGerais()
        {
            Console.WriteLine("=== RELATÓRIOS GERAIS ===");
            Console.WriteLine("1. Dashboard executivo");
            Console.WriteLine("2. Relatório de estoque");
            Console.WriteLine("3. Análise de clientes");

            var opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1": DashboardExecutivo(); break;
                case "2": RelatorioEstoque(); break;
                case "3": AnaliseClientes(); break;
            }
        }

    // Dashboard executivo (KPIs via agregações LINQ)
        private void DashboardExecutivo()
        {
            Console.WriteLine("=== DASHBOARD EXECUTIVO ===");
            
            // Cálculo de métricas principais usando LINQ
            var totalPedidos = _context.Pedidos.Count(p => p.Status != StatusPedido.Cancelado);
            var ticketMedio = _context.Pedidos
                .Where(p => p.Status != StatusPedido.Cancelado)
                .ToList()
                .Average(p => (decimal?)p.ValorTotal) ?? 0;
            
            var produtosEmEstoque = _context.Produtos.Count(p => p.Ativo && p.Estoque > 0);
            var clientesAtivos = _context.Clientes.Count(c => c.Ativo);
            
            // Faturamento dos últimos 30 dias
            var faturamentoMensal = _context.Pedidos
                .Where(p => p.Status != StatusPedido.Cancelado && 
                           p.DataPedido >= DateTime.Now.AddDays(-30))
                .ToList()
                .Sum(p => (decimal?)p.ValorTotal) ?? 0;
            
            // Valor total em estoque
            var valorTotalEstoque = _context.Produtos
                .Where(p => p.Ativo)
                .ToList()
                .Sum(p => (decimal?)(p.Estoque * p.Preco)) ?? 0;
            
            // Exibição formatada estilo dashboard
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║           RESUMO EXECUTIVO           ║");
            Console.WriteLine("╠══════════════════════════════════════╣");
            Console.WriteLine($"║ Total de Pedidos: {totalPedidos,18} ║");
            Console.WriteLine($"║ Ticket Médio: {ticketMedio,21:C} ║");
            Console.WriteLine($"║ Produtos em Estoque: {produtosEmEstoque,14} ║");
            Console.WriteLine($"║ Clientes Ativos: {clientesAtivos,18} ║");
            Console.WriteLine($"║ Faturamento (30 dias): {faturamentoMensal,12:C} ║");
            Console.WriteLine($"║ Valor Total Estoque: {valorTotalEstoque,14:C} ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
        }

    // Relatório detalhado de estoque (agrupamento por categoria)
        private void RelatorioEstoque()
        {
            Console.WriteLine("=== RELATÓRIO DE ESTOQUE ===");
            
            // Produtos agrupados por categoria
            var produtosPorCategoria = _context.Produtos
                .Include(p => p.Categoria)
                .Where(p => p.Ativo)
                .ToList() // Força execução no cliente para trabalhar com decimais
                .GroupBy(p => p.Categoria.Nome)
                .Select(g => new
                {
                    Categoria = g.Key,
                    QuantidadeProdutos = g.Count(),
                    EstoqueTotal = g.Sum(p => p.Estoque),
                    ValorEstoque = g.Sum(p => p.Estoque * p.Preco)
                })
                .OrderByDescending(x => x.ValorEstoque)
                .ToList();
            
            Console.WriteLine("\n--- ESTOQUE POR CATEGORIA ---");
            Console.WriteLine(new string('-', 70));
            Console.WriteLine("| {0,-15} | {1,-8} | {2,-12} | {3,-15} |", 
                "Categoria", "Produtos", "Estoque Total", "Valor Estoque");
            Console.WriteLine(new string('-', 70));
            
            foreach (var categoria in produtosPorCategoria)
            {
                Console.WriteLine("| {0,-15} | {1,-8} | {2,-12} | {3,-15:C} |",
                    categoria.Categoria.Length > 15 ? categoria.Categoria.Substring(0, 12) + "..." : categoria.Categoria,
                    categoria.QuantidadeProdutos,
                    categoria.EstoqueTotal,
                    categoria.ValorEstoque);
            }
            Console.WriteLine(new string('-', 70));
            
            // Análise de produtos zerados
            var produtosZerados = _context.Produtos
                .Include(p => p.Categoria)
                .Where(p => p.Ativo && p.Estoque == 0)
                .Select(p => new { p.Nome, Categoria = p.Categoria.Nome })
                .ToList();
            
            Console.WriteLine($"\n--- PRODUTOS ZERADOS ({produtosZerados.Count}) ---");
            foreach (var produto in produtosZerados)
            {
                Console.WriteLine($"- {produto.Nome} ({produto.Categoria})");
            }
            
            // Contagem de produtos em estoque baixo
            var produtosEstoqueBaixo = _context.Produtos
                .Where(p => p.Ativo && p.Estoque > 0 && p.Estoque < 20)
                .Count();
            
            Console.WriteLine($"\n--- RESUMO ---");
            Console.WriteLine($"Produtos zerados: {produtosZerados.Count}");
            Console.WriteLine($"Produtos em estoque baixo (< 20): {produtosEstoqueBaixo}");
            
            var valorTotalEstoque = produtosPorCategoria.Sum(x => x.ValorEstoque);
            Console.WriteLine($"Valor total em estoque: {valorTotalEstoque:C}");
        }

    // Análise de clientes (agrupamento por estado)
        private void AnaliseClientes()
        {
            Console.WriteLine("=== ANÁLISE DE CLIENTES ===");
            
            // Agrupamento de clientes por estado
            var clientesPorEstado = _context.Clientes
                .Where(c => c.Ativo && !string.IsNullOrEmpty(c.Estado))
                .GroupBy(c => c.Estado)
                .Select(g => new
                {
                    Estado = g.Key,
                    QuantidadeClientes = g.Count(),
                    ValorTotalPedidos = g.SelectMany(c => c.Pedidos)
                        .Where(p => p.Status != StatusPedido.Cancelado)
                        .Sum(p => (decimal?)p.ValorTotal) ?? 0
                })
                .OrderByDescending(x => x.QuantidadeClientes)
                .ToList();
            
            Console.WriteLine("\n--- CLIENTES POR ESTADO ---");
            Console.WriteLine(new string('-', 60));
            Console.WriteLine("| {0,-8} | {1,-10} | {2,-15} | {3,-12} |", 
                "Estado", "Clientes", "Valor Total", "Valor Médio");
            Console.WriteLine(new string('-', 60));
            
            foreach (var estado in clientesPorEstado)
            {
                var valorMedio = estado.QuantidadeClientes > 0 ? estado.ValorTotalPedidos / estado.QuantidadeClientes : 0;
                
                Console.WriteLine("| {0,-8} | {1,-10} | {2,-15:C} | {3,-12:C} |",
                    estado.Estado ?? "",
                    estado.QuantidadeClientes,
                    estado.ValorTotalPedidos,
                    valorMedio);
            }
            Console.WriteLine(new string('-', 60));
            
            // Métricas gerais de clientes
            var valorMedioGeral = _context.Clientes
                .Where(c => c.Ativo)
                .SelectMany(c => c.Pedidos)
                .Where(p => p.Status != StatusPedido.Cancelado)
                .ToList()
                .Average(p => (decimal?)p.ValorTotal) ?? 0;
            
            var totalClientes = _context.Clientes.Count(c => c.Ativo);
            var clientesComPedidos = _context.Clientes.Count(c => c.Ativo && c.Pedidos.Any());
            
            Console.WriteLine($"\n--- RESUMO GERAL ---");
            Console.WriteLine($"Total de clientes ativos: {totalClientes}");
            Console.WriteLine($"Clientes com pedidos: {clientesComPedidos}");
            Console.WriteLine($"Valor médio por pedido: {valorMedioGeral:C}");
            
            var percentualClientesComPedidos = totalClientes > 0 ? (clientesComPedidos * 100.0m / totalClientes) : 0;
            Console.WriteLine($"Percentual de clientes ativos: {percentualClientesComPedidos:F1}%");
        }

        #endregion
    #region Dispose e Finalização
    // Gerenciamento de recursos

    // Libera recursos do contexto (padrão Dispose)
        public void Dispose()
        {
            _context?.Dispose();
        }

        #endregion
    }
