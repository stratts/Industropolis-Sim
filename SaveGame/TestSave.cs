using System;

namespace Industropolis.Sim.SaveGame
{
    public static class TestSave
    {
        public static void Save(Map map)
        {
            var paths = map.VehiclePaths.Paths;
            var writer = new RowWriter("paths.csv", paths.Count);
            foreach (var path in paths)
            {
                if (path.PathType == PathType.Driveway) continue;
                writer.WriteRow(
                    path.Source.Pos.X.ToString(),
                    path.Source.Pos.Y.ToString(),
                    path.Dest.Pos.X.ToString(),
                    path.Dest.Pos.Y.ToString(),
                    path.PathType.ToString(),
                    path.Fixed.ToString()
                );
            }
            writer.SaveFile();
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