using UnityEngine;

public static class StringGenerator
{
    private static string FEMININ_GENDER = "F";
    //private static string MASCULIN_GENDER = "M";
    //private static string NEUTRAL_GENDER = "N";

    private static string[] expedition = new string[] {
        "Expédition,F,l'",
        "Aventure,F,l'",
        "Baroud,M,le",
        "Errance,F,l'",
        "Épisode,M,l'",
        "Épreuve,F,l'",
        "Péripétie,F,la",
        "Imprudence,F,l'",
        "Conquête,F,la",
        "Rencontre,F,la",
        "Mésaventure,F,la",
        "Équipée,F,l'",
        "Destin,M,le",
        "Entreprise,F,l'",
        "Odyssée,F,l'",
        "Évenement,M,l'"
    };

    private static string[] adjectifExpedition = new string[]{
        "douloureux,douloureuse",
        "incroyable",
        "fructueux,fructueuse",
        "enrichissant,enrichissante",
        "rémunérateur,rémunératrice",
        "lucratif,lucrative",
        "rentable",
        "salutaire",
        "fertile",
        "productif,productive",
        "intéressant,intéressante",
        "productif,productive",
        "juteux,juteuse",
        "fécond,féconde",
        "intarissable",
        "généreux,généreuse"
    };

    private static string[] commonItem = new string[]
    {
        "babiole,F,la",
        "breloque,F,la",
        "bibelot,M,le",
        "broutille,F,la",
        "bijou,M,le",
        "porte-bonheur,M,le",
        "bricole,F,la",
        "bronze,M,le",
        "curiosité,F,la",
        "étrangeté,F,l'",
        "ornement,M,l'",
        "bizarrerie,F,l'",
        "décoration,F,la"
    };

    private static string[] rareItem = new string[]
    {
        "bijou,M,le",
        "porte-bonheur,M,le",
        "bronze,M,le",
        "ornement,M,l'",
        "décoration,F,la",
        "chef-d'oeuvre,M,le"
    };

    private static string[] commonAdjectifItem = new string[]{
        "insignifiant,insignifiante",
        "inintéressant,inintéressante",
        "terne",
        "fade",
        "livide",
        "quelconque",
        "usé,usée",
        "grisâtre",
        "délavé,délavée",
        "banal",
        "ordinaire",
        "courant",
        "habituel,habituelle"
    };
    private static string[] uncommonAdjectifItem = new string[]{
        "original,originale",
        "étonnant,étonnante",
        "étrange",
        "atypique",
        "inhabituel,inhabituelle",
        "singulier,singulière",
        "anormal,anormale",
        "inusuel,inusuelle",
        "insolite"
    };
    private static string[] rareAdjectifItem = new string[]{
        "intéressant,intéressante",
        "brillant,brillante",
        "admirable",
        "précieux,précieuse",
        "remarquable",
        "surprenant,surprenante",
        "rare",
        "rarissime"
    };
    private static string[] epicAdjectifItem = new string[]{
        "incroyable",
        "étincelant,étincelante",
        "exceptionnel,exceptionnelle",
        "spectaculaire",
        "d'exception",
        "sans pareil",
        "parfait,parfaite",
        "sensationnel,sensationnelle",
        "prodigieux,prodigieuse",
        "parfait,parfaite"
    };
    private static string[] legendaryAdjectifItem = new string[]{
        "légendaire",
        "fabuleux,fabuleuse",
        "mithique",
        "fantastique",
        "transcendant,transcendante"
    };

    private static string[] descriptionItem = new string[]{
        "Sa place est dans un musée !",
        "Objet bien mais pas top, il aurait été inventé il y a longtemps avant JC.",
        "Mais qu'est ce que c'est que cette matière ? mais c'est... c'est...",
        "Un certain professeur Johanes l'a cherché toute sa vie.",
        "De fabrication artisanale, il a été réalisé grace à un mouvement manuel rotatif en s'aidant de la région axillaire.",
        "C'est une de mes plus belle pièce...",
        "Mais commençons par le début, qu'est-ce qu'un pont suspendu ?"
    };

    public static string ExpeditionNameGenerator()
    {
        string result = "";
        string[] ex = expedition[Random.Range(0, expedition.Length)].Split(',');
        result += ex[0] + " ";

        string[] adj = adjectifExpedition[Random.Range(0, adjectifExpedition.Length)].Split(',');
        if(adj.Length > 1 && ex[1].Equals(FEMININ_GENDER))
        {
            result += adj[1];
        }
        else
        {
            result += adj[0];
        }

        return result;
    }
    
    public static string ItemDescriptionGenerator()
    {
        return descriptionItem[Random.Range(0, descriptionItem.Length)];
    }
    public static string ItemNameGenerator(Rarity rarity)
    {
        string result = "";
        string[] adj;
        string[] item; 

        switch (rarity)
        {
            case Rarity.Uncommon:
                adj = uncommonAdjectifItem[Random.Range(0, uncommonAdjectifItem.Length)].Split(',');
                item = commonItem[Random.Range(0, commonItem.Length)].Split(',');
                break;
            case Rarity.Rare:
                adj = rareAdjectifItem[Random.Range(0, rareAdjectifItem.Length)].Split(',');
                item = rareItem[Random.Range(0, rareItem.Length)].Split(',');
                break;
            case Rarity.Epic:
                adj = epicAdjectifItem[Random.Range(0, epicAdjectifItem.Length)].Split(',');
                item = rareItem[Random.Range(0, rareItem.Length)].Split(',');
                break;
            case Rarity.Legendary:
                adj = legendaryAdjectifItem[Random.Range(0, legendaryAdjectifItem.Length)].Split(',');
                item = rareItem[Random.Range(0, rareItem.Length)].Split(',');
                break;
            default:
                adj = commonAdjectifItem[Random.Range(0, commonAdjectifItem.Length)].Split(',');
                item = commonItem[Random.Range(0, commonItem.Length)].Split(',');
                break;
        }
        
        result += item[0] + " ";

        if(adj.Length > 1 && item[1].Equals(FEMININ_GENDER))
        {
            result += adj[1];
        }
        else
        {
            result += adj[0];
        }

        return result;
    }
}
