using System.Collections.Generic;

public struct Recipe {
    public string Name { get; set; }
    public Item InputItem { get; set; }
    public int InputCount { get; set; }
    public Item OutputItem { get; set; }
    public int OutputCount { get; set; }
    public int ProcessingTime { get; set; }
}

public static class Recipes {
    public static Dictionary<string, Recipe> Recipe { get; set; } = new Dictionary<string, Recipe>() {
        {
            "Log", new Recipe() {
                Name = "Log",
                InputItem = Item.Wood,
                InputCount = 5,
                OutputItem = Item.Log,
                OutputCount = 1,
                ProcessingTime = 1
            } 
        },
        {
            "None", new Recipe() {
                Name = "None",
                InputItem = Item.None,
                InputCount = 0,
                OutputItem = Item.None,
                OutputCount = 0,
                ProcessingTime = 0
            } 
        }
    };
}