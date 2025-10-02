using System.Data.SQLite;

namespace CheckPoint1.Services;

public class AdoNetService
{
    private readonly string _connectionString;
        
    public AdoNetService()
    {
        // Connection string para SQLite usando o arquivo "loja.db"
        _connectionString = "Data Source=loja.db;Version=3;";
    }
        
    // ========== CONSULTAS COMPLEXAS ==========
        
    public void RelatorioVendasCompleto()
    {
        Console.WriteLine("=== RELATÓRIO VENDAS COMPLETO (ADO.NET) ===");
        using var conn = GetConnection();
        conn.Open();
        string sql = @"
            SELECT p.NumeroPedido, c.Nome AS NomeCliente, pr.Nome AS NomeProduto, pi.Quantidade, pi.PrecoUnitario,
                   (pi.Quantidade * pi.PrecoUnitario) AS Subtotal, p.DataPedido
            FROM Pedidos p
            JOIN Clientes c ON p.ClienteId = c.Id
            JOIN PedidoItens pi ON pi.PedidoId = p.Id
            JOIN Produtos pr ON pi.ProdutoId = pr.Id
            ORDER BY p.DataPedido, p.NumeroPedido
        ";
        using var cmd = new SQLiteCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
    string? lastPedido = null;
        while (reader.Read())
        {
            string numero = reader["NumeroPedido"]?.ToString() ?? string.Empty;
            if (lastPedido != numero)
            {
                Console.WriteLine($"\nPedido: {numero} | Cliente: {reader["NomeCliente"]} | Data: {Convert.ToDateTime(reader["DataPedido"]).ToShortDateString()}");
                lastPedido = numero;
            }
            Console.WriteLine($"  Produto: {reader["NomeProduto"]} | Qtde: {reader["Quantidade"]} | Unit: {reader["PrecoUnitario"]:C} | Subtotal: {reader["Subtotal"]:C}");
        }
    }
        
    public void FaturamentoPorCliente()
    {
        Console.WriteLine("=== FATURAMENTO POR CLIENTE ===");
        using var conn = GetConnection();
        conn.Open();
        string sql = @"
            SELECT c.Nome AS NomeCliente, COUNT(DISTINCT p.Id) AS QtdePedidos,
                   SUM(pi.Quantidade * pi.PrecoUnitario) AS Faturamento,
                   CASE WHEN COUNT(DISTINCT p.Id) > 0 THEN SUM(pi.Quantidade * pi.PrecoUnitario) * 1.0 / COUNT(DISTINCT p.Id) ELSE 0 END AS TicketMedio
            FROM Clientes c
            LEFT JOIN Pedidos p ON p.ClienteId = c.Id
            LEFT JOIN PedidoItens pi ON pi.PedidoId = p.Id
            GROUP BY c.Id, c.Nome
            ORDER BY Faturamento DESC
        ";
        using var cmd = new SQLiteCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
    Console.WriteLine($"{"Cliente",-20} {"Pedidos",7} {"Faturamento",15} {"Ticket Médio",15}");
        while (reader.Read())
        {
            Console.WriteLine($"{reader["NomeCliente"],-20} {reader["QtdePedidos"],7} {Convert.ToDecimal(reader["Faturamento"]):C} {Convert.ToDecimal(reader["TicketMedio"]):C}");
        }
    }
        
    public void ProdutosSemVenda()
    {
        Console.WriteLine("=== PRODUTOS SEM VENDAS ===");
        using var conn = GetConnection();
        conn.Open();
        string sql = @"
            SELECT c.Nome AS Categoria, p.Nome, p.Preco, p.Estoque, (p.Preco * p.Estoque) AS ValorParado
            FROM Produtos p
            JOIN Categorias c ON p.CategoriaId = c.Id
            LEFT JOIN PedidoItens pi ON pi.ProdutoId = p.Id
            WHERE pi.Id IS NULL
        ";
        using var cmd = new SQLiteCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        decimal total = 0;
    Console.WriteLine($"{"Categoria",-15} {"Produto",-20} {"Preço",10} {"Estoque",8} {"Valor Parado",15}");
        while (reader.Read())
        {
            decimal valor = Convert.ToDecimal(reader["ValorParado"]);
            total += valor;
            Console.WriteLine($"{reader["Categoria"],-15} {reader["Nome"],-20} {Convert.ToDecimal(reader["Preco"]):C} {reader["Estoque"],8} {valor,15:C}");
        }
        Console.WriteLine($"Total parado em estoque: {total:C}");
    }
        
    // ========== OPERAÇÕES DE DADOS ==========
        
