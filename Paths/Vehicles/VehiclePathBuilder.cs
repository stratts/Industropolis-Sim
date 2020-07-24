using System;
using System.Numerics;
using System.Collections.Generic;

namespace Industropolis.Sim
{
    public class VehiclePathBuilder : PathBuilder<VehicleNode, VehiclePath>
    {
        private Map _map;

        public VehiclePathBuilder(Map map) : base(map.VehiclePaths)
        {
            _map = map;
        }

        public override VehiclePath MakePath(PathType type, VehicleNode source, VehicleNode dest)
        {
            switch (type)
            {
                case PathType.Rail: return new Rail(source, dest);
                case PathType.SimpleRoad: return new SimpleRoad(source, dest);
                case PathType.OneWayRoad: return new OneWayRoad(source, dest);
                case PathType.Driveway: return new Driveway(source, dest);
                /*case PathType.Rail: return new Rail(source, dest);*/
                default: return new SimpleRoad(source, dest);
            }
        }

        public override VehicleNode MakeNode(IntVector pos, PathCategory category)
        {
            switch (category)
            {
                case PathCategory.Rail: return new RailNode(pos);
                case PathCategory.Road: return new RoadNode(pos);
                default: return new RoadNode(pos);
            }
        }

        public override bool CanBuildAt(PathType type, IntVector pos, Vector2 direction)
        {
            if (_map.GetBuilding(pos) != null) return false;
            return base.CanBuildAt(type, pos, direction);
        }

        public override void BuildPath(PathType type, IntVector source, IntVector dest, bool buildFixed = false)
        {
            base.BuildPath(type, source, dest, buildFixed);
            if (source == dest) return;
        }

        public override void DeletePathSegment(IntVector pos)
        {
            base.DeletePathSegment(pos);

            //SaveGame.TestSave.Save(_map);
        }

        public void ConnectBuilding(Building building)
        {
            var entrance = building.Entrance;
            if (entrance == null)
                throw new ArgumentException("Building does not have an entrance");
            BuildPath(entrance.Type, entrance.Start, entrance.End, true);
            var start = _manager.GetNode(entrance.Start);
            if (start == null) throw new Exception("Could not build start node for building");
            entrance.Connect(start);
        }

        public void DisconnectBuilding(Building building)
        {
            var entrance = building.Entrance;
            if (entrance == null || entrance.Node == null)
                throw new ArgumentException("Building does not have an entrance node");
            VehicleNode start = entrance.Node;
            VehicleNode? end = _manager.GetNode(entrance.End);
            _manager.RemoveNode(start);
            if (end != null)
            {
                end.Fixed = false;
                if (CanMergeNode(end)) MergeNode(end);
                else if (end.Connections.Count == 0) _manager.RemoveNode(end);
            }
            entrance.Disconnect();
        }
    }
}
