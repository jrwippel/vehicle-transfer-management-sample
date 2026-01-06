using DocumentFormat.OpenXml.Bibliography;
using System.ComponentModel.DataAnnotations;
using WebAppSystems.Helper;
using WebAppSystems.Models.Enums;
using WebAppSystemsTransp.Models;

namespace WebAppSystems.Models
{
    public class Attorney
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "{0} required")]
        [StringLength(60, MinimumLength = 3, ErrorMessage = "{0} Tamanho deveria ser entre 3 e 60")]
        public string Name { get; set; }

        [EmailAddress(ErrorMessage = "Digite um email válido")]
        [Required(ErrorMessage = "{0} required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "{0} required")]
        public string Phone { get; set; }

        [Display(Name = "Data de Nascimento")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime BirthDate { get; set; }    
     
        public ProfileEnum Perfil { get; set; }
        public string Password { get; set; }
        public DateTime RegisterDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string Login { get; set; }
        public string? Documento { get; set; }

        public Attorney()
        {
        }

        public Attorney(string name, string email, string phone, DateTime birthDate, ProfileEnum perfil, string password, DateTime registerDate, DateTime? updateDate, string login)
        {
            Name = name;
            Email = email;
            Phone = phone;
            BirthDate = birthDate;           
            Perfil = perfil;
            Password = password;
            RegisterDate = registerDate;
            UpdateDate = updateDate;
            Login = login;
        }

        public Attorney(int id, string name, string email, string phone, DateTime birthDate, ProfileEnum perfil, string password, DateTime? updateDate, string login)
        {
            Id = id;
            Name = name;
            Email = email;
            Phone = phone;
            BirthDate = birthDate;      
            Perfil = perfil;
            Password = password;
         // RegisterDate = registerDate;
            UpdateDate = updateDate;
            Login = login;
        }
 

        public bool ValidaSenha(string password)
        {
            return Password == password.GerarHash();
        }

        public void SetSenhaHash()
        {
            Password = Password.GerarHash();
        }
        public string GerarNovaSenha()
        {
            string novaSenha = Guid.NewGuid().ToString().Substring(0, 8);
            Password = novaSenha.GerarHash();
            return novaSenha;
        }
        public void SetNovaSenha(string novaSenha)
        {
            Password = novaSenha.GerarHash();
        }
    }
}
