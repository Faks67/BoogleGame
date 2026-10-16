using System.Text.Json;
using BoogleGame.Web.Moteur;

var builder = WebApplication.CreateBuilder(args);

string dossier = AppContext.BaseDirectory;
var francais = new Lexique("FR", new[] { Path.Combine(dossier, "MotsPossiblesFR.txt") });
var anglais = new Lexique("EN", new[] { Path.Combine(dossier, "MotsPossiblesEN.txt") });
var lexiques = new Dictionary<string, Lexique>
{
    ["FR"] = francais,
    ["EN"] = anglais,
    // Mixte : un mot est accepté s'il existe en français ou en anglais
    ["MIX"] = new Lexique("MIX", new[] { Path.Combine(dossier, "MotsPossiblesFR.txt"), Path.Combine(dossier, "MotsPossiblesEN.txt") })
};

builder.Services.AddMemoryCache(o => o.SizeLimit = 500);
builder.Services.AddSingleton(sp => new Parties(lexiques, sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>()));
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

var app = builder.Build();

// Le navigateur revalide les fichiers du site à chaque visite : une mise à jour est vue tout de suite
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "no-cache"
});

static object Decrire(Partie p) => new
{
    id = p.Id,
    langue = p.Langue,
    taille = p.Taille,
    cases = p.Plateau.Cases,
    nombreMots = p.Solutions.Count,
    pointsMax = p.PointsMax,
    motLePlusLong = p.Solutions.Count == 0 ? 0 : p.Solutions.Max(s => s.Mot.Length)
};

static IResult Erreur(string message, int code = 400) => Results.Json(new { erreur = message }, statusCode: code);

app.MapGet("/api/infos", () => new
{
    langues = new[] { new { code = "FR", nom = "Français", mots = francais.Taille }, new { code = "EN", nom = "Anglais", mots = anglais.Taille }, new { code = "MIX", nom = "Mixte", mots = lexiques["MIX"].Taille } },
    tailles = Parties.Tailles
});

app.MapPost("/api/parties", (NouvellePartie demande, Parties parties) =>
{
    string langue = (demande.Langue ?? "FR").ToUpperInvariant();
    if (!parties.LangueConnue(langue)) return Erreur("Langue inconnue.");
    if (!Parties.Tailles.Contains(demande.Taille)) return Erreur("Taille de plateau invalide.");
    int graine = demande.Graine ?? Random.Shared.Next(1, 1_000_000_000);
    return Results.Ok(Decrire(parties.Nouvelle(langue, demande.Taille, graine)));
});

app.MapGet("/api/parties/jour", (string? langue, Parties parties) =>
{
    langue = (langue ?? "FR").ToUpperInvariant();
    if (!parties.LangueConnue(langue)) return Erreur("Langue inconnue.");
    var partie = parties.DuJour(langue, DateOnly.FromDateTime(DateTime.Now));
    return Results.Ok(new { date = DateTime.Now.ToString("yyyy-MM-dd"), partie = Decrire(partie) });
});

app.MapGet("/api/parties/{id}", (string id, Parties parties) =>
    parties.Trouver(id) is Partie p ? Results.Ok(Decrire(p)) : Erreur("Partie introuvable.", 404));

// Vérifie un mot proposé : présent sur le plateau et dans le dictionnaire
app.MapPost("/api/parties/{id}/mots", (string id, MotPropose proposition, Parties parties) =>
{
    if (parties.Trouver(id) is not Partie partie) return Erreur("Partie introuvable.", 404);
    string mot = new string((proposition.Mot ?? "").ToUpperInvariant().Where(char.IsLetter).ToArray());
    if (mot.Length < 3) return Results.Ok(new { mot, valide = false, raison = "trop-court" });
    int[]? chemin = partie.Plateau.Chercher(mot);
    if (chemin == null) return Results.Ok(new { mot, valide = false, raison = "absent-plateau" });
    if (!parties.Lexique(partie.Langue).Contient(mot)) return Results.Ok(new { mot, valide = false, raison = "absent-dictionnaire", chemin });
    return Results.Ok(new { mot, valide = true, points = Plateau.Points(mot), chemin });
});

// Toutes les solutions, révélées en fin de partie
app.MapGet("/api/parties/{id}/solutions", (string id, Parties parties) =>
    parties.Trouver(id) is Partie p
        ? Results.Ok(p.Solutions.Select(s => new { mot = s.Mot, points = s.Points, chemin = s.Chemin }))
        : Erreur("Partie introuvable.", 404));

app.Run();

record NouvellePartie(string? Langue, int Taille, int? Graine);
record MotPropose(string? Mot);
