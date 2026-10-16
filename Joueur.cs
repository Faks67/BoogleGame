using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BoogleGame
{
    public class Joueur
    {
        // Attribut
        public string nom;
        public int score;
        // Création de liste de mot trouver
        public List<string> motTrouver = new List<string>();
        private HashSet<string> motsJoues; // Stocker les mots joués par le joueur



        //Constructeur Naturell
        //non ajout de la liste de mot en paramètre car le joueur commence avec une liste de mot vide 
        public Joueur(string nom, int score)
        {
            if (string.IsNullOrWhiteSpace(nom))
            {
                throw new ArgumentException("Le nom du joueur ne peut pas être vide.");
            }
            this.nom = nom;
            this.score = score;
            this.motTrouver = new List<string>();
            motsJoues = new HashSet<string>(); // Initialiser la collection ici
        }

         
        //Propriétés
        public int Score
        { get { return score; } }

        public List<string> MotTrouver
        { get { return motTrouver; } }

        public string Nom
        { get { return nom; } }


        


        //Méthode Contain pour le test d'apartenances

        public bool Contain(string mot)
        {
            
            return motTrouver.Contains(mot);
            
        }

        //Méthode Add_Mot pour ajouter un mot dans la liste des mots déjà trouvés 

        public void Add_Mot(string mot)
        {
            if(!Contain(mot))
            {
               
                motTrouver.Add(mot);
            }

        }

        public void AjouterPoints(int points)
        {
            if (points > 0)
            {
                this.score += points;
                Console.WriteLine($"{points} points ajoutés ! Score actuel : {Score} points.");
            }
            else
            {
                Console.WriteLine("Aucun point n'a été ajouté.");
            }
        }

        public int CalculerPointsMot(string mot, De[] des)
        {
            int totalPoints = 0;

            foreach (char lettre in mot)
            {
                // Trouver les points de chaque lettre à l'aide des dés
                foreach (De de in des)
                {
                    if (de.LettreVisible == lettre)
                    {
                        totalPoints += de.ObtenirPoints(lettre);
                        break; // On passe à la lettre suivante une fois le point ajouté
                    }
                }
            }

            return totalPoints;
        }


        /// <summary>
        /// Vérifie si un mot n'a pas encore été joué par ce joueur.
        /// Si le mot est nouveau, il est ajouté à la liste des mots joués.
        /// </summary>
        /// <param name="mot">Le mot à vérifier</param>
        /// <returns>True si le mot n'a pas été joué, False sinon</returns>
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

        //Méthode toString : Retourne une chaîne de caractère décrivant un joueur.
        public string toString()
        {
            string mot_trouver = "";
            foreach (string element in motTrouver)
            {
                 mot_trouver += element + " ";
            }
            return $"Nom du joueur : {nom} \nScore du Joueur : {score} \nMot trouver : {mot_trouver}";
        }
    }

}