    public void AtualizarEstoqueLote()
    {
        Console.WriteLine("=== ATUALIZAR ESTOQUE EM LOTE ===");
        using var conn = GetConnection();
        conn.Open();
        // Listar categorias
    var catCmd = new SQLiteCommand("SELECT Id, Nome FROM Categorias", conn);
        var catReader = catCmd.ExecuteReader();
        var categorias = new List<(int, string)>();
        while (catReader.Read())
            categorias.Add((Convert.ToInt32(catReader["Id"]), catReader["Nome"]?.ToString() ?? string.Empty));
        catReader.Close();
        Console.WriteLine("Categorias disponíveis:");
        foreach (var c in categorias)
            Console.WriteLine($"{c.Item1}: {c.Item2}");
        Console.Write("Informe o ID da categoria: ");
    int catId = int.Parse(Console.ReadLine() ?? "0");
        // Listar produtos da categoria
    var prodCmd = new SQLiteCommand("SELECT Id, Nome, Estoque FROM Produtos WHERE CategoriaId = @catId", conn);
        prodCmd.Parameters.AddWithValue("@catId", catId);
        var prodReader = prodCmd.ExecuteReader();
        var produtos = new List<(int, string, int)>();
        while (prodReader.Read())
            produtos.Add((Convert.ToInt32(prodReader["Id"]), prodReader["Nome"]?.ToString() ?? string.Empty, Convert.ToInt32(prodReader["Estoque"])));
        prodReader.Close();
        int totalAfetados = 0;
        foreach (var p in produtos)
        {
            Console.Write($"Produto: {p.Item2} (Estoque atual: {p.Item3}) - Novo estoque: ");
            int novoEstoque = int.Parse(Console.ReadLine() ?? "0");
            var updCmd = new SQLiteCommand("UPDATE Produtos SET Estoque = @novo WHERE Id = @id", conn);
            updCmd.Parameters.AddWithValue("@novo", novoEstoque);
            updCmd.Parameters.AddWithValue("@id", p.Item1);
            totalAfetados += updCmd.ExecuteNonQuery();
        }
        Console.WriteLine($"Registros afetados: {totalAfetados}");
    }
        
