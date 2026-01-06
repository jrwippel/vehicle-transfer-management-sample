using WebAppSystems.Data;
using WebAppSystemsTransp.Models;

namespace WebAppSystemsTransp.Services
{
    public class FotoService
    {
        private readonly WebAppSystemsContext _context; // DbContext do seu projeto

        public FotoService(WebAppSystemsContext context)
        {
            _context = context;
        }

        public async Task<FotoPedido> GetByIdAsync(int id)
        {
            return await _context.FotoPedido.FindAsync(id);
        }
    }
}
