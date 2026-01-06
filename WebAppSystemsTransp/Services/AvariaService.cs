using System;
using WebAppSystems.Data;
using WebAppSystemsTransp.Models;

namespace WebAppSystemsTransp.Services
{
    public class AvariaService
    {
        private readonly WebAppSystemsContext _context;

        public AvariaService(WebAppSystemsContext context)
        {
            _context = context;
        }

        public void AddAvaria(Avaria avaria)
        {
            _context.Avaria.Add(avaria);
            _context.SaveChanges();
        }
    }

}
