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

    private static string[] item = new string[]
    {
        "babiole,F,la",
        "breloque,F,la",
        "bibelot,M,le",
        "broutille,F,la",
        "bijou,M,le",
        "porte-bonheur,M,le",
        "chaîne,F,la",
        "bricole,F,la",
        "chaînette,F,la",
        "hochet,M,le",
        "Montre,F,la",
        "bronze,M,le",
        "curiosité,F,la",
        "étrangeté,F,l'",
        "bizarrerie,F,l'"
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
        "délavé,délavée"
    };
    private static string[] uncommonAdjectifItem = new string[]{
        "quelconque",
    };
    private static string[] rareAdjectifItem = new string[]{
        "intéressant,intéressante",
        "brillant,brillante",
    };
    private static string[] epicAdjectifItem = new string[]{
        "incroyable",
        "étincelant,étincelante",
        "exceptionnel,exceptionnelle"
    };
    private static string[] legendaryAdjectifItem = new string[]{
        "qui mérite une place dans un musé",
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
    
    public static string ItemNameGenerator(Rarity rarity)
    {
        string result = "";
        string[] it = item[Random.Range(0, item.Length)].Split(',');
        result += it[0] + " ";
        string[] adj;
        switch (rarity)
        {
            case Rarity.Uncommon:
                adj = uncommonAdjectifItem[Random.Range(0, uncommonAdjectifItem.Length)].Split(',');
                break;
            case Rarity.Rare:
                adj = rareAdjectifItem[Random.Range(0, rareAdjectifItem.Length)].Split(',');
                break;
            case Rarity.Epic:
                adj = epicAdjectifItem[Random.Range(0, epicAdjectifItem.Length)].Split(',');
                break;
            case Rarity.Legendary:
                adj = legendaryAdjectifItem[Random.Range(0, legendaryAdjectifItem.Length)].Split(',');
                break;
            default:
                adj = commonAdjectifItem[Random.Range(0, commonAdjectifItem.Length)].Split(',');
                break;
        }
        

        if(adj.Length > 1 && it[1].Equals(FEMININ_GENDER))
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
