using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SR.Api.Servicios;

public class PdfService
{
    const string Negro = "#000000";
    const string Crema = "#F3EEE3";
    const string Cian = "#04B9F8";
    const string Naranja = "#FE9E10";
    const string Rojo = "#F80343";
    const string Gris = "#D8D0BD";

    readonly byte[]? _logo;

    public PdfService(IWebHostEnvironment env)
    {
        var ruta = Path.Combine(AppContext.BaseDirectory, "Recursos", "logo-claro.png");
        if (!File.Exists(ruta)) ruta = Path.Combine(env.ContentRootPath, "Recursos", "logo-claro.png");
        _logo = File.Exists(ruta) ? File.ReadAllBytes(ruta) : null;
    }

    public byte[] Tabla(string titulo, string subtitulo, string[] columnas, IEnumerable<string[]> filas,
                        string[]? totales = null, int[]? alinearDerecha = null)
    {
        alinearDerecha ??= [];
        var lista = filas.ToList();

        return Document.Create(doc => doc.Page(p =>
        {
            p.Size(PageSizes.A4);
            p.Margin(32);
            p.DefaultTextStyle(t => t.FontSize(10).FontFamily("Arial").FontColor(Negro));

            p.Header().Column(col =>
            {
                col.Item().Row(r =>
                {
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().Text("SR Distribución de Máquinas Recreativas").FontSize(9).SemiBold();
                        c.Item().PaddingTop(4).Text(titulo).FontSize(20).Bold().Italic();
                        c.Item().Text(subtitulo).FontSize(10).FontColor("#555555");
                    });
                    if (_logo is not null)
                        r.ConstantItem(110).AlignRight().AlignMiddle().Image(_logo).FitArea();
                });
                col.Item().PaddingTop(10).Height(4).Background(Cian);
                col.Item().PaddingTop(2).Height(4).Background(Naranja);
                col.Item().PaddingTop(2).Height(4).Background(Rojo);
                col.Item().Height(14);
            });

            p.Content().Table(t =>
            {
                t.ColumnsDefinition(cd =>
                {
                    foreach (var _ in columnas) cd.RelativeColumn();
                });

                t.Header(h =>
                {
                    for (int i = 0; i < columnas.Length; i++)
                    {
                        var celda = h.Cell().Background(Negro).PaddingVertical(6).PaddingHorizontal(5);
                        var texto = alinearDerecha.Contains(i) ? celda.AlignRight() : celda.AlignLeft();
                        texto.Text(columnas[i]).FontColor(Crema).Bold().FontSize(9.5f);
                    }
                });

                if (lista.Count == 0)
                {
                    t.Cell().ColumnSpan((uint)columnas.Length).Padding(10).AlignCenter()
                        .Text("No hay datos para los filtros elegidos.").Italic();
                }

                for (int f = 0; f < lista.Count; f++)
                {
                    var fondo = f % 2 == 0 ? "#FFFFFF" : "#F8F5EE";
                    for (int i = 0; i < columnas.Length; i++)
                    {
                        var valor = i < lista[f].Length ? lista[f][i] : "";
                        var celda = t.Cell().Background(fondo).BorderBottom(0.5f).BorderColor(Gris).PaddingVertical(5).PaddingHorizontal(5);
                        (alinearDerecha.Contains(i) ? celda.AlignRight() : celda.AlignLeft()).Text(valor);
                    }
                }

                if (totales is not null)
                {
                    for (int i = 0; i < columnas.Length; i++)
                    {
                        var valor = i < totales.Length ? totales[i] : "";
                        var celda = t.Cell().BorderTop(1.5f).BorderColor(Naranja).PaddingVertical(6).PaddingHorizontal(5);
                        (alinearDerecha.Contains(i) ? celda.AlignRight() : celda.AlignLeft()).Text(valor).Bold();
                    }
                }
            });

            p.Footer().Row(r =>
            {
                r.RelativeItem().Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor("#777777");
                r.RelativeItem().AlignRight().Text(x =>
                {
                    x.DefaultTextStyle(s => s.FontSize(8).FontColor("#777777"));
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        })).GeneratePdf();
    }
}