    public void InserirPedidoCompleto()
    {
        Console.WriteLine("=== INSERIR PEDIDO COMPLETO ===");
        using var conn = GetConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            // Selecionar cliente
            var cliCmd = new SQLiteCommand("SELECT Id, Nome FROM Clientes", conn, tx);
            var cliReader = cliCmd.ExecuteReader();
            var clientes = new List<(int, string)>();
            while (cliReader.Read())
                clientes.Add((Convert.ToInt32(cliReader["Id"]), cliReader["Nome"]?.ToString() ?? string.Empty));
            cliReader.Close();
            Console.WriteLine("Clientes disponíveis:");
            foreach (var c in clientes)
                Console.WriteLine($"{c.Item1}: {c.Item2}");
            Console.Write("Informe o ID do cliente: ");
            int clienteId = int.Parse(Console.ReadLine() ?? "0");
            // Inserir pedido
            Console.Write("Número do pedido: ");
            string numeroPedido = Console.ReadLine() ?? string.Empty;
            var insPedido = new SQLiteCommand("INSERT INTO Pedidos (NumeroPedido, ClienteId, DataPedido) VALUES (@num, @cli, @data)", conn, tx);
            insPedido.Parameters.AddWithValue("@num", numeroPedido);
            insPedido.Parameters.AddWithValue("@cli", clienteId);
            insPedido.Parameters.AddWithValue("@data", DateTime.Now);
            insPedido.ExecuteNonQuery();
            // Obter ID do pedido inserido
            var idPedidoCmd = new SQLiteCommand("SELECT last_insert_rowid()", conn, tx);
            long pedidoId = (long)idPedidoCmd.ExecuteScalar();
            // Inserir itens
            while (true)
            {
                Console.Write("ID do produto (ou vazio para terminar): ");
                string prodStr = Console.ReadLine() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(prodStr)) break;
                int prodId = int.Parse(prodStr);
                Console.Write("Quantidade: ");
                int qtde = int.Parse(Console.ReadLine() ?? "0");
                // Validar estoque
                var estoqueCmd = new SQLiteCommand("SELECT Estoque, Preco FROM Produtos WHERE Id = @id", conn, tx);
                estoqueCmd.Parameters.AddWithValue("@id", prodId);
                using var estReader = estoqueCmd.ExecuteReader();
                if (!estReader.Read()) { Console.WriteLine("Produto não encontrado!"); continue; }
                int estoque = Convert.ToInt32(estReader["Estoque"]);
                decimal preco = Convert.ToDecimal(estReader["Preco"]);
                if (estoque < qtde) { Console.WriteLine("Estoque insuficiente!"); continue; }
                estReader.Close();
                // Inserir item
                var insItem = new SQLiteCommand("INSERT INTO PedidoItens (PedidoId, ProdutoId, Quantidade, PrecoUnitario) VALUES (@pid, @prod, @qtd, @preco)", conn, tx);
                insItem.Parameters.AddWithValue("@pid", pedidoId);
                insItem.Parameters.AddWithValue("@prod", prodId);
                insItem.Parameters.AddWithValue("@qtd", qtde);
                insItem.Parameters.AddWithValue("@preco", preco);
                insItem.ExecuteNonQuery();
                // Atualizar estoque
                var updEstoque = new SQLiteCommand("UPDATE Produtos SET Estoque = Estoque - @qtd WHERE Id = @id", conn, tx);
                updEstoque.Parameters.AddWithValue("@qtd", qtde);
                updEstoque.Parameters.AddWithValue("@id", prodId);
                updEstoque.ExecuteNonQuery();
            }
            tx.Commit();
            Console.WriteLine("Pedido inserido com sucesso!");
        }
        catch (Exception ex)
        {
            tx.Rollback();
            Console.WriteLine($"Erro ao inserir pedido: {ex.Message}");
        }
    }
        
    public void ExcluirDadosAntigos()
    {
        Console.WriteLine("=== EXCLUIR DADOS ANTIGOS ===");
        using var conn = GetConnection();
        conn.Open();
        // Supondo que existe um campo Status no Pedido (ex: Cancelado)
        string sql = @"
            DELETE FROM Pedidos WHERE Status = 'Cancelado' AND DataPedido < @dataLimite
        ";
        var cmd = new SQLiteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@dataLimite", DateTime.Now.AddMonths(-6));
        int afetados = cmd.ExecuteNonQuery();
        Console.WriteLine($"Pedidos cancelados excluídos: {afetados}");
    }
        
    public void ProcessarDevolucao()
    {
        Console.WriteLine("=== PROCESSAR DEVOLUÇÃO ===");
        using var conn = GetConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            Console.Write("Informe o número do pedido para devolução: ");
            string numeroPedido = Console.ReadLine();
            // Buscar pedido
            var pedCmd = new SQLiteCommand("SELECT Id, Status FROM Pedidos WHERE NumeroPedido = @num", conn, tx);
            pedCmd.Parameters.AddWithValue("@num", numeroPedido);
            object pedIdObj = pedCmd.ExecuteScalar();
            if (pedIdObj == null) { Console.WriteLine("Pedido não encontrado!"); return; }
            long pedidoId = (long)pedIdObj;
            // Validar se pode devolver (exemplo: status != Cancelado)
            var statusCmd = new SQLiteCommand("SELECT Status FROM Pedidos WHERE Id = @id", conn, tx);
            statusCmd.Parameters.AddWithValue("@id", pedidoId);
            string status = statusCmd.ExecuteScalar()?.ToString();
            if (status == "Cancelado") { Console.WriteLine("Pedido cancelado não pode ser devolvido!"); return; }
            // Buscar itens
            var itensCmd = new SQLiteCommand("SELECT ProdutoId, Quantidade FROM PedidoItens WHERE PedidoId = @pid", conn, tx);
            itensCmd.Parameters.AddWithValue("@pid", pedidoId);
            using var itensReader = itensCmd.ExecuteReader();
            var itens = new List<(int, int)>();
            while (itensReader.Read())
                itens.Add((Convert.ToInt32(itensReader["ProdutoId"]), Convert.ToInt32(itensReader["Quantidade"])));
            itensReader.Close();
            // Devolver estoque
            foreach (var item in itens)
            {
                var updEstoque = new SQLiteCommand("UPDATE Produtos SET Estoque = Estoque + @qtd WHERE Id = @id", conn, tx);
                updEstoque.Parameters.AddWithValue("@qtd", item.Item2);
                updEstoque.Parameters.AddWithValue("@id", item.Item1);
                updEstoque.ExecuteNonQuery();
            }
            tx.Commit();
            Console.WriteLine("Devolução processada e estoque atualizado.");
        }
        catch (Exception ex)
        {
            tx.Rollback();
            Console.WriteLine($"Erro ao processar devolução: {ex.Message}");
        }
    }
        
    // ========== ANÁLISES PERFORMANCE ==========
        
    public void AnalisarPerformanceVendas()
    {
        Console.WriteLine("=== ANÁLISE PERFORMANCE VENDAS ===");
        using var conn = GetConnection();
        conn.Open();
        string sql = @"
            SELECT strftime('%Y-%m', p.DataPedido) AS Mes, SUM(pi.Quantidade * pi.PrecoUnitario) AS Total
            FROM Pedidos p
            JOIN PedidoItens pi ON pi.PedidoId = p.Id
            GROUP BY Mes
            ORDER BY Mes
        ";
        using var cmd = new SQLiteCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        decimal? anterior = null;
        while (reader.Read())
        {
            string mes = reader["Mes"].ToString();
            decimal total = Convert.ToDecimal(reader["Total"]);
            string crescimento = "-";
            if (anterior.HasValue && anterior.Value > 0)
                crescimento = $"{((total - anterior.Value) / anterior.Value * 100):F2}%";
            Console.WriteLine($"{mes}: {total:C} (Crescimento: {crescimento})");
            anterior = total;
        }
    }
        
    // ========== UTILIDADES ==========
        
    private SQLiteConnection GetConnection()
    {
        return new SQLiteConnection(_connectionString);
    }
        

    public void TestarConexao()
    {
        Console.WriteLine("=== TESTE DE CONEXÃO ===");
        try
        {
            using var conn = GetConnection();
            conn.Open();
            var cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table'", conn);
            using var reader = cmd.ExecuteReader();
            Console.WriteLine("Tabelas no banco:");
            while (reader.Read())
                Console.WriteLine($"- {reader["name"]}");
            Console.WriteLine("Conexão e consulta executadas com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao conectar: {ex.Message}");
        }
    }
}