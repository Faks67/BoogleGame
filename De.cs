using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BoogleGame
{
    public class De
    {
        public char lettreVisible;// La lettre visible actuellement

        public char[] lettreFaces;// Les lettres sur les faces du dé

        private List<char> lettresDisponibles; // Liste de lettres disponibles

        public List<char> lettre = new List<char>();
        // Constructeur qui initialise le dé avec des lettres aléatoires
        // Constructeur qui initialise le dé avec des lettres aléatoires
        public De(string cheminFichierLettres)
        {
            lettreFaces = new char[6]; // 6 faces par dé
            lettresDisponibles = new List<char>();
            cheminFichierLettres = Path.Combine(AppContext.BaseDirectory, "Lettres.txt");

            // Charger les lettres du fichier
            ChargerLettres(cheminFichierLettres);

            // Assigner des lettres aléatoires aux faces
            AssignerLettresAuxFaces();

            // Lancer le dé pour initialiser une lettre visible
            Lance(new Random());
        }

        // Méthode pour charger les lettres du fichier
        /* private void ChargerLettres(string cheminFichierLettres)
         {
             if (!File.Exists(cheminFichierLettres))
             {
                 throw new FileNotFoundException("Le fichier de lettres n'existe pas : " + cheminFichierLettres);
             }

             // Lire chaque ligne du fichier et ajouter chaque lettre à la liste
             foreach (var ligne in File.ReadLines(cheminFichierLettres))
             {
                 foreach (var caractere in ligne.Split(new[] { ';', ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
                 {
                     if (char.TryParse(caractere, out char lettre))
                     {
                         lettresDisponibles.Add(lettre);
                     }
                 }
             }

             if (lettresDisponibles.Count == 0)
             {
                 throw new InvalidOperationException("Aucune lettre disponible après le chargement du fichier.");
             }
         }
        */

        private Dictionary<char, (int Points, int Frequence)> valeursLettres;

        private void ChargerLettres(string cheminFichierLettres)
        {
            valeursLettres = new Dictionary<char, (int Points, int Frequence)>();

            if (!File.Exists(cheminFichierLettres))
            {
                throw new FileNotFoundException("Le fichier de lettres n'existe pas : " + cheminFichierLettres);
            }

            // Lire chaque ligne et extraire les informations sur les lettres
            foreach (var ligne in File.ReadLines(cheminFichierLettres))
            {
                var elements = ligne.Split(';');
                if (elements.Length == 3)
                {
                    char lettre = elements[0][0];
                    int points = int.Parse(elements[1]);
                    int frequence = int.Parse(elements[2]);

                    valeursLettres[lettre] = (points, frequence);
                }
            }

            if (valeursLettres.Count == 0)
            {
                throw new InvalidOperationException("Aucune lettre valide trouvée dans le fichier.");
            }
        }


       /*private void ChargerLettres(string cheminFichierLettres)
        {
            if (!File.Exists(cheminFichierLettres))
            {
                throw new FileNotFoundException("Le fichier de lettres n'existe pas : " + cheminFichierLettres);
            }

            // Lire chaque ligne du fichier et ajouter chaque lettre non-numérique à la liste
            foreach (var ligne in File.ReadLines(cheminFichierLettres))
            {
                foreach (var caractere in ligne.Split(new[] { ';', ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (char.TryParse(caractere, out char lettre) && !char.IsDigit(lettre))
                    {
                        lettresDisponibles.Add(lettre);
                    }
                }
            }

            if (lettresDisponibles.Count == 0)
            {
                throw new InvalidOperationException("Aucune lettre valide disponible après le chargement du fichier.");
            }
        }
        */

        // Méthode pour assigner des lettres aléatoires aux faces du dé
       /* private void AssignerLettresAuxFaces()
        {
            Random rand = new Random();
            for (int i = 0; i < lettreFaces.Length; i++)
            {
                int indexRandom = rand.Next(lettresDisponibles.Count);
                lettreFaces[i] = lettresDisponibles[indexRandom];
            }
        }*/


        private void AssignerLettresAuxFaces()
        {
            Random rand = new Random();

            // Construire une liste de lettres pondérées en fonction de leur fréquence
            List<char> lettresPondérées = new List<char>();
            foreach (var entry in valeursLettres)
            {
                lettresPondérées.AddRange(Enumerable.Repeat(entry.Key, entry.Value.Frequence));
            }

            // Mélanger les lettres pour une distribution aléatoire
            lettresPondérées = lettresPondérées.OrderBy(_ => rand.Next()).ToList();

            // Assigner des lettres pondérées aux faces du dé
            for (int i = 0; i < lettreFaces.Length; i++)
            {
                if (lettresPondérées.Count > 0)
                {
                    lettreFaces[i] = lettresPondérées[rand.Next(lettresPondérées.Count)];
                }
            }
        }

       




        // Propriétés pour accéder aux lettres
        public char[] LettreFaces
        {
            get { return lettreFaces; }
        }

        public char LettreVisible
        {
            get { return lettreVisible; }
        }

        // Méthode pour lancer le dé et choisir une lettre visible
        /*public void Lance(Random rand)
        {
            int indexRandom = rand.Next(0, lettreFaces.Length);
            lettreVisible = lettreFaces[indexRandom];
        }*/

        // Méthode pour lancer le dé et choisir une lettre visible
        public void Lance(Random rand)
        {
            if (lettreFaces == null || lettreFaces.Length == 0)
            {
                throw new InvalidOperationException("Les lettres sur les faces du dé n'ont pas été initialisées.");
            }

            int indexRandom = rand.Next(0, lettreFaces.Length);
            lettreVisible = lettreFaces[indexRandom];
        }


        public int ObtenirPoints(char lettre)
        {
            if (valeursLettres.ContainsKey(lettre))
            {
                return valeursLettres[lettre].Points;
            }
            return 0; // Par défaut, une lettre inconnue vaut 0 points
        }



        // Méthode toString pour afficher les informations du dé
        public string toString()
        {
            if (lettreFaces == null || lettreFaces.Length == 0)
            {
                return "Le dé n'a pas été initialisé correctement.";
            }

            StringBuilder description = new StringBuilder("Lettres sur le dé : ");
            foreach (char c in lettreFaces)
            {
                description.Append(c + " ");
            }

            description.AppendLine();
            description.AppendLine("Lettre visible : " + lettreVisible);

            return description.ToString();
        }
    }

}


