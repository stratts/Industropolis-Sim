using System.Collections.Generic;

public struct Recipe
{
    public string Name { get; set; }
    public RecipeInput[] Input;
    public Item OutputItem { get; set; }
    public int OutputCount { get; set; }
    public int ProcessingTime { get; set; }
}

public struct RecipeInput
{
    public Item Item { get; set; }
    public int Count { get; set; }

    public RecipeInput(Item item, int count)
    {
        Item = item;
        Count = count;
    }
}

public static class Recipes
{
    public static Dictionary<string, Recipe> _recipes = new Dictionary<string, Recipe>() {
        {
            "None", new Recipe() {
                Name = "None",
                Input = new [] { new RecipeInput(Item.None, 0) },
                OutputItem = Item.None,
                OutputCount = 0,
                ProcessingTime = 0
            }
        },
        {
            "Log", new Recipe() {
                Name = "Log",
                Input = new [] {
                    new RecipeInput(Item.Wood, 5),
                    new RecipeInput(Item.Food, 10)
                    },
                OutputItem = Item.Log,
                OutputCount = 1,
                ProcessingTime = 1
            }
        },
        {
            "Food", new Recipe() {
                Name = "Food",
                Input = new [] { new RecipeInput(Item.None, 0) },
                OutputItem = Item.Food,
                OutputCount = 1,
                ProcessingTime = 1
            }
        }
    };

    public static Recipe GetRecipe(string name)
    {
        return _recipes[name];
    }

    public static ICollection<Recipe> GetRecipes()
    {
        List<Recipe> recipeList = new List<Recipe>(_recipes.Values);
        recipeList.Sort((a, b) =>
        {
            return a.OutputItem.CompareTo(b.OutputItem);
        });
        return recipeList;
    }
}