using System.Globalization;
using Microsoft.Extensions.Caching.Memory;

namespace BoogleGame.Web.Moteur;

public record Partie(string Id, string Langue, int Taille, int Graine, Plateau Plateau, List<Solution> Solutions)
{
    public int PointsMax => Solutions.Sum(s => s.Points);
}

/// <summary>
/// Crée les parties et garde leurs solutions en mémoire. Une partie est entièrement définie par
/// sa langue, sa taille et sa graine : l'identifiant "FR-4-123456" suffit à la recréer.
/// </summary>
public class Parties(IReadOnlyDictionary<string, Lexique> lexiques, IMemoryCache cache)
{
    public static readonly int[] Tailles = { 4, 5, 6 };

    // Nombre minimal de mots pour qu'une grille soit intéressante
    private static int MotsMinimum(int taille) => taille switch { 4 => 40, 5 => 90, _ => 150 };

    public bool LangueConnue(string langue) => lexiques.ContainsKey(langue);

    public Partie Nouvelle(string langue, int taille, int graine)
    {
        // On relance les dés (graine suivante) tant que la grille est trop pauvre
        for (int essai = 0; ; essai++)
        {
            var partie = Charger(langue, taille, graine + essai);
            if (partie.Solutions.Count >= MotsMinimum(taille) || essai >= 40) return partie;
        }
    }

    /// <summary>
    /// Défi du jour : même grille pour tout le monde, qui change chaque jour
    /// </summary>
    public Partie DuJour(string langue, DateOnly jour) =>
        Nouvelle(langue, 4, int.Parse(jour.ToString("yyyyMMdd", CultureInfo.InvariantCulture)) * 100);

    public Partie? Trouver(string id)
    {
        var morceaux = id.Split('-');
        if (morceaux.Length != 3 || !LangueConnue(morceaux[0]) ||
            !int.TryParse(morceaux[1], out int taille) || !Tailles.Contains(taille) ||
            !int.TryParse(morceaux[2], out int graine)) return null;
        return Charger(morceaux[0], taille, graine);
    }

    private Partie Charger(string langue, int taille, int graine)
    {
        string id = $"{langue}-{taille}-{graine}";
        return cache.GetOrCreate(id, entree =>
        {
            entree.SlidingExpiration = TimeSpan.FromHours(2);
            entree.Size = 1;
            var plateau = new Plateau(taille, langue, graine);
            return new Partie(id, langue, taille, graine, plateau, plateau.Resoudre(lexiques[langue]));
        })!;
    }

    public Lexique Lexique(string langue) => lexiques[langue];
}
