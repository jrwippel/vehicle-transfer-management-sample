using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppSystems.Data;
using WebAppSystems.Models.Enums;
using WebAppSystemsTransp.Models;
using WebAppSystemsTransp.Models.Dto;
using WebAppSystemsTransp.Models.Enums;
using iText.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text.pdf.draw;
using System.Drawing.Imaging;

using System.Drawing;

namespace WebAppSystemsTransp.Services


{
    public class PedidoPdfService
    {
        private readonly WebAppSystemsContext _context;
        private readonly BlobStorageService _blobStorageService;

        public PedidoPdfService(WebAppSystemsContext context, BlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        // Método auxiliar para converter valores 1 a 5 para suas respectivas descrições
        string ConverterCombustivel(int? valor)
        {
            return valor switch
            {
                1 => "Reserva",
                2 => "1/4",
                3 => "1/2",
                4 => "3/4",
                5 => "Cheio (F)",
                _ => "Desconhecido"
            };
        }

        public async Task<FileStreamResult> GeneratePdfAsync(int id)
        {
            try
            {



                // Obtém os dados do pedido do banco
                var pedido = await _context.Pedido
                    .Include(p => p.Veiculo)
                    .Include(p => p.ClienteCarga)
                    .Include(p => p.ClienteDescarga)
                    .Include(p => p.Motorista)
                    .FirstOrDefaultAsync(p => p.Id == id);



                // Criar nome de arquivo concatenando ID e Hora de Execução
                string fileName = $"GuiaTransporte_{id}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string outputPath = Path.Combine(Path.GetTempPath(), fileName);

                using (FileStream fs = new FileStream(outputPath, FileMode.Create))
                {
                    Document doc = new Document(PageSize.A4);
                    PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                    doc.Open();

                    // Definição da fonte
                    iTextSharp.text.Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                    iTextSharp.text.Font fieldFont = FontFactory.GetFont(FontFactory.COURIER, 12);

                    // Título do documento
                    Paragraph title = new Paragraph("Guia de Transporte", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    doc.Add(title);
                    doc.Add(new Paragraph("\n"));

                    // Criar a linha tracejada
                    // Definição das fontes
                    iTextSharp.text.Font labelFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, iTextSharp.text.Font.BOLD);


                    // Linha tracejada (pode ser ajustada conforme necessário)
                    LineSeparator dottedLine = new LineSeparator(1, 100, BaseColor.BLACK, Element.ALIGN_CENTER, 2);

                    // Adiciona a linha tracejada
                    doc.Add(new Chunk(dottedLine));

                    // Adiciona os textos formatados lado a lado (Label e Campo)
                    Paragraph pedidoParagraph = new Paragraph();
                    pedidoParagraph.Add(new Chunk("Pedido: ", labelFont));
                    pedidoParagraph.Add(new Chunk(pedido.Id.ToString(), fieldFont));
                    doc.Add(pedidoParagraph);


                    // Adiciona Matrícula
                    Paragraph matriculaParagraph = new Paragraph();
                    matriculaParagraph.Add(new Chunk("Matrícula: ", labelFont));
                    matriculaParagraph.Add(new Chunk(pedido.Veiculo.Matricula, fieldFont));
                    doc.Add(matriculaParagraph);

                    doc.Add(new Paragraph("\n")); // Adiciona uma linha em branco

                    // Linha tracejada para separar
                    doc.Add(dottedLine);  // Adiciona a linha tracejada diretamente

                    // Adiciona Cliente Carga
                    Paragraph clienteCargaParagraph = new Paragraph();
                    clienteCargaParagraph.Add(new Chunk("Cliente Carga: ", labelFont));
                    clienteCargaParagraph.Add(new Chunk(pedido.ClienteCarga.Nome, fieldFont));
                    doc.Add(clienteCargaParagraph);

                    // Adiciona Endereço Carga
                    Paragraph enderecoCargaParagraph = new Paragraph();
                    enderecoCargaParagraph.Add(new Chunk("Endereço Carga: ", labelFont));
                    enderecoCargaParagraph.Add(new Chunk(pedido.ClienteCarga.Local, fieldFont));
                    doc.Add(enderecoCargaParagraph);

                    // Adiciona Data Carga
                    Paragraph dataCargaParagraph = new Paragraph();
                    dataCargaParagraph.Add(new Chunk("Data Carga: ", labelFont));
                    dataCargaParagraph.Add(new Chunk(pedido.DataExecucao?.ToString("dd/MM/yyyy") ?? "Não informado", fieldFont));
                    doc.Add(dataCargaParagraph);

                    // Adiciona Hora Carga
                    Paragraph horaCargaParagraph = new Paragraph();
                    horaCargaParagraph.Add(new Chunk("Hora Carga: ", labelFont));
                    horaCargaParagraph.Add(new Chunk(pedido.HoraInicio?.ToString(@"hh\:mm\:ss") ?? "Não informado", fieldFont));
                    doc.Add(horaCargaParagraph);

                    // Adiciona KM Carga
                    Paragraph kmCargaParagraph = new Paragraph();
                    kmCargaParagraph.Add(new Chunk("KM Carga: ", labelFont));
                    kmCargaParagraph.Add(new Chunk(pedido.KmInicial.ToString(), fieldFont));
                    doc.Add(kmCargaParagraph);

                    // Combustível
                    Paragraph combustivelParagraph = new Paragraph();
                    combustivelParagraph.Add(new Chunk("Combustível: ", labelFont));
                    combustivelParagraph.Add(new Chunk(ConverterCombustivel(pedido.CombustivelInicial), fieldFont));
                    doc.Add(combustivelParagraph);


                    // Método auxiliar para converter valores 1 e 2 para "Sim" e "Não"
                    string ConverterParaDescricao(int? valor)
                    {
                        return valor == 1 ? "Sim" : valor == 2 ? "Não" : "Desconhecido";
                    }

                    // Triângulo Homologado
                    Paragraph trianguloHomologadoParagraph = new Paragraph();
                    trianguloHomologadoParagraph.Add(new Chunk("Triângulo Homologado: ", labelFont));
                    trianguloHomologadoParagraph.Add(new Chunk(ConverterParaDescricao(pedido.TrianguloHomologado), fieldFont));
                    doc.Add(trianguloHomologadoParagraph);

                    // Colete Homologado
                    Paragraph coleteHomologadoParagraph = new Paragraph();
                    coleteHomologadoParagraph.Add(new Chunk("Colete Homologado: ", labelFont));
                    coleteHomologadoParagraph.Add(new Chunk(ConverterParaDescricao(pedido.ColeteHomologado), fieldFont));
                    doc.Add(coleteHomologadoParagraph);

                    // Documento do Veículo
                    Paragraph documentoVeiculoParagraph = new Paragraph();
                    documentoVeiculoParagraph.Add(new Chunk("Documento Veículo: ", labelFont));
                    documentoVeiculoParagraph.Add(new Chunk(ConverterParaDescricao(pedido.DocumentoVeiculo), fieldFont));
                    doc.Add(documentoVeiculoParagraph);

                    // Documento do Seguro
                    Paragraph documentoSeguroParagraph = new Paragraph();
                    documentoSeguroParagraph.Add(new Chunk("Documento Seguro: ", labelFont));
                    documentoSeguroParagraph.Add(new Chunk(ConverterParaDescricao(pedido.DocumentoSeguro), fieldFont));
                    doc.Add(documentoSeguroParagraph);

                    if (pedido.TempoEsperaCarga.HasValue)
                    {
                        Paragraph tempoEsperaCargaParagraph = new Paragraph();
                        tempoEsperaCargaParagraph.Add(new Chunk("Tempo Espera Carga: ", labelFont));
                        tempoEsperaCargaParagraph.Add(new Chunk(pedido.TempoEsperaCarga.Value.ToString(@"hh\:mm\:ss"), fieldFont));
                        doc.Add(tempoEsperaCargaParagraph);
                    }

                    if (!string.IsNullOrWhiteSpace(pedido.ObservacaoCarga))
                    {
                        // Observação Carga
                        Paragraph observacaoCargaParagraph = new Paragraph();
                        observacaoCargaParagraph.Add(new Chunk("Observação Carga: ", labelFont));
                        observacaoCargaParagraph.Add(new Chunk(pedido.ObservacaoCarga ?? "Não informada", fieldFont));
                        doc.Add(observacaoCargaParagraph);
                    }


                    doc.Add(new Paragraph("\n")); // Linha em branco após os campos de carga

                    // Linha tracejada para separar
                    doc.Add(dottedLine);  // Linha tracejada

                    // Adiciona Cliente Descarga
                    Paragraph clienteDescargaParagraph = new Paragraph();
                    clienteDescargaParagraph.Add(new Chunk("Cliente Descarga: ", labelFont));
                    clienteDescargaParagraph.Add(new Chunk(pedido.ClienteDescarga.Nome, fieldFont));
                    doc.Add(clienteDescargaParagraph);

                    // Adiciona Endereço Descarga
                    Paragraph enderecoDescargaParagraph = new Paragraph();
                    enderecoDescargaParagraph.Add(new Chunk("Endereço Descarga: ", labelFont));
                    enderecoDescargaParagraph.Add(new Chunk(pedido.ClienteDescarga.Local, fieldFont));
                    doc.Add(enderecoDescargaParagraph);

                    // Adiciona Data Descarga
                    Paragraph dataDescargaParagraph = new Paragraph();
                    dataDescargaParagraph.Add(new Chunk("Data Descarga: ", labelFont));
                    dataDescargaParagraph.Add(new Chunk(pedido.DataExecucao?.ToString("dd/MM/yyyy") ?? "Não informado", fieldFont));
                    doc.Add(dataDescargaParagraph);

                    // Adiciona Hora Descarga
                    Paragraph horaDescargaParagraph = new Paragraph();
                    horaDescargaParagraph.Add(new Chunk("Hora Descarga: ", labelFont));
                    horaDescargaParagraph.Add(new Chunk(pedido.HoraFinal?.ToString(@"hh\:mm\:ss") ?? "Não informado", fieldFont));
                    doc.Add(horaDescargaParagraph);

                    // Adiciona KM Descarga
                    Paragraph kmDescargaParagraph = new Paragraph();
                    kmDescargaParagraph.Add(new Chunk("KM Descarga: ", labelFont));
                    kmDescargaParagraph.Add(new Chunk(pedido.KmFinal.ToString(), fieldFont));
                    doc.Add(kmDescargaParagraph);

                    // Combustível
                    Paragraph combustivelFinalParagraph = new Paragraph();
                    combustivelFinalParagraph.Add(new Chunk("Combustível Final: ", labelFont));
                    combustivelFinalParagraph.Add(new Chunk(ConverterCombustivel(pedido.CombustivelFinal), fieldFont));
                    doc.Add(combustivelFinalParagraph);

                    if (pedido.TempoEsperaDescarga.HasValue)
                    {
                        Paragraph tempoEsperaDescargaParagraph = new Paragraph();
                        tempoEsperaDescargaParagraph.Add(new Chunk("Tempo Espera Descarga: ", labelFont));
                        tempoEsperaDescargaParagraph.Add(new Chunk(pedido.TempoEsperaDescarga.Value.ToString(@"hh\:mm\:ss"), fieldFont));
                        doc.Add(tempoEsperaDescargaParagraph);
                    }

                    if (!string.IsNullOrWhiteSpace(pedido.ObservacaoDescarga))
                    {
                        // Observação Descarga
                        Paragraph observacaoDescargaParagraph = new Paragraph();
                        observacaoDescargaParagraph.Add(new Chunk("Observação Descarga: ", labelFont));
                        observacaoDescargaParagraph.Add(new Chunk(pedido.ObservacaoDescarga, fieldFont));
                        doc.Add(observacaoDescargaParagraph);
                    }

                    doc.Add(new Paragraph("\n")); // Linha em branco após os campos de descarga
                    doc.Add(dottedLine);  // Linha tracejada



                    // Buscar assinaturas do banco de dados filtrando pelo TipoFotoVeiculo AssMotorista ou AssCliente
                    var assinaturas = _context.FotoPedido
                        .Where(f => f.PedidoId == pedido.Id &&
                                   (f.TipoFotoVeiculo == TipoFotoVeiculo.AssMotorista ||
                                    f.TipoFotoVeiculo == TipoFotoVeiculo.AssCliente))
                        .ToList();



                    // Criar uma tabela para as assinaturas
                    PdfPTable table = new PdfPTable(2);
                    table.WidthPercentage = 100f;
                    table.SetWidths(new float[] { 1f, 1f }); // Definição de largura das colunas

                    // Função auxiliar para adicionar imagens ao PDF
                    void AdicionarImagemNaTabela(PdfPTable tabela, byte[] imageBytes)
                    {
                        if (imageBytes != null && imageBytes.Length > 0)
                        {
                            using (MemoryStream ms = new MemoryStream(imageBytes))
                            {
                                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(ms);
                                image.ScaleToFit(200f, 100f);

                                PdfPCell imageCell = new PdfPCell(image)
                                {
                                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                                    HorizontalAlignment = Element.ALIGN_LEFT
                                };
                                tabela.AddCell(imageCell);
                            }
                        }
                        else
                        {
                            PdfPCell emptyCell = new PdfPCell(new Paragraph("N/A")) // Exibe "N/A" se não houver assinatura
                            {
                                Border = iTextSharp.text.Rectangle.NO_BORDER,
                                HorizontalAlignment = Element.ALIGN_CENTER
                            };
                            tabela.AddCell(emptyCell);
                        }
                    }

                    // 🔹 Buscar e exibir assinaturas de CARGA (TipoFotoPedido == 1)
                    // Certifique-se de que o BlobStorageService está sendo injetado na classe, algo assim:
                    // private readonly BlobStorageService _blobStorageService;

                    // Buscar URLs para as assinaturas de CARGA
                    var assinaturaClienteCargaUrl = assinaturas
                        .FirstOrDefault(f => f.TipoPedido == TipoFotoPedido.Carga && f.TipoFotoVeiculo == TipoFotoVeiculo.AssCliente)?
                        .UrlFoto;

                    var assinaturaMotoristaCargaUrl = assinaturas
                        .FirstOrDefault(f => f.TipoPedido == TipoFotoPedido.Carga && f.TipoFotoVeiculo == TipoFotoVeiculo.AssMotorista)?
                        .UrlFoto;

                    // Baixar as imagens usando o método DownloadFileAsync
                    var assinaturaClienteCargaStream = assinaturaClienteCargaUrl != null
                        ? await _blobStorageService.DownloadFileAsync(assinaturaClienteCargaUrl)
                        : null;

                    var assinaturaMotoristaCargaStream = assinaturaMotoristaCargaUrl != null
                        ? await _blobStorageService.DownloadFileAsync(assinaturaMotoristaCargaUrl)
                        : null;

                    // Converter Stream para byte[] se não for nulo
                    byte[] assinaturaClienteCargaBytes = assinaturaClienteCargaStream != null
                        ? await StreamToByteArrayAss(assinaturaClienteCargaStream)
                        : null;

                    byte[] assinaturaMotoristaCargaBytes = assinaturaMotoristaCargaStream != null
                        ? await StreamToByteArrayAss(assinaturaMotoristaCargaStream)
                        : null;

                    // Adicionar as imagens baixadas na tabela
                    AdicionarImagemNaTabela(table, assinaturaClienteCargaBytes);
                    AdicionarImagemNaTabela(table, assinaturaMotoristaCargaBytes);

                    // Método para converter Stream em byte[]




                    table.AddCell(new PdfPCell(new Paragraph("Cliente Carga: " + pedido.NomePessoaCar, fieldFont))
                    {
                        Border = iTextSharp.text.Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });


                    table.AddCell(new PdfPCell(new Paragraph("Motorista:" + pedido.Motorista.Name, fieldFont))
                    {
                        Border = iTextSharp.text.Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });

                    // 🔹 Buscar e exibir assinaturas de DESCARGA (TipoFotoPedido == 2)


                    // Buscar URLs para as assinaturas de DESCARGA
                    var assinaturaClienteDescargaUrl = assinaturas
                        .FirstOrDefault(f => f.TipoPedido == TipoFotoPedido.Descarga && f.TipoFotoVeiculo == TipoFotoVeiculo.AssCliente)?
                        .UrlFoto;

                    var assinaturaMotoristaDescargaUrl = assinaturas
                        .FirstOrDefault(f => f.TipoPedido == TipoFotoPedido.Descarga && f.TipoFotoVeiculo == TipoFotoVeiculo.AssMotorista)?
                        .UrlFoto;

                    // Baixar as imagens usando o método DownloadFileAsync
                    var assinaturaClienteDescargaStream = assinaturaClienteDescargaUrl != null
                        ? await _blobStorageService.DownloadFileAsync(assinaturaClienteDescargaUrl)
                        : null;

                    var assinaturaMotoristaDescargaStream = assinaturaMotoristaDescargaUrl != null
                        ? await _blobStorageService.DownloadFileAsync(assinaturaMotoristaDescargaUrl)
                        : null;

                    // Converter Stream para byte[] se não for nulo
                    byte[] assinaturaClienteDescargaBytes = assinaturaClienteDescargaStream != null
                        ? await StreamToByteArrayAss(assinaturaClienteDescargaStream)
                        : null;

                    byte[] assinaturaMotoristaDescargaBytes = assinaturaMotoristaDescargaStream != null
                        ? await StreamToByteArrayAss(assinaturaMotoristaDescargaStream)
                        : null;

                    // Adicionar as imagens baixadas na tabela
                    AdicionarImagemNaTabela(table, assinaturaClienteDescargaBytes);
                    AdicionarImagemNaTabela(table, assinaturaMotoristaDescargaBytes);


                    table.AddCell(new PdfPCell(new Paragraph("Cliente Descarga:" + pedido.NomePessoaDes, fieldFont))
                    {
                        Border = iTextSharp.text.Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });

                    table.AddCell(new PdfPCell(new Paragraph("Motorista:" + pedido.Motorista.Name, fieldFont))
                    {
                        Border = iTextSharp.text.Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });

                    /*

                    // Adicionar a tabela ao documento
                    doc.Add(table);


                    // Obtém as avarias relacionadas ao pedido
                    var avarias = await _context.Avaria
                        .Where(a => a.PedidoId == id)
                        .ToListAsync();
                    if (avarias.Any())
                    {
                        // Verifica se precisa iniciar uma nova página antes de adicionar a imagem e os círculos
                        doc.NewPage();  // Cria uma nova página no PDF


                        // Adiciona um título para a legenda
                        Paragraph legendaTitulo = new Paragraph("Legenda de Avarias", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD));
                        legendaTitulo.Alignment = Element.ALIGN_CENTER;
                        legendaTitulo.SpacingAfter = 10f;
                        doc.Add(legendaTitulo);

                        // Cria uma tabela para a legenda (2 colunas: Código e Descrição)
                        PdfPTable tabelaLegenda = new PdfPTable(2);
                        tabelaLegenda.WidthPercentage = 50; // Ajusta a largura da tabela
                        tabelaLegenda.HorizontalAlignment = Element.ALIGN_CENTER;
                        tabelaLegenda.SetWidths(new float[] { 1, 4 }); // Define largura das colunas

                        // Cabeçalhos da tabela
                        PdfPCell cellCodigo = new PdfPCell(new Phrase("Código", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD)));
                        cellCodigo.BackgroundColor = new BaseColor(200, 200, 200); // Cinza claro
                        cellCodigo.HorizontalAlignment = Element.ALIGN_CENTER;
                        tabelaLegenda.AddCell(cellCodigo);

                        PdfPCell cellDescricao = new PdfPCell(new Phrase("Descrição", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD)));
                        cellDescricao.BackgroundColor = new BaseColor(200, 200, 200);
                        cellDescricao.HorizontalAlignment = Element.ALIGN_CENTER;
                        tabelaLegenda.AddCell(cellDescricao);

                        // Adiciona as linhas com os valores do enum TipoAvaria
                        foreach (TipoAvaria tipo in Enum.GetValues(typeof(TipoAvaria)))
                        {
                            tabelaLegenda.AddCell(new PdfPCell(new Phrase(((int)tipo).ToString())) { HorizontalAlignment = Element.ALIGN_CENTER });
                            tabelaLegenda.AddCell(new PdfPCell(new Phrase(tipo.ToString())));
                        }

                        // Adiciona a tabela ao documento
                        doc.Add(tabelaLegenda);

                        // Adiciona um espaço antes da imagem
                        doc.Add(new Paragraph("\n"));


                        // Defina a posição da imagem 10 cm abaixo da observação
                        float offsetY = 150f;  // A distância para descer a imagem (aproximadamente 10 cm)

                        string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avarias.png");
                        iTextSharp.text.Image avariasImage = iTextSharp.text.Image.GetInstance(imagePath);

                        // Define o tamanho da imagem
                        float imgWidth = 500f;
                        float imgHeight = 300f;

                        // Posição da imagem na página (ajustando para um deslocamento de 10 cm)
                        float imgYPosition = doc.BottomMargin + offsetY; // Ajusta o deslocamento

                        // Centraliza a imagem na página
                        avariasImage.ScaleAbsolute(imgWidth, imgHeight);
                        avariasImage.SetAbsolutePosition((PageSize.A4.Width - imgWidth) / 2, imgYPosition);
                        doc.Add(avariasImage);

                        // Agora, calcule corretamente a posição dos círculos com base na imagem
                        PdfContentByte canvas = writer.DirectContent;
                        BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        canvas.SetFontAndSize(bf, 12); // Define o tamanho da fonte

                        foreach (var avaria in avarias.OrderBy(a => a.Id))
                        {
                            // Calcula a posição relativa ao início da imagem
                            float posX = (PageSize.A4.Width - imgWidth) / 2 + (imgWidth * avaria.X / 500);  // Escala X
                            float posY = imgYPosition + (imgHeight - (imgHeight * avaria.Y / 300));  // Escala Y (invertido)

                            // Desenha um círculo vermelho
                            canvas.SetColorFill(BaseColor.RED);
                            canvas.Circle(posX, posY, 8);  // Ajuste o raio conforme necessário
                            canvas.Fill();

                            // Define a cor do texto dentro do círculo (branco)
                            canvas.SetColorFill(BaseColor.WHITE);


                            // Converte o enum para número
                            string tipoAvaria = ((int)avaria.TipoAvaria).ToString();


                            // Calcula o deslocamento para centralizar o número dentro do círculo
                            float textWidth = bf.GetWidthPoint(tipoAvaria, 12);  // Largura do número
                            float textX = posX - (textWidth / 2);  // Centraliza na horizontal
                            float textY = posY - 4;  // Ajusta para o centro vertical

                            // Adiciona o número dentro do círculo
                            canvas.BeginText();
                            canvas.SetTextMatrix(textX, textY);
                            canvas.ShowText(tipoAvaria);
                            canvas.EndText();
                        }

                    }
                    */

                    // 🔹 Nova página para fotos do TipoFotoVeiculo == 12
                    doc.NewPage();
                    Paragraph tituloFotos12 = new Paragraph("Foto Específica do Veículo", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 14, iTextSharp.text.Font.BOLD));
                    tituloFotos12.Alignment = Element.ALIGN_CENTER;
                    tituloFotos12.SpacingAfter = 15f;
                    doc.Add(tituloFotos12);

                    // Filtra apenas imagens do tipo TipoFotoVeiculo == 12
                    var imagensTipo12 = await _context.FotoPedido
                        .Where(f => f.PedidoId == id && (int)f.TipoFotoVeiculo == 12)
                        .ToListAsync();

                    foreach (var foto in imagensTipo12)
                    {
                        if (!string.IsNullOrEmpty(foto.NomeArquivo)) // NomeArquivo armazena a URL do Blob
                        {
                            var imgUrl = foto.NomeArquivo; // Obtém a URL da imagem

                            // Baixa e processa a imagem
                            var fileStreamResult = await BaixarImagem(imgUrl);
                            var imgStream = fileStreamResult?.FileStream;

                            if (imgStream != null)
                            {
                                byte[] imgBytes = await StreamToByteArray(imgStream);

                                if (imgBytes != null && imgBytes.Length > 0)
                                {
                                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imgBytes);

                                    // 🔹 Ajuste do tamanho da imagem para caber melhor na página
                                    float larguraMaxima = 400f; // Ajuste conforme necessário
                                    float alturaMaxima = 400f;
                                    img.ScaleToFit(larguraMaxima, alturaMaxima);
                                    img.Alignment = Element.ALIGN_CENTER;

                                    doc.Add(img);
                                    doc.Add(new Paragraph("\n")); // Espaço entre imagens
                                }
                            }
                        }
                    }



                    // 🔹 Nova página para fotos dos pedidos
                    doc.NewPage();
                    Paragraph tituloFotos = new Paragraph("Fotos do Pedido", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 14, iTextSharp.text.Font.BOLD));
                    tituloFotos.Alignment = Element.ALIGN_CENTER;
                    tituloFotos.SpacingAfter = 15f;
                    doc.Add(tituloFotos);

                    var imagensPedido = await _context.FotoPedido
                    .Where(f => f.PedidoId == id && (int)f.TipoFotoVeiculo >= 1 && (int)f.TipoFotoVeiculo <= 11 && (int)f.TipoFotoVeiculo != 8 && (int)f.TipoFotoVeiculo != 9)
                    .OrderBy(f => f.TipoPedido) // Ordena primeiro por TipoPedido (Carga ou Descarga)
                    .ToListAsync();


                    // Separando fotos por TipoPedido (Carga e Descarga)
                    var fotosCarga = imagensPedido.Where(f => f.TipoPedido == TipoFotoPedido.Carga).ToList();
                    var fotosDescarga = imagensPedido.Where(f => f.TipoPedido == TipoFotoPedido.Descarga).ToList();

                    async Task AdicionarFotosAoPDF(List<FotoPedido> fotos, string titulo)
                    {
                        if (fotos.Any())
                        {
                            // Adiciona um subtítulo para a seção (Carga ou Descarga)
                            Paragraph subtitulo = new Paragraph(titulo, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD));
                            subtitulo.Alignment = Element.ALIGN_LEFT;
                            subtitulo.SpacingBefore = 10f;
                            subtitulo.SpacingAfter = 5f;
                            doc.Add(subtitulo);

                            PdfPTable tabelaFotos = new PdfPTable(2); // 2 colunas para exibição lado a lado
                            tabelaFotos.WidthPercentage = 100;
                            tabelaFotos.SetWidths(new float[] { 1, 1 });

                            foreach (var foto in fotos)
                            {
                                if (!string.IsNullOrEmpty(foto.NomeArquivo)) // NomeArquivo armazena a URL do Blob
                                {
                                    // Recupera a URL do Blob Storage (substituindo ImageData)
                                    var imgUrl = foto.NomeArquivo; // NomeArquivo contém o URL do Blob

                                    // Baixa a imagem a partir do Blob Storage usando o método DownloadFileAsync
                                    //var imgStream = await _blobStorageService.DownloadFileAsync(imgUrl);
                                    // Chama o método BaixarImagem para baixar e redimensionar a imagem
                                    var fileStreamResult = await BaixarImagem(imgUrl);
                                    var imgStream = fileStreamResult?.FileStream;

                                    if (imgStream != null)
                                    {
                                        // Converte o Stream para byte[] se não for nulo
                                        byte[] imgBytes = await StreamToByteArray(imgStream);

                                        if (imgBytes != null && imgBytes.Length > 0)
                                        {
                                            iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imgBytes);

                                            // 🔹 Redimensionar a imagem para um tamanho médio
                                            float larguraMaxima = 50f; // Largura máxima permitida
                                            float alturaMaxima = 50f;  // Altura máxima permitida
                                            img.ScaleToFit(larguraMaxima, alturaMaxima);

                                            PdfPCell cellImagem = new PdfPCell(img);
                                            cellImagem.HorizontalAlignment = Element.ALIGN_CENTER;
                                            cellImagem.Border = iTextSharp.text.Rectangle.NO_BORDER;
                                            tabelaFotos.AddCell(cellImagem);
                                        }
                                    }
                                }
                            }

                            // Caso o número de imagens seja ímpar, adicionamos uma célula vazia para alinhar corretamente
                            if (fotos.Count % 2 != 0)
                            {
                                tabelaFotos.AddCell(new PdfPCell() { Border = iTextSharp.text.Rectangle.NO_BORDER });
                            }

                            doc.Add(tabelaFotos);
                        }
                    }

