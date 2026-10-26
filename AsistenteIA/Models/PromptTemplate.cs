namespace AsistenteIA.Models;

public class PromptTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Text { get; set; } = string.Empty;
}

public static class PromptTemplates
{
    public static List<PromptTemplate> All { get; } =
    [
        new() { Name = "Explicar un concepto", Category = "Explicación",
                Text = "Explica con palabras sencillas qué es [TEMA] y da un ejemplo." },
        new() { Name = "Resumir un tema", Category = "Resumen",
                Text = "Resume en cinco puntos los aspectos más importantes de [TEMA]." },
        new() { Name = "Crear ejemplos", Category = "Ejemplos",
                Text = "Dame tres ejemplos prácticos de [TEMA] aplicados a la vida real." },
        new() { Name = "Preguntas de práctica", Category = "Estudio",
                Text = "Genera cinco preguntas de práctica sobre [TEMA] con su respuesta." },
        new() { Name = "Comparar conceptos", Category = "Comparación",
                Text = "Compara [CONCEPTO A] y [CONCEPTO B] indicando diferencias y similitudes." }
    ];
}