# BoogleGame

Jeu de Boggle en console C# (.NET 8) pour deux joueurs.

Chaque joueur a 60 secondes pour trouver des mots sur un plateau de dés mélangés (4x4 par défaut, de 2x2 à 16x16). Les mots sont vérifiés dans un dictionnaire français, anglais ou les deux, et chaque lettre rapporte des points selon sa valeur.

## Lancer le jeu

Prérequis : [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

```
dotnet run --project BoogleGame.csproj
```

## Déroulement

1. Choisir la taille du plateau (`y` pour la personnaliser, `n` pour garder 4x4).
2. Entrer le nom des deux joueurs.
3. Choisir le dictionnaire : `1` français, `2` anglais, `3` les deux.
4. Chaque joueur tape ses mots pendant son tour ; `quit` termine le tour plus tôt.

## Fichiers de données

- `MotsPossiblesFR.txt`, `MotsPossiblesEN.txt` : dictionnaires (mots séparés par des espaces).
- `Lettres.txt` : une ligne par lettre au format `lettre;points;fréquence`. Les valeurs reprennent celles du Scrabble français.

Ces fichiers sont copiés à côté de l'exécutable à la compilation et lus depuis ce dossier.