                    // Adiciona as fotos separadas por tipo
                    await AdicionarFotosAoPDF(fotosCarga, "📦 Fotos da Carga");
                    await AdicionarFotosAoPDF(fotosDescarga, "📤 Fotos da Descarga");


                    doc.Close();

                }
                // 🔹 Retorna o PDF como um arquivo para download
                var stream = new FileStream(outputPath, FileMode.Open, FileAccess.Read);
                return new FileStreamResult(stream, "application/pdf")
                {
                    FileDownloadName = $"Pedido_{id}.pdf"
                };


            }
            catch (Exception ex)
            {
                // 🔹 Log do erro (caso tenha um logger, pode substituir pelo log real)
                Console.WriteLine($"Erro ao gerar PDF: {ex.Message}");

                // Retorna um erro interno para o usuário
                return null; // ou lançar uma exceção dependendo da sua necessidade
            }
        }

        private async Task<byte[]> StreamToByteArrayAss(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                return ms.ToArray();
            }
        }
        public async Task<byte[]> StreamToByteArray(Stream inputStream)
        {
            if (inputStream == null)
            {
                return null;
            }

            using (var image = System.Drawing.Image.FromStream(inputStream))
            {
                using (var ms = new MemoryStream())
                {
                    // Define a qualidade da imagem
                    var qualityParam = new EncoderParameter(Encoder.Quality, 50L); // 50L é a qualidade da imagem (0L-100L)
                    var jpegCodec = GetEncoderInfo(System.Drawing.Imaging.ImageFormat.Jpeg);
                    var encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = qualityParam;

                    // Salva a imagem com a qualidade definida
                    image.Save(ms, jpegCodec, encoderParams);

                    return ms.ToArray();
                }
            }
        }

        private ImageCodecInfo GetEncoderInfo(System.Drawing.Imaging.ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        public async Task<FileStreamResult> BaixarImagem(string fileName)
        {
            var fileStream = await _blobStorageService.DownloadFileAsync(fileName);

            using (var image = System.Drawing.Image.FromStream(fileStream)) // Carrega a imagem original
            {
                var resizedImageStream = new MemoryStream();
                var encoder = ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 30L); // Ajuste a qualidade aqui (0 a 100)

                // Salva a imagem redimensionada e compactada no stream
                image.Save(resizedImageStream, encoder, encoderParameters);
                resizedImageStream.Position = 0;

                return new FileStreamResult(resizedImageStream, "image/jpeg")
                {
                    FileDownloadName = fileName
                };
            }
        }
    }
}