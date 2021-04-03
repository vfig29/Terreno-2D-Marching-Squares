using System;

public class EliRNG
{
    //Funções de RNG feitas por Eliote.
    public static bool RolarEscolha(out int retorno, int seed, params double[] args)
    {
        //Sempre ordenar os parametros de forma decrescente.
        double totalChance = 0;
        retorno = 0;
        for (int i = 0; i < args.Length; i++)
        {
            if (RolarTentativa(args[i], seed - i))
            {
                retorno = i;
            }
            totalChance += args[i];
        }
        if (totalChance != 1.0)
        {
            retorno = -1;
            return false;
        }
        else
        {
            return true;
        }
    }

    public static bool RolarTentativa(double chance, int seed)
    {
        Random rnd = new Random(seed);
        if (rnd.NextDouble() < chance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

