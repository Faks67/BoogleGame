namespace BoogleGame.Web.Moteur;

public record Solution(string Mot, int Points, int[] Chemin);

/// <summary>
/// Plateau de Boggle : lancer des dés (reproductible grâce à la graine), recherche de mots et solveur
/// </summary>
public class Plateau
{
    // Dés du Boggle anglais (4x4) et du Big Boggle (5x5)
    private static readonly string[] DesAnglais4 =
    {
        "AAEEGN", "ABBJOO", "ACHOPS", "AFFKPS", "AOOTTW", "CIMOTU", "DEILRX", "DELRVY",
        "DISTTY", "EEGHNW", "EEINSU", "EHRTVW", "EIOSST", "ELRTTY", "HIMNQU", "HLNNRZ"
    };
    private static readonly string[] DesAnglais5 =
    {
        "AAAFRS", "AAEEEE", "AAFIRS", "ADENNN", "AEEEEM", "AEEGMU", "AEGMNN", "AFIRSY", "BJKQXZ",
        "CCENST", "CEIILT", "CEILPT", "CEIPST", "DDHNOT", "DHHLOR", "DHLNOR", "DHLNOR", "EIIITT",
        "EMOTTT", "ENSSSU", "FIPRSY", "GORRVW", "IPRRRY", "NOOTUW", "OOOTTU"
    };
    // Dés du Boggle français
    private static readonly string[] DesFrancais =
    {
        "ETUKNO", "EVGTIN", "DECAMP", "IELRUW", "EHIFSE", "RECALS", "ENTDOS", "OFXRIA",
        "NAVEDZ", "EIOATA", "GLENYU", "BMAQJO", "TLIBRA", "SPULTE", "AIMSOR", "ENHRIS"
    };

    public int Taille { get; }
    /// <summary>Faces visibles, ligne par ligne ("QU" pour le Q)</summary>
    public string[] Cases { get; }

    public Plateau(int taille, string langue, int graine)
    {
        Taille = taille;
        var aleatoire = new Random(graine);
        string[] jeu = langue == "EN" ? (taille >= 5 ? DesAnglais5 : DesAnglais4) : DesFrancais;

        // Autant de dés que de cases : on réutilise le jeu de dés si le plateau est plus grand
        var des = new List<string>();
        while (des.Count < taille * taille)
            des.AddRange(jeu.OrderBy(_ => aleatoire.Next()));
        des = des.Take(taille * taille).OrderBy(_ => aleatoire.Next()).ToList();

        Cases = des.Select(de =>
        {
            char face = de[aleatoire.Next(de.Length)];
            return face == 'Q' ? "QU" : face.ToString();
        }).ToArray();
    }

    public static int Points(string mot) => mot.Length switch
    {
        < 3 => 0,
        3 or 4 => 1,
        5 => 2,
        6 => 3,
        7 => 5,
        _ => 11
    };

    private IEnumerable<int> Voisins(int i)
    {
        int l = i / Taille, c = i % Taille;
        for (int dl = -1; dl <= 1; dl++)
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dl == 0 && dc == 0) continue;
                int nl = l + dl, nc = c + dc;
                if (nl >= 0 && nl < Taille && nc >= 0 && nc < Taille) yield return nl * Taille + nc;
            }
    }

    /// <summary>
    /// Cherche un chemin de cases adjacentes (sans réutiliser une case) qui écrit le mot
    /// </summary>
    public int[]? Chercher(string mot)
    {
        var chemin = new List<int>();
        var utilisees = new bool[Cases.Length];

        bool Explorer(int i, int position)
        {
            string tuile = Cases[i];
            if (utilisees[i] || string.CompareOrdinal(mot, position, tuile, 0, tuile.Length) != 0) return false;
            utilisees[i] = true;
            chemin.Add(i);
            int suite = position + tuile.Length;
            if (suite == mot.Length || Voisins(i).Any(v => Explorer(v, suite))) return true;
            utilisees[i] = false;
            chemin.RemoveAt(chemin.Count - 1);
            return false;
        }

        for (int i = 0; i < Cases.Length; i++)
            if (Explorer(i, 0)) return chemin.ToArray();
        return null;
    }

    /// <summary>
    /// Tous les mots du dictionnaire qu'on peut former sur le plateau
    /// </summary>
    public List<Solution> Resoudre(Lexique lexique)
    {
        var trouves = new Dictionary<string, int[]>();
        var utilisees = new bool[Cases.Length];
        var chemin = new List<int>();

        void Explorer(int i, string prefixe)
        {
            string mot = prefixe + Cases[i];
            if (!lexique.EstPrefixe(mot)) return;
            utilisees[i] = true;
            chemin.Add(i);
            if (mot.Length >= 3 && lexique.Contient(mot) && !trouves.ContainsKey(mot)) trouves[mot] = chemin.ToArray();
            foreach (int v in Voisins(i))
                if (!utilisees[v]) Explorer(v, mot);
            utilisees[i] = false;
            chemin.RemoveAt(chemin.Count - 1);
        }

        for (int i = 0; i < Cases.Length; i++) Explorer(i, "");
        return trouves
            .Select(kv => new Solution(kv.Key, Points(kv.Key), kv.Value))
            .OrderByDescending(s => s.Mot.Length).ThenBy(s => s.Mot, StringComparer.Ordinal)
            .ToList();
    }
}
