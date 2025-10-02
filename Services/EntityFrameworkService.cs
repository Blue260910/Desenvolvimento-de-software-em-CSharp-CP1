
using CheckPoint1;
namespace CheckPoint1.Services;

    public class EntityFrameworkService
    {
        private readonly CheckpointContext _context;

        public EntityFrameworkService()
        {
            _context = new CheckpointContext();
        }

        // ========== CRUD CATEGORIAS ==========

        public void CriarCategoria()
        {
            Console.WriteLine("=== CRIAR CATEGORIA ===");
            Console.Write("Nome da categoria: ");
            var nome = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nome)) { Console.WriteLine("Nome inválido!"); return; }
            _context.Categorias.Add(new Models.Categoria { Nome = nome });
            _context.SaveChanges();
            Console.WriteLine("Categoria criada!");
        }

        public void ListarCategorias()
        {
            Console.WriteLine("=== CATEGORIAS ===");
            var categorias = _context.Categorias
                .Select(c => new {
                    c.Id,
                    c.Nome,
                    QtdeProdutos = c.Produtos.Count
                }).ToList();
            foreach (var c in categorias)
                Console.WriteLine($"{c.Id}: {c.Nome} (Produtos: {c.QtdeProdutos})");
        }

        // ========== CRUD PRODUTOS ==========

        public void CriarProduto()
        {
            Console.WriteLine("=== CRIAR PRODUTO ===");
            ListarCategorias();
            Console.Write("Informe o ID da categoria: ");
            if (!int.TryParse(Console.ReadLine(), out int catId)) { Console.WriteLine("ID inválido!"); return; }
            var categoria = _context.Categorias.Find(catId);
            if (categoria == null) { Console.WriteLine("Categoria não encontrada!"); return; }
            Console.Write("Nome do produto: ");
            var nome = Console.ReadLine();
            Console.Write("Preço: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal preco)) { Console.WriteLine("Preço inválido!"); return; }
            Console.Write("Estoque: ");
            if (!int.TryParse(Console.ReadLine(), out int estoque)) { Console.WriteLine("Estoque inválido!"); return; }
            _context.Produtos.Add(new Models.Produto { Nome = nome ?? string.Empty, Preco = preco, Estoque = estoque, CategoriaId = catId });
            _context.SaveChanges();
            Console.WriteLine("Produto criado!");
        }

        public void ListarProdutos()
        {
            Console.WriteLine("=== PRODUTOS ===");
            var produtos = _context.Produtos
                .Select(p => new {
                    p.Id,
                    p.Nome,
                    p.Preco,
                    p.Estoque,
                    Categoria = p.Categoria.Nome
                }).ToList();
            foreach (var p in produtos)
                Console.WriteLine($"{p.Id}: {p.Nome} | {p.Categoria} | Preço: {p.Preco:C} | Estoque: {p.Estoque}");
        }

        public void AtualizarProduto()
        {
            Console.WriteLine("=== ATUALIZAR PRODUTO ===");
            ListarProdutos();
            Console.Write("ID do produto: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("ID inválido!"); return; }
            var prod = _context.Produtos.Find(id);
            if (prod == null) { Console.WriteLine("Produto não encontrado!"); return; }
            Console.Write($"Novo nome ({prod.Nome}): ");
            var nome = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(nome)) prod.Nome = nome;
            Console.Write($"Novo preço ({prod.Preco}): ");
            if (decimal.TryParse(Console.ReadLine(), out decimal preco)) prod.Preco = preco;
            Console.Write($"Novo estoque ({prod.Estoque}): ");
            if (int.TryParse(Console.ReadLine(), out int estoque)) prod.Estoque = estoque;
            _context.SaveChanges();
            Console.WriteLine("Produto atualizado!");
        }

        // ========== CRUD CLIENTES ==========

        public void CriarCliente()
        {
            Console.WriteLine("=== CRIAR CLIENTE ===");
            Console.Write("Nome: ");
            var nome = Console.ReadLine();
            Console.Write("Email: ");
            var email = Console.ReadLine();
            if (_context.Clientes.Any(c => c.Email == email)) { Console.WriteLine("Email já cadastrado!"); return; }
            Console.Write("CPF (somente números): ");
            var cpf = new string((Console.ReadLine() ?? "").Where(char.IsDigit).ToArray());
            if (cpf.Length != 11) { Console.WriteLine("CPF inválido!"); return; }
            // Se não existir campo Cpf, remova do construtor:
            _context.Clientes.Add(new Models.Cliente { Nome = nome ?? string.Empty, Email = email ?? string.Empty });
            _context.SaveChanges();
            Console.WriteLine("Cliente criado!");
        }

        public void ListarClientes()
        {
            Console.WriteLine("=== CLIENTES ===");
            var clientes = _context.Clientes
                .Select(c => new {
                    c.Id,
                    c.Nome,
                    c.Email,
                    QtdePedidos = c.Pedidos.Count
                }).ToList();
            foreach (var c in clientes)
                Console.WriteLine($"{c.Id}: {c.Nome} | {c.Email} | Pedidos: {c.QtdePedidos}");
        }

        public void AtualizarCliente()
        {
            Console.WriteLine("=== ATUALIZAR CLIENTE ===");
            ListarClientes();
            Console.Write("ID do cliente: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("ID inválido!"); return; }
            var cli = _context.Clientes.Find(id);
            if (cli == null) { Console.WriteLine("Cliente não encontrado!"); return; }
            Console.Write($"Novo nome ({cli.Nome}): ");
            var nome = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(nome)) cli.Nome = nome;
            Console.Write($"Novo email ({cli.Email}): ");
            var email = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(email) && !_context.Clientes.Any(c => c.Email == email && c.Id != id)) cli.Email = email;
            _context.SaveChanges();
            Console.WriteLine("Cliente atualizado!");
        }

        // ========== CRUD PEDIDOS ==========

        public void CriarPedido()
        {
            Console.WriteLine("=== CRIAR PEDIDO ===");
            ListarClientes();
            Console.Write("ID do cliente: ");
            if (!int.TryParse(Console.ReadLine(), out int clienteId)) { Console.WriteLine("ID inválido!"); return; }
            var cliente = _context.Clientes.Find(clienteId);
            if (cliente == null) { Console.WriteLine("Cliente não encontrado!"); return; }
            string numeroPedido = $"PED{DateTime.Now.Ticks % 1000000:D6}";
            var pedido = new Models.Pedido { ClienteId = clienteId, NumeroPedido = numeroPedido, DataPedido = DateTime.Now, Itens = new List<Models.PedidoItem>() };
            while (true)
            {
                ListarProdutos();
                Console.Write("ID do produto (ou vazio para terminar): ");
                var prodStr = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(prodStr)) break;
                if (!int.TryParse(prodStr, out int prodId)) { Console.WriteLine("ID inválido!"); continue; }
                var produto = _context.Produtos.Find(prodId);
                if (produto == null) { Console.WriteLine("Produto não encontrado!"); continue; }
                Console.Write("Quantidade: ");
                if (!int.TryParse(Console.ReadLine(), out int qtde) || qtde <= 0) { Console.WriteLine("Quantidade inválida!"); continue; }
                if (produto.Estoque < qtde) { Console.WriteLine("Estoque insuficiente!"); continue; }
                pedido.Itens.Add(new Models.PedidoItem { ProdutoId = prodId, Quantidade = qtde, PrecoUnitario = produto.Preco });
                produto.Estoque -= qtde;
            }
            if (pedido.Itens.Count == 0) { Console.WriteLine("Pedido sem itens!"); return; }
            _context.Pedidos.Add(pedido);
            _context.SaveChanges();
            Console.WriteLine($"Pedido criado! Número: {numeroPedido}");
        }

        public void ListarPedidos()
        {
            Console.WriteLine("=== PEDIDOS ===");
            var pedidos = _context.Pedidos
                .Select(p => new {
                    p.Id,
                    p.NumeroPedido,
                    Cliente = p.Cliente.Nome,
                    p.DataPedido,
                    Itens = p.Itens.Select(i => new {
                        Produto = i.Produto.Nome,
                        i.Quantidade,
                        i.PrecoUnitario
                    }).ToList()
                }).ToList();
            foreach (var p in pedidos)
            {
                Console.WriteLine($"Pedido: {p.NumeroPedido} | Cliente: {p.Cliente} | Data: {p.DataPedido:dd/MM/yyyy}");
                foreach (var i in p.Itens)
                    Console.WriteLine($"  Produto: {i.Produto} | Qtde: {i.Quantidade} | Unit: {i.PrecoUnitario:C}");
            }
        }

        public void AtualizarStatusPedido()
        {
            Console.WriteLine("=== ATUALIZAR STATUS PEDIDO ===");
            ListarPedidos();
            Console.Write("ID do pedido: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("ID inválido!"); return; }
            var pedido = _context.Pedidos.Find(id);
            if (pedido == null) { Console.WriteLine("Pedido não encontrado!"); return; }
            var statusAtual = pedido.Status;
            Console.WriteLine($"Status atual: {statusAtual}");
            Console.WriteLine("Status disponíveis: Pendente, Confirmado, Cancelado, Entregue");
            Console.Write("Novo status: ");
            var novoStatusStr = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(novoStatusStr)) { Console.WriteLine("Status inválido!"); return; }
            // Simples validação de transição
            if (statusAtual == StatusPedido.Entregue || statusAtual == StatusPedido.Cancelado) { Console.WriteLine("Não é possível alterar este pedido."); return; }
            if (!Enum.TryParse<StatusPedido>(novoStatusStr, true, out var novoStatus)) { Console.WriteLine("Status inválido!"); return; }
            pedido.Status = novoStatus;
            _context.SaveChanges();
            Console.WriteLine("Status atualizado!");
        }

        public void CancelarPedido()
        {
            Console.WriteLine("=== CANCELAR PEDIDO ===");
            ListarPedidos();
            Console.Write("ID do pedido: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("ID inválido!"); return; }
            var pedido = _context.Pedidos.Find(id);
            if (pedido == null) { Console.WriteLine("Pedido não encontrado!"); return; }
            if (pedido.Status != StatusPedido.Pendente && pedido.Status != StatusPedido.Confirmado) { Console.WriteLine("Só é possível cancelar pedidos pendentes ou confirmados."); return; }
            pedido.Status = StatusPedido.Cancelado;
            foreach (var item in _context.PedidoItens.Where(i => i.PedidoId == id))
            {
                var prod = _context.Produtos.Find(item.ProdutoId);
                if (prod != null) prod.Estoque += item.Quantidade;
            }
            _context.SaveChanges();
            Console.WriteLine("Pedido cancelado e estoque devolvido.");
        }

        // ========== CONSULTAS LINQ AVANÇADAS ==========

        public void ConsultasAvancadas()
        {
            Console.WriteLine("=== CONSULTAS LINQ ===");
            Console.WriteLine("1. Produtos mais vendidos");
            Console.WriteLine("2. Clientes com mais pedidos");
            Console.WriteLine("3. Faturamento por categoria");
            Console.WriteLine("4. Pedidos por período");
            Console.WriteLine("5. Produtos em estoque baixo");
            

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

        private void ProdutosMaisVendidos()
        {
            var query = _context.PedidoItens
                .GroupBy(i => new { i.ProdutoId, i.Produto.Nome, Categoria = i.Produto.Categoria.Nome })
                .Select(g => new {
                    Produto = g.Key.Nome,
                    Categoria = g.Key.Categoria,
                    QtdeVendida = g.Sum(x => x.Quantidade)
                })
                .OrderByDescending(x => x.QtdeVendida)
                .ToList();
            foreach (var p in query)
                Console.WriteLine($"{p.Produto} | {p.Categoria} | Vendidos: {p.QtdeVendida}");
        }

        private void ClientesComMaisPedidos()
        {
            var query = _context.Pedidos
                .GroupBy(p => new { p.ClienteId, p.Cliente.Nome })
                .Select(g => new {
                    Cliente = g.Key.Nome,
                    QtdePedidos = g.Count()
                })
                .OrderByDescending(x => x.QtdePedidos)
                .ToList();
            foreach (var c in query)
                Console.WriteLine($"{c.Cliente} | Pedidos: {c.QtdePedidos}");
        }

        private void FaturamentoPorCategoria()
        {
            var query = _context.PedidoItens
                .GroupBy(i => i.Produto.Categoria.Nome)
                .Select(g => new {
                    Categoria = g.Key,
                    Faturamento = g.Sum(x => x.Quantidade * x.PrecoUnitario),
                    ProdutosVendidos = g.Select(x => x.ProdutoId).Distinct().Count(),
                    TicketMedio = g.Average(x => x.Quantidade * x.PrecoUnitario)
                })
                .OrderByDescending(x => x.Faturamento)
                .ToList();
            foreach (var c in query)
                Console.WriteLine($"{c.Categoria} | Faturamento: {c.Faturamento:C} | Produtos: {c.ProdutosVendidos} | Ticket Médio: {c.TicketMedio:C}");
        }

        private void PedidosPorPeriodo()
        {
            Console.Write("Data início (dd/MM/yyyy): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime inicio)) { Console.WriteLine("Data inválida!"); return; }
            Console.Write("Data fim (dd/MM/yyyy): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime fim)) { Console.WriteLine("Data inválida!"); return; }
            var query = _context.Pedidos
                .Where(p => p.DataPedido >= inicio && p.DataPedido <= fim)
                .GroupBy(p => p.DataPedido.Date)
                .Select(g => new {
                    Data = g.Key,
                    QtdePedidos = g.Count(),
                    Total = g.SelectMany(x => x.Itens).Sum(i => i.Quantidade * i.PrecoUnitario)
                })
                .OrderBy(x => x.Data)
                .ToList();
            foreach (var d in query)
                Console.WriteLine($"{d.Data:dd/MM/yyyy} | Pedidos: {d.QtdePedidos} | Total: {d.Total:C}");
        }

        private void ProdutosEstoqueBaixo()
        {
            var query = _context.Produtos
                .Where(p => p.Estoque < 20)
                .Select(p => new {
                    p.Nome,
                    p.Estoque,
                    Categoria = p.Categoria.Nome
                })
                .OrderBy(p => p.Estoque)
                .ToList();
            foreach (var p in query)
                Console.WriteLine($"{p.Nome} | {p.Categoria} | Estoque: {p.Estoque}");
        }

        private void AnaliseVendasMensal()
        {
            var query = _context.PedidoItens
                .GroupBy(i => new { i.Pedido.DataPedido.Year, i.Pedido.DataPedido.Month })
                .Select(g => new {
                    Ano = g.Key.Year,
                    Mes = g.Key.Month,
                    QtdeVendida = g.Sum(x => x.Quantidade),
                    Faturamento = g.Sum(x => x.Quantidade * x.PrecoUnitario)
                })
                .OrderBy(x => x.Ano).ThenBy(x => x.Mes)
                .ToList();
            decimal? anterior = null;
            foreach (var m in query)
            {
                string crescimento = "-";
                if (anterior.HasValue && anterior.Value > 0)
                    crescimento = $"{((m.Faturamento - anterior.Value) / anterior.Value * 100):F2}%";
                Console.WriteLine($"{m.Mes:00}/{m.Ano}: {m.Faturamento:C} (Crescimento: {crescimento})");
                anterior = m.Faturamento;
            }
        }

        private void TopClientesPorValor()
        {
            var query = _context.Pedidos
                .GroupBy(p => new { p.ClienteId, p.Cliente.Nome })
                .Select(g => new {
                    Cliente = g.Key.Nome,
                    ValorTotal = g.SelectMany(x => x.Itens).Sum(i => i.Quantidade * i.PrecoUnitario)
                })
                .OrderByDescending(x => x.ValorTotal)
                .Take(10)
                .ToList();
            foreach (var c in query)
                Console.WriteLine($"{c.Cliente} | Valor Total: {c.ValorTotal:C}");
        }

        // ========== RELATÓRIOS GERAIS ==========

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

        private void DashboardExecutivo()
        {
            var totalPedidos = _context.Pedidos.Count();
            var ticketMedio = _context.Pedidos.Any() ? _context.Pedidos.SelectMany(p => p.Itens).Sum(i => i.Quantidade * i.PrecoUnitario) / _context.Pedidos.Count() : 0;
            var produtosEstoque = _context.Produtos.Sum(p => p.Estoque);
            var clientesAtivos = _context.Clientes.Count();
            var faturamentoMensal = _context.PedidoItens
                .Where(i => i.Pedido.DataPedido.Month == DateTime.Now.Month && i.Pedido.DataPedido.Year == DateTime.Now.Year)
                .Sum(i => i.Quantidade * i.PrecoUnitario);
            Console.WriteLine($"Pedidos: {totalPedidos}");
            Console.WriteLine($"Ticket médio: {ticketMedio:C}");
            Console.WriteLine($"Produtos em estoque: {produtosEstoque}");
            Console.WriteLine($"Clientes ativos: {clientesAtivos}");
            Console.WriteLine($"Faturamento do mês: {faturamentoMensal:C}");
        }

        private void RelatorioEstoque()
        {
            var categorias = _context.Categorias
                .Select(c => new {
                    c.Nome,
                    Produtos = c.Produtos.Select(p => new { p.Nome, p.Estoque, p.Preco })
                }).ToList();
            decimal totalEstoque = 0;
            Console.WriteLine("=== ESTOQUE POR CATEGORIA ===");
            foreach (var c in categorias)
            {
                Console.WriteLine($"Categoria: {c.Nome}");
                foreach (var p in c.Produtos)
                {
                    decimal valor = p.Estoque * p.Preco;
                    totalEstoque += valor;
                    Console.WriteLine($"  {p.Nome} | Estoque: {p.Estoque} | Valor: {valor:C}");
                }
            }
            Console.WriteLine($"Valor total em estoque: {totalEstoque:C}");
            var zerados = _context.Produtos.Where(p => p.Estoque == 0).ToList();
            if (zerados.Any())
            {
                Console.WriteLine("Produtos zerados:");
                foreach (var p in zerados)
                    Console.WriteLine($"  {p.Nome}");
            }
            var baixo = _context.Produtos.Where(p => p.Estoque < 20 && p.Estoque > 0).ToList();
            if (baixo.Any())
            {
                Console.WriteLine("Produtos em estoque baixo:");
                foreach (var p in baixo)
                    Console.WriteLine($"  {p.Nome} | Estoque: {p.Estoque}");
            }
        }

        private void AnaliseClientes()
        {
            // Supondo que existe campo Estado em Cliente
            var porEstado = _context.Clientes
                .GroupBy(c => c.Estado)
                .Select(g => new { Estado = g.Key, Qtde = g.Count() })
                .ToList();
            Console.WriteLine("Clientes por estado:");
            foreach (var e in porEstado)
                Console.WriteLine($"{e.Estado}: {e.Qtde}");
            var valorMedio = _context.Pedidos
                .GroupBy(p => p.ClienteId)
                .Select(g => g.SelectMany(x => x.Itens).Sum(i => i.Quantidade * i.PrecoUnitario))
                .DefaultIfEmpty(0).Average();
            Console.WriteLine($"Valor médio por cliente: {valorMedio:C}");
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
