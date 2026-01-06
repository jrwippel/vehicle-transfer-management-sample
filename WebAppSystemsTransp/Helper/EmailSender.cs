using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using WebAppSystems.Helper;

public class EmailSender  
{
    private readonly string _clientId = "1d4277da-242a-4b29-b789-36ce1a992f7f";
    private readonly string _clientSecret = "67a8630d-a16a-4ba9-8a75-4172469e8d43";
    private readonly string _tenantId = "9386e41d-725b-406c-a2cc-923493b054a3";
    private readonly string _userEmail = "jrwsolucoesti@hotmail.com";

    // O método é síncrono e retorna um bool
    public bool Enviar(string destinatario, string assunto, string mensagem)
    {
        string accessToken = GetAccessToken();  // Não precisa de await

        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var emailContent = new
                {
                    message = new
                    {
                        subject = assunto,
                        body = new { contentType = "HTML", content = mensagem },
                        toRecipients = new[] {
                            new { emailAddress = new { address = destinatario } }
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(emailContent);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Usando .GetAwaiter().GetResult() para chamar a versão síncrona
                var response = client.PostAsync($"https://graph.microsoft.com/v1.0/users/{_userEmail}/sendMail", content).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode(); // Garante que o status da resposta seja 2xx

                return true;
            }
        }
        catch (Exception ex)
        {
            // Gravar log de erro ao fazer o envio de email
            Console.WriteLine($"Erro ao enviar email: {ex.Message}");
            return false;  // Retorna false se houver erro
        }
    }

    private string GetAccessToken()
    {
        var app = ConfidentialClientApplicationBuilder.Create(_clientId)
            .WithClientSecret(_clientSecret)
            .WithAuthority($"https://login.microsoftonline.com/{_tenantId}")
            .Build();

        var result = app.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" }).ExecuteAsync().GetAwaiter().GetResult();
        return result.AccessToken;
    }
}
