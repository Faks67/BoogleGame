using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoogleGame
{
    public class Dictionnaire
    {
        // Attributs
        public Dictionary<int, int> nombreDeMotsParLongueur = new Dictionary<int, int>();
        public Dictionary<char, int> nombreDeMotsParLettre = new Dictionary<char, int>();
        public string langue; // FR pour français, EN pour anglais, par exemple
        public List<string> mots = new List<string>(); // La liste des mots du dictionnaire
        private HashSet<string> motsJoues = new HashSet<string>();

        public Dictionnaire(string cheminFichierDictionnaire, string langue)
        {
            this.langue = langue;
            this.langue = langue;
            mots = new List<string>();
            nombreDeMotsParLettre = new Dictionary<char, int>();
            nombreDeMotsParLongueur = new Dictionary<int, int>();


            ChargerMots(cheminFichierDictionnaire);
            CalculerStatistiques();
        }

        public bool VerifierMotNonJoue(string mot)
        {
            if (motsJoues.Contains(mot))
            {
                Console.WriteLine($"Le mot '{mot}' a déjà été joué !");
                return false;
            }

            motsJoues.Add(mot); // Ajouter le mot à la liste des mots joués
            return true;
        }


        public bool ValiderMot(string mot)
        {
            if (!RechDichoRecursif(mot))
            {
                Console.WriteLine($"Le mot '{mot}' n'existe pas dans le dictionnaire.");
                return false;
            }

            return VerifierMotNonJoue(mot);
        }

        public string Langue
        { get { return langue; } }
        public void ChargerMots(string cheminFichierDictionnaire)
        {
            foreach (var ligne in File.ReadLines(cheminFichierDictionnaire))
            {
                string[] motsLigne = ligne.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var mot in motsLigne)
                {
                    mots.Add(mot.ToUpper()); // S'assurer que tout est en majuscule
                }
            }
            mots.Sort(); // Trier les mots pour la recherche dichotomique
        }



        public void AfficherMots()
        {

            Console.WriteLine(mots.Count);
        }


        // Méthode pour rechercher un mot de manière récursive
        public bool RechDichoRecursif(string motSaisi, int debut = 0, int fin = -1)
        {
            if (fin == -1) fin = mots.Count - 1; // Initialiser fin si ce n'est pas défini

            if (debut >= fin)
                return false; // Cas de base : le mot n'est pas trouvé

            int milieu = (debut + fin) / 2;
            int comparaison = string.Compare(mots[milieu], motSaisi);
            //Console.WriteLine(milieu);

            if (comparaison == 0)
                return true; // Le mot est trouvé
            else if (comparaison > 0)
                return RechDichoRecursif(motSaisi, debut, milieu); // Rechercher dans la première moitié
            else
                return RechDichoRecursif(motSaisi, milieu + 1, fin); // Rechercher dans la deuxième moitié


        }

        public void CalculerNombreDeMotsParLettre()
        {
            foreach (string mot in mots)
            {


                char premiereLettre = mot[0];
                // Suite du traitement comme précédemment...

                if (nombreDeMotsParLettre.ContainsKey(premiereLettre))
                {
                    nombreDeMotsParLettre[premiereLettre]++;
                }
                else
                {
                    nombreDeMotsParLettre[premiereLettre] = 1;
                }


            }
        }

        public void CalculerNombreDeMotsParLongueur()
        {
            foreach (var mot in mots)
            {
                int longueur = mot.Length; // Utilise la longueur correcte en entier

                if (nombreDeMotsParLongueur.ContainsKey(longueur))
                {
                    nombreDeMotsParLongueur[longueur]++;
                }
                else
                {
                    nombreDeMotsParLongueur[longueur] = 1;
                }


            }
        }

        public void VerifierIntegriteDictionnaire()
        {
            // Calculer le nombre total de mots en sommant les valeurs du dictionnaire
            int sommeDesMots = 0;
            foreach (var valeur in nombreDeMotsParLettre.Values)
            {
                sommeDesMots += valeur;
            }

            // Vérifier si la somme des mots par lettre est égale au nombre total de mots
            bool sommeCorrecte = sommeDesMots == mots.Count;
            Console.WriteLine("Somme des mots par lettre : " + sommeDesMots);
            Console.WriteLine("Nombre total de mots dans le dictionnaire : " + mots.Count);
            Console.WriteLine("La somme est correcte : " + sommeCorrecte);

            // Vérifier que toutes les lettres de A à Z sont présentes comme clés
            bool toutesLettresPresentes = true;
            for (char lettre = 'A'; lettre <= 'Z'; lettre++)
            {
                if (!nombreDeMotsParLettre.ContainsKey(lettre))
                {
                    toutesLettresPresentes = false;
                    Console.WriteLine("Lettre manquante : " + lettre);
                }
            }

            Console.WriteLine("Toutes les lettres de A à Z sont présentes : " + toutesLettresPresentes);
        }




        public void CalculerStatistiques()
        {
            CalculerNombreDeMotsParLettre();
            CalculerNombreDeMotsParLongueur();
        }


        /* public string toString()
         {
             string infoDictionnaire = $"La langue du dictionnaire est : {langue}";
             string nombre_de_mot_par_lettre = $"\nLe nombre de mot par lettre est : {nombreDeMotsParLettre.Count.ToString()}";
             string nombre_de_mot_par_longueur = $"\nLe nombre de mot par longueur est : {nombreDeMotsParLongueur.Count.ToString()}";
             return $"{infoDictionnaire} {nombreDeMotsParLettre} {nombreDeMotsParLongueur} ";
         }
        */
        public string toString()
        {
            string description = $"La langue du dictionnaire est : {langue}\n";

            description += "\nNombre de mots par lettre :\n";
            foreach (var entry in nombreDeMotsParLettre)
            {
                description += $"Lettre '{entry.Key}': {entry.Value} mots\n";
            }

            description += "\nNombre de mots par longueur :\n";
            foreach (var entry in nombreDeMotsParLongueur)
            {
                description += $"Longueur {entry.Key} : {entry.Value} mots\n";
            }

            return description;
        }

    }
}