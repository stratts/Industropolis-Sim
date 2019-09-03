using System.Collections.Generic;

public class Workshop : ProductionBuilding {
    public Recipe Recipe {
        get {
            return _recipe;
        }
        set {
            _recipe = value;
            LoadRecipe(value);
        }
    }

    private Recipe _recipe;

    public Workshop() {
        Type = BuildingType.Workshop;
        _requiredResources = new Dictionary<Item, int>() {
            {Item.Wood, 50}
        };
        Recipe = Recipes.GetRecipe("None");
    }

    private void LoadRecipe(Recipe recipe) {
        var input = new DirectConsumer();
        foreach (RecipeInput i in recipe.Input) {
            input.AddItem(i.Count * 2, i.Count, i.Item);
        }
        Input = input;
        Consumer = input;
        var producer = new DirectProducer(recipe.OutputCount * 5, recipe.OutputCount, recipe.OutputItem);
        Producer = producer;
        Output = producer;
        ProcessingTime = recipe.ProcessingTime;
    }
}