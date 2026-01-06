using NuGet.Protocol.Plugins;
using Org.BouncyCastle.Crypto.Agreement.JPake;
using WebAppSystems.Models;
using WebAppSystems.Models.Enums;

namespace WebAppSystems.Data
{
    public class SeedingService
    {

        private WebAppSystemsContext _context;
        public SeedingService(WebAppSystemsContext context)
        {
            _context = context;     
        }
        

        public void Seed()
        {
            if (_context.Attorney.Any())          
               
            {
                return;
            }          
            Attorney a1 = new Attorney("Administrador", "jrwippel@hotmail.com", "47 9 99346159", new DateTime(1998, 4, 21), ProfileEnum.Admin, "7c4a8d09ca3762af61e59520943dc26494f8941b", new DateTime(1998, 4, 21), new DateTime(1998, 4, 21), "admin");
            _context.Attorney.AddRange(a1);

            _context.SaveChanges();
        }
    }
}
