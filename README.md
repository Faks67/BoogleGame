# BoogleGame

Jeu de Boggle en C# (.NET 8), en deux versions :
- **le jeu web** (`BoogleGame.Web`) : une vraie partie de Boggle dans le navigateur, sur ordinateur ou téléphone ;
- **le jeu console** d'origine, pour deux joueurs.

## Jouer dans le navigateur

Double-cliquer sur `lancer_jeu.cmd`, ou :

```
dotnet run --project BoogleGame.Web
```

puis ouvrir http://localhost:5090.

### Modes

| Mode | Principe |
|---|---|
| Partie solo | Une grille, un chrono, les 5 meilleurs scores gardés pour chaque réglage |
| Duel | 2 à 4 joueurs sur le même écran, chacun son tour sur la même grille. Comme au vrai Boggle, un mot trouvé par plusieurs joueurs ne rapporte rien |
| Défi du jour | La même grille 4×4 pour tout le monde, qui change chaque jour (3 minutes) |

Réglages : dictionnaire français, anglais ou mixte ; grille 4×4, 5×5 ou 6×6 ; durée de 1 à 5 minutes.

### Comment jouer

- Glissez le doigt ou la souris sur des lettres voisines (y compris en diagonale), sans réutiliser un dé.
- Ou touchez les lettres une à une, puis retouchez la dernière pour valider.
- Ou tapez le mot au clavier : le chemin s'allume sur la grille, `Entrée` pour valider, `Échap` pour effacer.
- « Tourner la grille » la fait pivoter d'un quart de tour, comme un vrai plateau.
- À la fin, tous les mots possibles sont révélés : touchez-en un pour voir son chemin sur la grille.

### Points (barème officiel)

| Longueur | 3-4 | 5 | 6 | 7 | 8+ |
|---|---|---|---|---|---|
| Points | 1 | 2 | 3 | 5 | 11 |

« Qu » est un seul dé qui compte pour deux lettres.

### Fonctionnement

- Les grilles sont tirées avec les vrais dés du Boggle (dés français ; dés anglais, et ceux du Big Boggle en 5×5 et 6×6) à partir d'une graine : une partie est reproductible (`FR-4-123456`). Pour les grandes grilles, le jeu de dés est réutilisé.
- Le serveur résout chaque grille (recherche en profondeur avec élagage par préfixe, `BoogleGame.Web/Moteur/Plateau.cs`) et écarte les grilles trop pauvres en mots.
- Les mots sont vérifiés côté serveur : présents sur la grille et dans le dictionnaire (`MotsPossiblesFR.txt`, `MotsPossiblesEN.txt`).
- Records et défis sont gardés dans le navigateur.

## Jeu console

```
dotnet run --project BoogleGame.csproj
```

1. Choisir la taille du plateau (`y` pour la personnaliser, `n` pour garder 4×4).
2. Entrer le nom des deux joueurs.
3. Choisir le dictionnaire : `1` français, `2` anglais, `3` les deux.
4. Chaque joueur tape ses mots pendant son tour ; `quit` termine le tour plus tôt.

`Lettres.txt` donne pour chaque lettre ses points et sa fréquence (`lettre;points;fréquence`, valeurs du Scrabble français).
