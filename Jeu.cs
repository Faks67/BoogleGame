using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoogleGame
{
    class Jeu
    {
        static void Main(string[] args)
        {
            /*
            Console.WriteLine("Bienvenue dans le jeu de Boggle !");
            Console.WriteLine("Initialisation des composants...\n");

            try
            {
                // Étape 1 : Initialiser les dictionnaires
                Console.WriteLine("Chargement des dictionnaires...");
               Dictionnaire dictionnaire = new Dictionnaire(Path.Combine(AppContext.BaseDirectory, "MotsPossiblesFR.txt"),"FR");
                dictionnaire.CalculerNombreDeMotsParLettre();
                dictionnaire.CalculerNombreDeMotsParLongueur();

                // Affichage des informations du dictionnaire français
                Console.WriteLine("\nAffichage du dictionnaire français :");
                Console.WriteLine(dictionnaire.toString());
                dictionnaire.AfficherMots();

                // Étape 2 : Initialiser un joueur
                Console.Write("\nEntrez le nom du joueur : ");
                string nomDuJoueur = Console.ReadLine();
                Joueur joueur = new Joueur(nomDuJoueur, 0); // Score initial à 0
                Console.WriteLine(joueur.toString());

                // Étape 3 : Initialiser le plateau avec 16 dés
                Console.WriteLine("\nCréation et initialisation des dés pour le plateau...");
                De[] des = new De[16];
                for (int i = 0; i < des.Length; i++)
                {
                    des[i] = new De(Path.Combine(AppContext.BaseDirectory, "LettreTest.txt"));
                }

                // Initialisation du plateau
                Plateau plateau = new Plateau(des, dictionnaire);

                // Afficher le plateau
                Console.WriteLine("\nAffichage du plateau :");
                Console.WriteLine(plateau.toString());

                // Étape 4 : Jouer au jeu
                Console.WriteLine("\nLe jeu commence maintenant !");
                string motSaisi;
                do
                {
                    Console.Write("Entrez un mot (ou tapez 'quit' pour quitter) : ");
                    motSaisi = Console.ReadLine()?.ToUpper(); // Convertir en majuscules pour éviter les erreurs

                    if (motSaisi == "QUIT")
                        break;

                    // Vérifier si le mot est valide sur le plateau
                    bool motValide = plateau.Test_Plateau(motSaisi);
                    if (motValide)
                    {
                        Console.WriteLine($"Bravo ! Le mot '{motSaisi}' est valide.");
                        joueur.AjouterPoints(motSaisi.Length); // Ajouter des points en fonction de la longueur du mot
                    }
                    else
                    {
                        Console.WriteLine($"Désolé, le mot '{motSaisi}' est invalide.");
                    }

                    // Afficher le score actuel
                    Console.WriteLine($"Score actuel de {joueur.Nom} : {joueur.Score}");
                }
                while (true);

                Console.WriteLine("\nFin de la partie !");
                Console.WriteLine($"Score final de {joueur.Nom} : {joueur.Score}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'initialisation : " + ex.Message);
            }

            Console.WriteLine("\nAppuyez sur Entrée pour quitter...");
            Console.ReadLine();
            */


            Console.WriteLine("Bienvenue dans le jeu de Boggle !");
            Console.WriteLine("Préparation du jeu...\n");

            // Étape 1 : Configuration de la taille du plateau
            Console.Write("Souhaitez-vous personnaliser la taille du plateau ? (y/n) : ");
            string choixTaille = Console.ReadLine()?.ToLower();

            int taillePlateau = 4; // Taille par défaut
            if (choixTaille == "y")
            {
                bool tailleValide = false;
                while (!tailleValide)
                {
                    Console.Write("Entrez la taille du plateau (minimum 2, maximum 16) : ");
                    if (int.TryParse(Console.ReadLine(), out taillePlateau) && taillePlateau >= 2 && taillePlateau <= 16)
                    {
                        tailleValide = true;
                    }
                    else
                    {
                        Console.WriteLine("Entrée invalide. Veuillez saisir un nombre entre 2 et 16.");
                    }
                }
            }
            Console.WriteLine($"Taille sélectionnée : {taillePlateau}x{taillePlateau}");





            // Étape 1 : Initialisation des joueurs
            Console.Write("Entrez le nom du joueur 1 : ");
            string nomJoueur1 = Console.ReadLine();
            Joueur joueur1 = new Joueur(nomJoueur1, 0);

            Console.Write("Entrez le nom du joueur 2 : ");
            string nomJoueur2 = Console.ReadLine();
            Joueur joueur2 = new Joueur(nomJoueur2, 0);

            // Étape 2 : Initialisation des dictionnaires
            


            Console.WriteLine("Choisissez une option :");
            Console.WriteLine("1. Dictionnaire Français (FR)");
            Console.WriteLine("2. Dictionnaire Anglais (EN)");
            Console.WriteLine("3. Dictionnaire Mixte (FR + EN)");

            int choix = int.Parse(Console.ReadLine() ?? "1");

            string cheminDictionnaire = "";
            switch (choix)
            {
                case 1:
                    cheminDictionnaire = Path.Combine(AppContext.BaseDirectory, "MotsPossiblesFR.txt");
                    break;
                case 2:
                    cheminDictionnaire = Path.Combine(AppContext.BaseDirectory, "MotsPossiblesEN.txt");
                    break;
                case 3:
                    cheminDictionnaire = Path.Combine(AppContext.BaseDirectory, "MotsMixte.txt");
                    // Fusion des deux dictionnaires (si fichier inexistant)
                    if (!File.Exists(cheminDictionnaire))
                    {
                        File.WriteAllLines(cheminDictionnaire,
                            File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "MotsPossiblesFR.txt"))
                            .Concat(File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "MotsPossiblesEN.txt"))));
                    }
                    break;
                default:
                    Console.WriteLine("Option invalide, dictionnaire FR par défaut.");
                    cheminDictionnaire = Path.Combine(AppContext.BaseDirectory, "MotsPossiblesFR.txt");
                    break;
            }

            Dictionnaire dictionnaire = new Dictionnaire(cheminDictionnaire, choix == 2 ? "EN" : "FR");


            // Étape 4 : Jeu principal avec gestion du temps
            TimeSpan tempsParJoueur = TimeSpan.FromMinutes(1); // Temps limité pour chaque joueur
            JouerUnTour(joueur1, dictionnaire, taillePlateau, tempsParJoueur);
            JouerUnTour(joueur2, dictionnaire, taillePlateau, tempsParJoueur);

            // Étape 4 : Annonce des résultats
            Console.WriteLine("\nFin de la partie !");
            Console.WriteLine($"Score final de {joueur1.Nom} : {joueur1.Score}");
            Console.WriteLine($"Score final de {joueur2.Nom} : {joueur2.Score}");

            if (joueur1.Score > joueur2.Score)
            {
                Console.WriteLine($"Félicitations, {joueur1.Nom} a gagné !");
            }
            else if (joueur2.Score > joueur1.Score)
            {
                Console.WriteLine($"Félicitations, {joueur2.Nom} a gagné !");
            }
            else
            {
                Console.WriteLine("C'est une égalité !");
            }
        }

        /// <summary>
        /// Gère le tour d'un joueur, incluant la gestion du temps et l'affichage des scores.
        /// </summary>
        static void JouerUnTour(Joueur joueur, Dictionnaire dictionnaire, int taillePlateau, TimeSpan tempsRestant)
        {
            Console.WriteLine($"\n{joueur.Nom}, c'est votre tour ! Vous avez {tempsRestant.TotalSeconds} secondes pour jouer.");

            // Initialisation du plateau
            Plateau plateau = new Plateau(taillePlateau, dictionnaire);
            plateau.LancerTousLesDes(); // Mélanger les dés
            Console.WriteLine("\nVoici le plateau après mélange :");
            Console.WriteLine(plateau.toString());

            Stopwatch chronometre = Stopwatch.StartNew(); // Début du chrono

            while (chronometre.Elapsed < tempsRestant)
            {
                Console.Write("\nEntrez un mot (ou tapez 'quit' pour quitter) : ");
                string motSaisi = Console.ReadLine()?.ToUpper();

                if (motSaisi == "QUIT")
                {
                    Console.WriteLine($"{joueur.Nom} a décidé d'arrêter ce tour.");
                    break;
                }

                // Vérification des mots joués
                if (!dictionnaire.VerifierMotNonJoue(motSaisi))
                {
                    Console.WriteLine($"Le mot '{motSaisi}' a déjà été utilisé !");
                    continue;
                }

                // Test si le mot est valide
                if (!plateau.Test_Plateau(motSaisi, joueur))
                {
                    Console.WriteLine($"Désolé, le mot '{motSaisi}' est invalide.");
                    continue;
                }


                // Calcul des points pondérés
                int pointsMot = 0;
                foreach (var lettre in motSaisi)
                {
                    pointsMot += plateau.des.First(d => d.LettreVisible == lettre).ObtenirPoints(lettre);
                }
                joueur.AjouterPoints(pointsMot);

                Console.WriteLine($"Bravo ! Le mot '{motSaisi}' est valide.");
                Console.WriteLine($"Score ajouté : {pointsMot} points ! Score actuel de {joueur.Nom} : {joueur.Score}");
                Console.WriteLine($"Temps restant : {(tempsRestant - chronometre.Elapsed).TotalSeconds:F1} secondes");
            }

            chronometre.Stop();
            Console.WriteLine($"\nFin du tour de {joueur.Nom} !");
        }

    }

}

