namespace BoogleGame.Web.Moteur;

/// <summary>
/// Dictionnaire d'une langue : recherche d'un mot et d'un préfixe par dichotomie dans une liste triée
/// </summary>
public class Lexique
{
    private readonly string[] mots;
    private readonly HashSet<string> ensemble;

    public string Langue { get; }
    public int Taille => mots.Length;

    public Lexique(string langue, IEnumerable<string> chemins)
    {
        Langue = langue;
        var tous = new HashSet<string>(StringComparer.Ordinal);
        foreach (string chemin in chemins)
            foreach (string ligne in File.ReadLines(chemin))
                foreach (string brut in ligne.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string mot = brut.Trim().ToUpperInvariant();
                    // Mots de 3 lettres ou plus, lettres A à Z uniquement (pas d'apostrophe ni de tiret)
                    if (mot.Length >= 3 && mot.All(c => c is >= 'A' and <= 'Z')) tous.Add(mot);
                }
        mots = tous.ToArray();
        Array.Sort(mots, StringComparer.Ordinal);
        ensemble = tous;
    }

    public bool Contient(string mot) => ensemble.Contains(mot);

    /// <summary>
    /// Vrai si au moins un mot commence par ce préfixe (élague la recherche du solveur)
    /// </summary>
    public bool EstPrefixe(string prefixe)
    {
        int bas = 0, haut = mots.Length;
        while (bas < haut)
        {
            int milieu = (bas + haut) / 2;
            if (string.CompareOrdinal(mots[milieu], prefixe) < 0) bas = milieu + 1;
            else haut = milieu;
        }
        return bas < mots.Length && mots[bas].StartsWith(prefixe, StringComparison.Ordinal);
    }
}
