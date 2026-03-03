using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SistemaIgreja.API.Services;

/// <summary>
/// Extrai título, data, descrição e texto de uma URL de notícia (meta tags e conteúdo).
/// </summary>
public class NoticiaUrlExtractorService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public NoticiaUrlExtractorService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<NoticiaExtraidaDto?> ExtrairAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        url = url.Trim();
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            url = "https://" + url;

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        client.Timeout = TimeSpan.FromSeconds(15);

        string html;
        try
        {
            var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            html = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(html))
            return null;

        var titulo = ExtrairMetaContent(html, "og:title")
            ?? ExtrairMetaContent(html, "twitter:title")
            ?? ExtrairTitle(html);

        var dataStr = ExtrairMetaContent(html, "article:published_time")
            ?? ExtrairMetaContent(html, "date")
            ?? ExtrairMetaContent(html, "publish_date");
        DateTime? data = null;
        if (!string.IsNullOrWhiteSpace(dataStr) && DateTime.TryParse(dataStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
            data = parsed;

        var texto = ExtrairCorpoTexto(html);

        // Descrição: 1) trecho abaixo do título no HTML  2) primeira frase/parágrafo do corpo  3) meta (se não for autor/site)  4) início do texto
        var descricao = ExtrairTrechoAbaixoDoTitulo(html)
            ?? (string.IsNullOrWhiteSpace(texto) ? null : ExtrairPrimeiraFraseOuParagrafo(texto));

        if (string.IsNullOrWhiteSpace(descricao))
        {
            var metaDesc = ExtrairMetaContent(html, "og:description")
                ?? ExtrairMetaContent(html, "twitter:description")
                ?? ExtrairMetaContent(html, "description", byProperty: false);
            if (!DescricaoPareceAutorOuSite(metaDesc))
                descricao = metaDesc?.Trim();
        }

        if (string.IsNullOrWhiteSpace(descricao) && !string.IsNullOrWhiteSpace(texto))
        {
            var inicio = texto!.Trim();
            if (inicio.Length > 320)
            {
                var corte = inicio.Substring(0, 320);
                var ultimoEspaco = corte.LastIndexOf(' ');
                descricao = ultimoEspaco > 150 ? corte.Substring(0, ultimoEspaco) + "…" : corte.TrimEnd() + "…";
            }
            else
                descricao = inicio;
        }

        var imagemUrl = ExtrairMetaContent(html, "og:image")
            ?? ExtrairMetaContent(html, "twitter:image");

        return new NoticiaExtraidaDto
        {
            Titulo = titulo?.Trim() ?? "",
            Descricao = descricao?.Trim() ?? "",
            Texto = texto?.Trim() ?? "",
            Data = data,
            Url = url,
            ImagemUrl = imagemUrl?.Trim()
        };
    }

    /// <summary>
    /// Extrai o trecho que aparece logo abaixo do título (subtítulo/lead) em páginas de notícia.
    /// </summary>
    private static string? ExtrairTrechoAbaixoDoTitulo(string html)
    {
        const int maxLength = 600;
        // sub-title é usado pelo CPAD News para o trecho logo abaixo do título
        var classes = new[] { "sub-title", "subtitle", "lead", "excerpt", "summary", "resumo", "chapeu", "article-lead", "entry-summary", "post-excerpt", "single-excerpt", "article-description", "post-description", "description", "intro", "article-intro", "entry-excerpt", "tdb-block-inner" };
        foreach (var className in classes)
        {
            // div ou p com a classe (pode ter outras classes junto)
            var pattern = $@"<(?:div|p)[^>]*class=[""'][^""']*{Regex.Escape(className)}[^""']*[""'][^>]*>([\s\S]*?)</(?:div|p)>";
            var m = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
            if (!m.Success) continue;
            var inner = m.Groups[1].Value;
            var texto = StripTags(inner);
            if (string.IsNullOrWhiteSpace(texto)) continue;
            texto = texto.Trim();
            if (texto.Length < 10) continue; // descarte trechos mínimos
            return texto.Length > maxLength ? texto.Substring(0, maxLength).TrimEnd() + "…" : texto;
        }
        // Alguns sites usam o primeiro <p> dentro de article ou de um bloco de conteúdo
        var articleMatch = Regex.Match(html, @"<article[^>]*>[\s\S]*?<(?:p|div)[^>]*>([\s\S]*?)</(?:p|div)>", RegexOptions.IgnoreCase);
        if (articleMatch.Success)
        {
            var firstBlock = StripTags(articleMatch.Groups[1].Value).Trim();
            if (firstBlock.Length >= 20 && firstBlock.Length <= maxLength)
                return firstBlock;
            if (firstBlock.Length > maxLength)
                return firstBlock.Substring(0, maxLength).TrimEnd() + "…";
        }
        // CPAD e similares: procurar qualquer <p> ou <div> cujo texto pareça lead (ex: "Segundo a Portas Abertas...")
        var allBlocks = Regex.Matches(html, @"<(?:p|div)[^>]*>([\s\S]*?)</(?:p|div)>", RegexOptions.IgnoreCase);
        foreach (Match b in allBlocks)
        {
            var inner = StripTags(b.Groups[1].Value).Trim();
            if (inner.Length < 30) continue;
            var isLead = inner.StartsWith("Segundo ", StringComparison.OrdinalIgnoreCase) ||
                inner.StartsWith("Segundo a ", StringComparison.OrdinalIgnoreCase) ||
                inner.StartsWith("De acordo com ", StringComparison.OrdinalIgnoreCase) ||
                inner.StartsWith("Conforme ", StringComparison.OrdinalIgnoreCase);
            if (!isLead) continue;
            // Se o bloco for curto, usar inteiro; senão usar só a primeira frase
            if (inner.Length <= 400)
                return inner.Length > maxLength ? inner.Substring(0, maxLength).TrimEnd() + "…" : inner;
            var primeiraFrase = Regex.Match(inner, @"^([^.]{10,400})\.\s");
            if (primeiraFrase.Success)
                return primeiraFrase.Groups[1].Value.Trim() + ".";
        }
        return null;
    }

    /// <summary>
    /// Extrai a primeira frase ou primeiro parágrafo curto do texto do artigo (ótimo para lead).
    /// </summary>
    private static string? ExtrairPrimeiraFraseOuParagrafo(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        texto = texto.Trim();
        // Parágrafos (separados por quebra dupla)
        var paragrafos = Regex.Split(texto, @"\n\s*\n|\r\n\s*\r\n");
        foreach (var p in paragrafos)
        {
            var t = p.Trim();
            if (t.Length >= 50 && t.Length <= 450) return t;
        }
        // Primeira frase (até ponto seguido de espaço)
        var match = Regex.Match(texto, @"^([^.]{40,450})\.\s", RegexOptions.Singleline);
        if (match.Success) return match.Groups[1].Value.Trim() + ".";
        return null;
    }

    /// <summary>
    /// Rejeita meta description quando for claramente autor/redação/site (ex: "Redação CPAD News Website").
    /// </summary>
    private static bool DescricaoPareceAutorOuSite(string? meta)
    {
        if (string.IsNullOrWhiteSpace(meta) || meta.Length < 25) return true;
        var m = meta.Trim();
        if (m.Length > 200) return false; // descrições longas são válidas
        if (m.Contains("Website", StringComparison.OrdinalIgnoreCase)) return true;
        if (m.Contains("Redação", StringComparison.OrdinalIgnoreCase) && m.Length < 80) return true;
        if (Regex.IsMatch(m, @"^By\s+\w+", RegexOptions.IgnoreCase)) return true;
        return false;
    }

    private static string? ExtrairMetaContent(string html, string propertyOrName, bool byProperty = true)
    {
        var attr = byProperty ? "property" : "name";
        var pattern = $@"<meta\s+[^>]*{attr}=[""'](?:[^""']*){Regex.Escape(propertyOrName)}[^""']*[""'][^>]*content=[""']([^""']*)[""']";
        var m = Regex.Match(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (m.Success)
            return DecodeHtml(m.Groups[1].Value);
        pattern = $@"<meta\s+[^>]*content=[""']([^""']*)[""'][^>]*{attr}=[""'](?:[^""']*){Regex.Escape(propertyOrName)}[^""']*[""']";
        m = Regex.Match(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return m.Success ? DecodeHtml(m.Groups[1].Value) : null;
    }

    private static string? ExtrairTitle(string html)
    {
        var m = Regex.Match(html, @"<title[^>]*>([^<]*)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return m.Success ? DecodeHtml(m.Groups[1].Value).Trim() : null;
    }

    private static string? ExtrairCorpoTexto(string html)
    {
        // Tenta primeiro <article> e <main>
        foreach (var tag in new[] { "article", "main" })
        {
            var pattern = $@"<{tag}[^>]*>([\s\S]*?)</{tag}>";
            var m = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
            if (!m.Success) continue;
            var texto = StripTags(m.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(texto) && texto.Length > 100)
                return texto.Length > 5000 ? texto.Substring(0, 5000) + "…" : texto;
        }
        // Tenta divs com classes comuns de conteúdo
        foreach (var className in new[] { "post-content", "entry-content", "content-body", "article-body", "single-content", "post-body", "conteudo", "content" })
        {
            var pattern = $@"<div[^>]*class=[""'][^""']*{Regex.Escape(className)}[^""']*[""'][^>]*>([\s\S]*?)</div>";
            var m = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
            if (!m.Success) continue;
            var texto = StripTags(m.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(texto) && texto.Length > 100)
                return texto.Length > 5000 ? texto.Substring(0, 5000) + "…" : texto;
        }
        var full = StripTags(html);
        if (!string.IsNullOrWhiteSpace(full) && full.Length > 150)
            return full.Length > 5000 ? full.Substring(0, 5000) + "…" : full;
        return null;
    }

    private static string StripTags(string html)
    {
        html = Regex.Replace(html, @"<script[\s\S]*?</script>", " ", RegexOptions.IgnoreCase);
        html = Regex.Replace(html, @"<style[\s\S]*?</style>", " ", RegexOptions.IgnoreCase);
        html = Regex.Replace(html, @"<[^>]+>", " ", RegexOptions.IgnoreCase);
        html = DecodeHtml(html);
        html = Regex.Replace(html, @"\s+", " ").Trim();
        return html;
    }

    private static string DecodeHtml(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return System.Net.WebUtility.HtmlDecode(text);
    }
}

public class NoticiaExtraidaDto
{
    public string Titulo { get; set; } = "";
    public string Descricao { get; set; } = "";
    public string Texto { get; set; } = "";
    public DateTime? Data { get; set; }
    public string? Url { get; set; }
    /// <summary>URL da imagem de destaque (og:image) para baixar e usar na notícia.</summary>
    public string? ImagemUrl { get; set; }
}

public class ExtrairNoticiaUrlRequest
{
    public string Url { get; set; } = "";
}
