using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoogleGame
{
    public class Plateau
    {
        public De[] des;
        public Dictionnaire dictionnaire;
        private int taille;
       




        public Plateau(int taille, Dictionnaire dictionnaire)
        {
            this.taille = taille;
            this.des = new De[taille * taille]; // Initialiser un tableau de dés pour une grille taille x taille
            this.dictionnaire = dictionnaire;

            // Créer et remplir les dés aléatoirement pour chaque case du plateau
            for (int i = 0; i < des.Length; i++)
            {
                des[i] = new De(Path.Combine(AppContext.BaseDirectory, "Lettres.txt")); // Fichier de lettres
            }

            // Mélanger les dés pour une disposition aléatoire
            MelangerDes();

        }



        /*
        public Plateau(De[] desExistants)
        {
            if (desExistants.Length != 16)
            {
                throw new ArgumentException("Le tableau des dés doit contenir exactement 16 dés.");
            }

            // Copier les dés existants pour remplir le plateau
            des = new De[16];
            Array.Copy(desExistants, des, 16);

            // Mélanger les dés pour une disposition aléatoire
            MelangerDes();
        }*/

        /// <summary>
        /// Mélanger les dés sur le plateau pour une disposition aléatoire.
        /// </summary>
        private void MelangerDes()
        {
            Random rand = new Random();
            for (int i = 0; i < des.Length; i++)
            {
                int randomIndex = rand.Next(des.Length);
                // Échanger les dés
                De temp = des[i];
                des[i] = des[randomIndex];
                des[randomIndex] = temp;
            }
        }

        /// <summary>
        /// Affiche les informations détaillées de chaque dé sur le plateau.
        /// </summary>
        public void AfficherTousLesDes()
        {
            Console.WriteLine("Affichage détaillé des dés sur le plateau :");
            for (int i = 0; i < des.Length; i++)
            {
                Console.WriteLine($"Dé {i + 1} :");
                Console.WriteLine(des[i].toString());
                Console.WriteLine(); // Saut de ligne pour séparer les dés
            }
        }


        public static Dictionary<char, (int Points, int Frequence)> ChargerValeursLettres(string cheminFichierLettres)
        {
            var valeursLettres = new Dictionary<char, (int Points, int Frequence)>();

            foreach (var ligne in File.ReadAllLines(cheminFichierLettres))
            {
                var elements = ligne.Split(';');
                char lettre = elements[0][0];
                int points = int.Parse(elements[1]);      // Points associés à la lettre
                int frequence = int.Parse(elements[2]);  // Nombre d'apparitions
                valeursLettres[lettre] = (points, frequence);
            }

            return valeursLettres;
        }




        /*
        public string toString()
        {
            string description = "Voici le plateau :\n";

            for (int i = 0; i < 4; i++) // Ligne
            {
                for (int j = 0; j < 4; j++) // Colonne
                {
                    int index = i * 4 + j;
                    description += des[index].LettreVisible + " ";
                }
                description += "\n"; // Saut de ligne après chaque rangée de 4 dés
            }

            return description;
        }

        */
        /// <summary>
        /// Représentation textuelle du plateau sous forme d'une grille.
        /// </summary>
        /// <returns>Une chaîne de caractères représentant le plateau.</returns>
        public string toString()
        {
            if (des == null)
                return "Erreur : le tableau de dés n'a pas été initialisé.";

            StringBuilder description = new StringBuilder("Voici le plateau :\n");

            // Parcourt ligne par ligne
            for (int i = 0; i < taille; i++) // Ligne
            {
                for (int j = 0; j < taille; j++) // Colonne
                {
                    int index = i * taille + j; // Calculer l'index du tableau à partir des coordonnées
                    description.Append(des[index].LettreVisible + " ");
                }
                description.AppendLine(); // Saut de ligne après chaque rangée
            }

            return description.ToString();
        }





        public De[] Des
        {
            get { return des; }
        }


        /// <summary>
        /// Lancer tous les dés pour redéfinir leur lettre visible.
        /// </summary>
        public void LancerTousLesDes()
        {
            Random random = new Random();
            foreach (var de in des)
            {
                de.Lance(random); // Définir une nouvelle lettre visible pour chaque dé
            }
        }

        /// <summary>
        /// Teste si un mot donné peut être formé sur le plateau selon les règles du jeu.
        /// </summary>
        /// <param name="motSaisi">Le mot à rechercher sur le plateau.</param>
        /// <returns>True si le mot est trouvé, sinon False.</returns>
        public bool Test_Plateau(string motSaisi, Joueur joueur)
        {
            // Vérifier si le mot a déjà été joué par ce joueur
            if (!joueur.VerifierMotNonJoue(motSaisi))
            {
                return false; // Mot déjà joué
            }

            // Vérifie si le mot existe dans le dictionnaire
            if (!dictionnaire.RechDichoRecursif(motSaisi))
            {
                Console.WriteLine($"Le mot '{motSaisi}' n'est pas dans le dictionnaire.");
                return false;
            }

            // Matrice de visites pour suivre les cases explorées
            bool[,] visite = new bool[taille, taille];

            // Parcourt chaque cellule pour trouver un point de départ potentiel
            for (int i = 0; i < taille; i++)
            {
                for (int j = 0; j < taille; j++)
                {
                    if (RechercheAdjacente(motSaisi, 0, i, j, visite))
                    {
                        return true; // Mot trouvé
                    }
                }
            }

            return false; // Mot non trouvé
        }


        /// <summary>
        /// Recherche récursive pour valider les mots adjacents.
        /// </summary>
        private bool RechercheAdjacente(string motSaisi, int index, int x, int y, bool[,] visite)
        {
            if (index == motSaisi.Length) // Si on atteint la fin du mot
                return true;

            if (x < 0 || x >= taille || y < 0 || y >= taille || visite[x, y] || des[x * taille + y].LettreVisible != motSaisi[index])
                return false;

            visite[x, y] = true; // Marquer la case actuelle comme visitée

            int[] directionsX = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] directionsY = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for (int d = 0; d < directionsX.Length; d++)
            {
                if (RechercheAdjacente(motSaisi, index + 1, x + directionsX[d], y + directionsY[d], visite))
                    return true;
            }

            visite[x, y] = false; // Backtracking
            return false;
        }



        private void ValiderOccurrencesGlobales(Dictionary<char, (int Points, int Frequence)> valeursLettres)
        {
            var occurrencesGlobales = new Dictionary<char, int>();

            // Compter les occurrences de chaque lettre sur le plateau
            foreach (var de in des)
            {
                foreach (var lettre in de.LettreFaces)
                {
                    if (!occurrencesGlobales.ContainsKey(lettre))
                        occurrencesGlobales[lettre] = 0;

                    occurrencesGlobales[lettre]++;
                }
            }

            // Vérifier les occurrences par rapport aux fréquences autorisées
            foreach (var lettre in occurrencesGlobales.Keys)
            {
                if (occurrencesGlobales[lettre] > valeursLettres[lettre].Frequence)
                {
                    throw new InvalidOperationException($"La lettre '{lettre}' dépasse son occurrence maximale autorisée ({valeursLettres[lettre].Frequence}).");
                }
            }
        }





        /* public string toString()
         {
             string description = "Voici le plateau :\n";

             for (int i = 0; i < 4; i++) // Ligne
             {
                 for (int j = 0; j < 4; j++) // Colonne
                 {
                     int index = i * 4 + j;
                     description += des[index].LettreVisible + " ";
                 }
                 description += "\n"; // Saut de ligne après chaque rangée de 4 dés
             }

             return description;
         }


         */






        /*
                public string toString()
                {
                    string infoPlateau = " ";
                    foreach (var entry in des)
                    {
                        infoPlateau += entry.ToString();
                    }
                    return infoPlateau; 
                }

                // Méthode ToString pour l'affichage en grille 4x4
                public override string ToString()
                {
                    string affichage = "";
                    for (int i = 0; i < des.Length; i++)
                    {
                        affichage += des[i].LettreVisible + " ";
                        if ((i + 1) % 4 == 0) affichage += "\n"; // Passer à la ligne après chaque 4 dés
                    }
                    return affichage;
                }
                /*
                // Deuxième méthode pour un affichage simplifié en ligne
                public string ToStringSimplifie()
                {
                    string affichage = "Plateau : ";
                    foreach (var de in des)
                    {
                        affichage += de.LettreVisible + " ";
                    }
                    return affichage.Trim(); // Supprime l’espace final
                }

                //Troisième méthode pour un affichage Détailé de chaque dé et sa face visible
                public string ToStringDetaille()
                {
                    string affichage = "Plateau détaillé :\n";
                    for (int i = 0; i < des.Length; i++)
                    {
                        // Affiche l’index du dé, son contenu (lettre visible)
                        affichage += $"Dé {i + 1} (face visible : {des[i].LettreVisible}) ";

                        // Ajoute une nouvelle ligne tous les 4 dés pour former la structure 4x4
                        if ((i + 1) % 4 == 0) affichage += "\n";
                    }
                    return affichage;
                }

                */

    }
}
