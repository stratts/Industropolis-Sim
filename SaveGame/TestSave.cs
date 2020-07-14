using System;

namespace Industropolis.Sim.SaveGame
{
    public static class TestSave
    {
        public static void Save(Map map)
        {
            Console.Write("Saving... ");
            var paths = map.VehiclePaths.Paths;
            var writer = new RowWriter("paths.csv", paths.Count);
            var start = DateTime.Now;
            foreach (var path in paths)
            {
                if (path.PathType == PathType.Driveway) continue;
                writer.WriteRow(
                    path.Source.Pos.X,
                    path.Source.Pos.Y,
                    path.Dest.Pos.X,
                    path.Dest.Pos.Y,
                    path.PathType,
                    path.Fixed
                );
            }
            writer.SaveFile();

            writer = new RowWriter("chunk.csv", ((32 * 32) + 1) * map.Chunks.Count);


            foreach (var chunk in map.Chunks)
            {
                writer.WriteRow(chunk.Pos, chunk.Size);
                for (int x = 0; x < chunk.Size.X; x++)
                {
                    for (int y = 0; y < chunk.Size.Y; y++)
                    {
                        var tile = chunk.Tiles[x, y];
                        if (tile.Resource == Item.None) writer.WriteRow(tile.Nutrients);
                        else writer.WriteRow(tile.Nutrients, tile.Resource, tile.ResourceCount);
                    }
                }
            }
            Console.Write("writing... ");
            writer.SaveFile();
            Console.WriteLine("done.");
            var end = DateTime.Now;
            var span = end - start;
            Console.WriteLine(span.TotalSeconds);
        }

        public static void Load(Map map)
        {
            var reader = new RowReader("paths.csv");
            reader.LoadFile();
            while (!reader.AtEnd)
            {
                var row = reader.ReadRow();
                var source = (int.Parse(row[0]), int.Parse(row[1]));
                var dest = (int.Parse(row[2]), int.Parse(row[3]));
                var type = Enum.Parse<PathType>(row[4]);
                var isFixed = bool.Parse(row[5]);
                map.BuildPath(type, source, dest, isFixed);
            }
        }
    }
}