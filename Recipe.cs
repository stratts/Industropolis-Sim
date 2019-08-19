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
    public static Dictionary<string, Recipe> _recipes = new Dictionary<string, Recipe>() {
        {
            "None", new Recipe() {
                Name = "None",
                InputItem = Item.None,
                InputCount = 0,
                OutputItem = Item.None,
                OutputCount = 0,
                ProcessingTime = 0
            } 
        },
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
            "Food", new Recipe() {
                Name = "Food",
                InputItem = Item.None,
                InputCount = 0,
                OutputItem = Item.Food,
                OutputCount = 1,
                ProcessingTime = 1
            } 
        }
    };

    public static Recipe GetRecipe(string name) {
        return _recipes[name];
    }

    public static ICollection<Recipe> GetRecipes() {
        List<Recipe> recipeList = new List<Recipe>(_recipes.Values);
        recipeList.Sort((a, b) => {
            return a.OutputItem.CompareTo(b.OutputItem);
        });
        return recipeList;
    }
}