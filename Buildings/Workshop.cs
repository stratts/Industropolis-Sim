using System;
using System.Collections.Generic;

namespace Industropolis.Sim
{
    public class Workshop : ProductionBuilding
    {
        public Recipe Recipe
        {
            get
            {
                return _recipe;
            }
            set
            {
                _recipe = value;
                LoadRecipe(value);
            }
        }

        private Recipe _recipe;

        public event Action<Workshop>? RecipeChanged;

        public Workshop()
        {
            Type = BuildingType.Workshop;
            Recipe = Recipes.GetRecipe("None");
            Width = 2;
            Height = 2;
            Entrance = new BuildingEntrance(this, (0, 1), PathCategory.Road);
        }

        private void LoadRecipe(Recipe recipe)
        {
            RecipeChanged?.Invoke(this);
            var input = new DirectConsumer();
            foreach (RecipeInput i in recipe.Input)
            {
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
}
