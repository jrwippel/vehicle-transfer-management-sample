using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebAppSystems.Models;
using WebAppSystemsTransp.Models;

namespace WebAppSystems.Data
{
    public class WebAppSystemsContext : DbContext
    {
        public WebAppSystemsContext (DbContextOptions<WebAppSystemsContext> options)
            : base(options)
        {
        }

       
        public DbSet<WebAppSystems.Models.Attorney> Attorney { get; set; } = default!;
        public DbSet<WebAppSystemsTransp.Models.Veiculo>? Veiculo { get; set; }
        public DbSet<WebAppSystemsTransp.Models.FotoPedido>? FotoPedido { get; set; }
        public DbSet<WebAppSystemsTransp.Models.Avaria>? Avaria { get; set; }

        public DbSet<WebAppSystemsTransp.Models.Pedido>? Pedido { get; set; }

        public DbSet<WebAppSystemsTransp.Models.Cliente>? Cliente { get; set; }

        public DbSet<WebAppSystemsTransp.Models.Marca>? Marca { get; set; }

        public DbSet<WebAppSystemsTransp.Models.Modelo>? Modelo { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração de relacionamento para ClienteCarga
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.ClienteCarga)
                .WithMany(c => c.PedidosCarga)
                .HasForeignKey(p => p.ClienteCargaId)
                .OnDelete(DeleteBehavior.Restrict); // Evita exclusão em cascata

            // Configuração de relacionamento para ClienteDescarga
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.ClienteDescarga)
                .WithMany(c => c.PedidosDescarga)
                .HasForeignKey(p => p.ClienteDescargaId)
                .OnDelete(DeleteBehavior.Restrict); // Evita exclusão em cascata
/*
            modelBuilder.Entity<Pedido>()
                .HasMany(v => v.Locations)
                .WithOne(l => l.Pedido)
                .HasForeignKey(l => l.PedidoId);
*/
            // Outras configurações de modelo podem ir aqui, se necessário         
        }            



    }
}


