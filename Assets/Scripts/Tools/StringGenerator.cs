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

    public static string ExpeditionNameGenerator()
    {
        string result = "";
        string[] ex = expedition[Random.Range(0, expedition.Length)].Split(',');
        result += ex[0] + " ";

        string[] adj = adjectifExpedition[Random.Range(0, adjectifExpedition.Length)].Split(',');
        if (adj.Length > 1 && ex[1].Equals(FEMININ_GENDER))
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
