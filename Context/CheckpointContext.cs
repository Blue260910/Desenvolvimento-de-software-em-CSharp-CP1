
using Microsoft.EntityFrameworkCore;
using CheckPoint1.Models;

namespace CheckPoint1;

public class CheckpointContext : DbContext
{
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<PedidoItem> PedidoItens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=loja.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Categoria -> Produtos (1:N, cascade delete)
        modelBuilder.Entity<Produto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Produtos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Cliente -> Pedidos (1:N, cascade delete)
        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Cliente)
            .WithMany(c => c.Pedidos)
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Pedido -> PedidoItens (1:N, cascade delete)
        modelBuilder.Entity<PedidoItem>()
            .HasOne(pi => pi.Pedido)
            .WithMany(p => p.Itens)
            .HasForeignKey(pi => pi.PedidoId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Produto -> PedidoItens (1:N, restrict delete)
        modelBuilder.Entity<PedidoItem>()
            .HasOne(pi => pi.Produto)
            .WithMany(p => p.PedidoItens)
            .HasForeignKey(pi => pi.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Índices únicos
        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.Email)
            .IsUnique();

        modelBuilder.Entity<Pedido>()
            .HasIndex(p => p.NumeroPedido)
            .IsUnique();

        // Dados iniciais
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nome = "Eletrônicos" },
            new Categoria { Id = 2, Nome = "Roupas" },
            new Categoria { Id = 3, Nome = "Livros" }
        );

        modelBuilder.Entity<Produto>().HasData(
            new Produto { Id = 1, Nome = "Notebook", Preco = 3500, Estoque = 5, CategoriaId = 1 },
            new Produto { Id = 2, Nome = "Smartphone", Preco = 2000, Estoque = 0, CategoriaId = 1 },
            new Produto { Id = 3, Nome = "Camiseta", Preco = 50, Estoque = 20, CategoriaId = 2 },
            new Produto { Id = 4, Nome = "Calça Jeans", Preco = 120, Estoque = 10, CategoriaId = 2 },
            new Produto { Id = 5, Nome = "Livro A", Preco = 40, Estoque = 0, CategoriaId = 3 },
            new Produto { Id = 6, Nome = "Livro B", Preco = 60, Estoque = 7, CategoriaId = 3 }
        );

        modelBuilder.Entity<Cliente>().HasData(
            new Cliente { Id = 1, Nome = "João Silva", Email = "joao@email.com" },
            new Cliente { Id = 2, Nome = "Maria Souza", Email = "maria@email.com" }
        );

        modelBuilder.Entity<Pedido>().HasData(
            new Pedido { Id = 1, NumeroPedido = "PED001", ClienteId = 1, DataPedido = new DateTime(2025, 10, 1) },
            new Pedido { Id = 2, NumeroPedido = "PED002", ClienteId = 2, DataPedido = new DateTime(2025, 10, 2) }
        );

        modelBuilder.Entity<PedidoItem>().HasData(
            new PedidoItem { Id = 1, PedidoId = 1, ProdutoId = 1, Quantidade = 1, PrecoUnitario = 3500 },
            new PedidoItem { Id = 2, PedidoId = 1, ProdutoId = 2, Quantidade = 2, PrecoUnitario = 2000 },
            new PedidoItem { Id = 3, PedidoId = 2, ProdutoId = 3, Quantidade = 3, PrecoUnitario = 50 },
            new PedidoItem { Id = 4, PedidoId = 2, ProdutoId = 5, Quantidade = 1, PrecoUnitario = 40 }
        );
    }
}