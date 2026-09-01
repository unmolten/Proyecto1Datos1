using System.Data;

public class Metodos
{   
    public static void mensajeshow(string mensaje)
    {
        string[] full = mensaje.Split("/");
        string instruccion = full[0];
        string datos = full[1];
        
        Console.WriteLine(full[0]);
        Console.WriteLine(full[1]);
    }
}