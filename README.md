# Node_Programming

Projekt Unity (URP, Unity 6) implementujący prosty system **wizualnego programowania opartego na blokach funkcyjnych** (namespace `FunctionalBlocks`).

## Architektura

Projekt składa się z czterech głównych plików w `Assets/Scripts/`:

- **GraphAsset.cs** — `ScriptableObject` przechowujący definicję grafu: listę **bloków** (węzłów), **zmiennych** oraz ID bloku startowego. Definiuje też wszystkie typy danych: `Value` (Number, Bool, Vector3, Text), `BlockDef`, `VariableDef` i enumy operacji.

- **GraphRunner.cs** — `MonoBehaviour` odpowiedzialny za **wykonanie grafu** w runtime. Iteruje po blokach od `startBlockId`, wykonując je sekwencyjnie z limitem kroków (zabezpieczenie przed nieskończoną pętlą). Zarządza kontekstem wykonania (`ExecutionContext`) ze zmiennymi i referencjami do stworzonych obiektów.

- **Editor/GraphAssetEditor.cs** — Custom editor dla `GraphAsset` z możliwością **eksportu/importu grafu do/z JSON**.

- **Editor/BlockDefDrawer.cs** — Custom `PropertyDrawer` wyświetlający bloki kontekstowo — pokazuje tylko pola odpowiednie dla danego typu bloku.

## Dostępne typy bloków

| Typ | Funkcja |
|-----|---------|
| **Start** | Punkt wejścia grafu |
| **CreatePrimitive** | Tworzy obiekt 3D (Cube/Sphere/Cylinder) z pozycją, rotacją i skalą |
| **Transform** | Przesuwa/obraca/skaluje istniejący obiekt (tryb Set lub Add) |
| **SetNumber** | Ustawia wartość zmiennej liczbowej |
| **CompareNumber** | Porównuje dwie liczby i zapisuje wynik bool |
| **If** | Rozgałęzienie warunkowe (true/false branch) |

## Typy wartości

- **Number** — liczba zmiennoprzecinkowa (`float`)
- **Bool** — wartość logiczna
- **Vector3** — wektor 3D
- **Text** — łańcuch znaków (używany m.in. jako uchwyt do obiektów)

## Użycie

1. Utwórz asset grafu: **Create > Functional Blocks > Graph Asset**
2. W Inspectorze dodaj zmienne i bloki, łącząc je za pomocą pól `nextId` / `trueNextId` / `falseNextId`
3. Dodaj komponent `GraphRunner` do obiektu na scenie i przypisz asset grafu
4. Graf zostanie wykonany automatycznie przy starcie sceny (lub ręcznie przez Context Menu > Run From Asset)

Grafy można również eksportować i importować jako JSON za pomocą przycisków w Inspectorze.

## Podsumowanie

Projekt to prototyp silnika **node-based scripting** dla Unity — pozwala definiować proste programy (tworzenie obiektów, transformacje, warunki) jako graf bloków w `ScriptableObject`, edytować je w Inspectorze lub przez JSON, a następnie uruchamiać w scenie. Przypomina uproszczoną wersję Unity Visual Scripting, ale z własnym formatem danych i dedykowanym runtimem.
