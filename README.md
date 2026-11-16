#Treasure Inc.

www.treasure-inc.fr

## Équipements des personnages

Les règles (génération des objets équipables, caractéristiques, compétences) sont dans le moteur : voir la section
« Équipements et personnages » de son README. Le client affiche et envoie les demandes à l'API.

- Un objet équipable a des `stats` (JSON du serveur, ex. `{"slot": 3, "for": 5, "spe1": "finish_him"}`) : elles sont
  affichées dans l'écran de l'objet au-dessus de la description, et l'objet porte une icône « gear » dans l'inventaire.
- Les textes des compétences (`spe1`, `spe2`…) sont locaux au client : `Equipment.cs`.
- La sélection automatique de la vente a une case « équipables », décochée par défaut : les objets équipables ne sont
  alors jamais sélectionnés.
- Écran d'équipe (`TeamPanel`) : un clic sur un emplacement ouvre l'inventaire sur les objets qui y vont, un clic sur
  l'un d'eux l'équipe (il quitte l'inventaire). « Retirer » remet l'objet porté dans l'inventaire.
- À l'envoi d'une expédition, le joueur choisit le personnage qui la mène (`CharacterChooser`) : c'est lui qui marche
  dans le mini-jeu. Il est indisponible jusqu'à l'ouverture du coffre ; quand tous sont partis, on ne peut plus ni
  creuser ni envoyer d'expédition.

### Numéros des emplacements

Ce sont ceux du moteur. `TeamPanel` retrouve chaque emplacement par son nom dans la scène (`TeamPanel/Crew/Inventory`) :

| N° | Emplacement | Objet de la scène | Position |
|---|---|---|---|
| 1 | Tête | `GameObject` | haut |
| 2 | Armure | `GameObject_1` | centre |
| 3 | Arme | `GameObject_4` | gauche |
| 4 | Bouclier | `GameObject_3` | droite |
| 5 | Bottes | `GameObject_2` | bas |
| 6 | Gants | `GameObject_6` | bas gauche |
| 7 | Anneau | `GameObject_5` | bas droite |
| 8 | Amulette | `GameObject_7` | petit, haut gauche |
| 9 | Talisman | `GameObject_8` | petit, haut droite |

Les personnages sont numérotés à partir de 1, dans l'ordre des portraits de `TeamPanel/ListCrewMask/ListCrew`.
Les éléments ajoutés par cette fonctionnalité (case « équipables », bouton « Retirer », icône, choix du personnage)
sont créés par les scripts au lancement : la scène et les prefabs ne sont pas modifiés.


### Skins des équipements

Le `graphics` d'un objet est son rang dans `ItemManager.sprites` : les 181 sprites de la planche `roguelikeitems`
(réglés dans la scène), puis 144 skins dessinés par le code (`ItemArt`), ajoutés au lancement : 16 par famille
d'équipement (épées, boucliers, armures, casques, bottes, gants, anneaux, amulettes, talismans), soit 4 formes peintes
chacune avec 4 jeux de couleurs. Le moteur connaît ces numéros (`Equipment.Kind`, `engine.items.graphics-count`) et un
objet en base garde le sien : on n'ajoute une famille, une forme ou un jeu de couleurs qu'en accord avec le moteur, et
on ne change jamais l'ordre des existants. Une forme est une grille de 16 × 16 caractères, décrite en tête d'`ItemArt.cs`.

## Police

Tous les textes utilisent `Assets/Font/Jersey15-Regular.ttf` (Jersey 15, licence OFL jointe), une police pixel lisible
à petite taille. Les textes créés par le code reprennent celle d'un texte de la scène (`UIBuilder.ModelText`).

## Obstacles et combats

Tout est décidé par le moteur à l'envoi de l'expédition (voir « Obstacles et combats » dans son README) : la réponse
contient `run`, le chemin du personnage étage par étage, que le mini-jeu (`ExpeditionRunPanel`) se contente de rejouer.

- **Barricade** : le personnage s'arrête et frappe le temps indiqué par le serveur, puis la barricade casse.
- **Monstre** : le combat est rejoué événement par événement (coups, critiques, soins, esquives, compétences), avec les
  barres de vie du personnage (celle du prefab `CrewMember`) et du monstre. Toucher l'écran n'accélère ni l'un ni l'autre.
- **Gardien** : il cache le coffre de fin d'étage, qui apparaît à sa mort.
- **Mort** : si la vie du personnage tombe à zéro, l'expédition revient aussitôt ; l'écran de retour dit à quel étage il
  est tombé, et le serveur ne donne que la part des étages terminés.
- Les monstres et la barricade sont dessinés par le code (`MonsterArt`, une forme par `kind` du serveur), comme les
  textes qui s'envolent (`EncounterView`, `FloatingText`) : ni la scène ni les prefabs ne sont modifiés.
