#Treasure Inc.

www.treasure-inc.fr

## Équipements des personnages

Les règles (génération des objets équipables, caractéristiques, compétences) sont dans le moteur : voir la section
« Équipements et personnages » de son README. Le client affiche et envoie les demandes à l'API.

- Un objet équipable a des `stats` (JSON du serveur, ex. `{"slot": 3, "for": 5, "spe1": "breakable"}`) : elles sont
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

© 2021 Arnaud Boidard