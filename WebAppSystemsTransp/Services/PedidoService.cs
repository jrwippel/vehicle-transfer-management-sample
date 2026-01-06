using Google;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using WebAppSystems.Data;
using WebAppSystems.Models;
using WebAppSystemsTransp.Models;
using WebAppSystemsTransp.Models.Enums;

namespace WebAppSystems.Services
{
    public class PedidoService
    {
        private readonly WebAppSystemsContext _context; // DbContext do seu projeto

        public PedidoService(WebAppSystemsContext context)
        {
            _context = context;
        }

        public void UpdatePedido(Pedido pedido)
        {
            _context.Pedido.Update(pedido); // Marca o objeto como modificado
            _context.SaveChanges(); // Salva as alterações no banco
        }


        public void DeleteFotoPedido(FotoPedido foto)
        {
            _context.FotoPedido.Remove(foto);
            _context.SaveChanges();
        }


        public FotoPedido GetFotoPedidoByFileName(string fileName)
        {
            return _context.FotoPedido.FirstOrDefault(f => f.NomeArquivo == fileName);
        }


        public FotoPedido SaveOrUpdateFotoPedido(
            int pedidoId,
            TipoFotoVeiculo tipoFotoVeiculo,
            string nomeArquivo,
            string urlFoto,
            TipoFotoPedido tipoFotoPedido // Novo parâmetro
)
            {
            var fotoPedido = new FotoPedido
            {
                PedidoId = pedidoId,
                TipoFotoVeiculo = tipoFotoVeiculo,
                NomeArquivo = nomeArquivo,
                UrlFoto = urlFoto,
                TipoPedido = tipoFotoPedido, // Salva o TipoFotoPedido
                DataUpload = DateTime.UtcNow
            };

            _context.FotoPedido.Add(fotoPedido);
            _context.SaveChanges();

            return fotoPedido;
        }




        // Método para buscar todos os pedidos (apenas informações principais)
        // Método para buscar todos os pedidos (apenas informações principais)
        public IEnumerable<Pedido> GetAllPedidos()
        {
            return _context.Pedido
                .AsNoTracking()
                .Include(p => p.ClienteCarga) // Inclui informações do cliente de carga
                .Include(p => p.ClienteDescarga) // Inclui informações do cliente de descarga
                 .Include(p => p.Veiculo)
                .ToList();
        }



        public Pedido GetPedidoById(int id)
        {
            return _context.Pedido
                .Include(p => p.Veiculo) // Inclui o Veículo
                .ThenInclude(v => v.Modelo) // Inclui o Modelo do Veículo
                .ThenInclude(m => m.Marca) // Inclui a Marca do Modelo
                .Include(p => p.Motorista) // Inclui o Motorista
                .Include(p => p.ClienteCarga) // Inclui o Cliente de Carga
                .Include(p => p.ClienteDescarga) // Inclui o Cliente de Descarga
                .Include(p => p.FotoPedidos) // Inclui as Fotos do Pedido
                .FirstOrDefault(p => p.Id == id); // Busca o pedido pelo ID
        }


        // Retorna um pedido específico pelo ID com as fotos associadas
        public Pedido GetPhotoPedidoById(int id)
        {
            return _context.Pedido
                .AsNoTracking() // Melhor para consultas somente leitura
                .Include(p => p.FotoPedidos) // Inclui o relacionamento com as fotos
                .FirstOrDefault(p => p.Id == id);
        }

    }
}