- Les textes des compétences sont dans `Equipment.cs`. L'écran d'équipe ne montre que le nom, le rang et le niveau du
  personnage : son bouton « Stats » ouvre un écran défilant (`CharacterDetails`, construit par le code) avec ce qu'il
  vaut en combat (`combat` : PV, attaque, défense, vitesse, critique), ses caractéristiques et ses compétences propres
  avec celles de son équipement. Chaque compétence y a son encadré (`SkillBox`) : son nom en en-tête, son effet
  dessous ; un clic sur l'en-tête replie ou déplie l'encadré, ouvert par défaut.
- Le doigt qui invitait à toucher l'écran pendant le mini-jeu n'est plus affiché (l'objet `Touch` du prefab est
  désactivé par `ExpeditionRunPanel`).
- Nouvelle caractéristique : l'agilité (`agi`), surtout donnée par les bottes.

## Niveaux et expérience

Les règles (gain, longueur des niveaux, points de caractéristique) sont dans le moteur : voir « Niveaux et expérience »
dans son README. Le client ne calcule rien : il affiche ce que le serveur répond.

- Chaque personnage reçu de l'API a un `level` (1 à 100), l'expérience acquise dans ce niveau (`xp`) et ce que le niveau
  demande (`xpNext`, 0 au niveau 100). Ses `stats` sont déjà celles de son niveau.
- L'ouverture d'un coffre répond `{items, xp}`. L'écran de retour d'expédition (`LootPanel`) montre les objets puis, en
  dessous, ce que l'expédition a rapporté à son personnage (`XpGainView`) : l'expérience gagnée, une barre qui se
  remplit, le passage de niveau et les caractéristiques gagnées. Le contenu de l'écran défile quand il dépasse.
- Le niveau est affiché dans l'écran d'équipe, dans le choix du personnage d'une expédition et dans la collection du
  recrutement.

## Tris, filtres et favoris

Les trois écrans qui listent des personnages (équipe, choix du personnage d'une expédition, collection du recrutement)
ont la même rangée de boutons (`CharacterFilterBar`), de gauche à droite :

- **Tri** : un clic passe au suivant (numéro, rang, niveau, puissance, nom). La puissance est la somme des
  caractéristiques, équipement compris.
- **Flèche** : sens du tri.
- **Étoile** : n'afficher que les favoris.
- **Dispo** : n'afficher que les personnages qui ne sont pas en expédition (absent de la collection).
- **D … SSS** : masquer ou afficher les personnages d'un rang.

Chaque écran garde ses propres réglages (`CharacterListOptions`), conservés dans les préférences de l'appareil.

Un personnage devient **favori** par l'étoile de son portrait (grand portrait de l'écran d'équipe, ou coin du portrait
dans le choix du personnage). Les favoris passent toujours en tête des listes, quel que soit le tri. Ils sont conservés
dans les préférences de l'appareil, par compte (`CharacterFavorites`) : le serveur ne les connaît pas.

## Quêtes quotidiennes et hebdomadaires

Chaque jour et chaque semaine, le serveur propose au joueur quelques quêtes (3 par défaut) : « Réalisez 5 expéditions »,
« Combattez 20 monstres », « Ramassez 100 objets », « Invoquez 20 personnages »… Le client ne décide de rien : il lit
les quêtes (`GET /V1/user/{id}/quests`), écrit leur phrase à partir de ce qu'elles comptent (`Quest.Describe`) et
demande la récompense d'une quête terminée (`POST /V1/user/{id}/quests/{quête}/claim`).

- **Bouton** (`QuestButton`) : une petite icône de liste, en haut à gauche de l'écran, à gauche du HUD. Une pastille
  rouge y compte les quêtes terminées dont la récompense attend.
- **Écran** (`QuestPanel`) : les quêtes quotidiennes puis les hebdomadaires, chacune avec sa progression, sa récompense
  et son **coffre**. Le coffre d'une quête terminée bat : un clic donne l'argent et les objets, montrés dans l'écran des
  objets trouvés (`LootPanel.ShowItems`). Le temps restant avant les prochaines quêtes est affiché ; une récompense non
  récupérée à ce moment-là est perdue.
- **Mise à jour** (`QuestManager`) : les quêtes sont relues, sans écran de chargement, après chaque action qui peut
  compter (coffre ouvert, invocation, vente, case creusée), à l'ouverture de l'écran et à la fin d'une période.

Le bouton et l'écran sont construits par code (`UIBuilder`), sans rien changer aux scènes : `QuestPanel.Create` est
appelé par `TeamPanel`, dont il reprend le cadre.

© 2021 Arnaud Boidard
