using Microsoft.EntityFrameworkCore;
using WebAppSystems.Data;
using WebAppSystemsTransp.Models;

namespace WebAppSystemsTransp.Services
{
    public class ModeloService
    {

        private readonly WebAppSystemsContext _context;

        public ModeloService(WebAppSystemsContext context)
        {
            _context = context;
        }


        public async Task<(IEnumerable<Modelo> records, int totalRecords)> FindAllAsync(
            int page,
            int length,
            string searchValue = "",
            int orderColumn = 0,
            string orderDir = "desc"
            )
        {
            var query = _context.Modelo
            .Include(m => m.Marca) // Correto, pois 'Marca' é uma relação
            .AsQueryable();


            // Filtro de pesquisa
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(pr =>
                    pr.Nome.ToLower().Contains(searchValue) ||
                    pr.Marca.Nome.ToLower().Contains(searchValue));                    
            }

            // Ordenação: padrão é Data desc e HoraInicial desc
            query = orderColumn switch
            {
                0 => orderDir == "desc"
                    ? query.OrderByDescending(pr => pr.Marca.Nome)
                    : query.OrderBy(pr => pr.Marca.Nome),
                1 => orderDir == "desc"
                    ? query.OrderByDescending(pr => pr.Marca.Nome)
                    : query.OrderBy(pr => pr.Marca.Nome),
                _ => query.OrderByDescending(pr => pr.Nome) // Default
            };


            // Total de registros filtrados
            int totalRecords = await query.CountAsync();

            // Paginação após ordenação
            var records = await query
                .Skip((page - 1) * length)
                .Take(length)
                .ToListAsync();

            return (records, totalRecords);
        }

    }
}
