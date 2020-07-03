using System;
using System.Collections.Generic;

namespace Industropolis.Sim
{
    public class RoadBuilder : PathBuilder<RoadNode, Road>
    {
        private Map _map;

        public RoadBuilder(Map map) : base(map.Roads)
        {
            _map = map;
        }

        public override Road MakePath(PathType type, RoadNode source, RoadNode dest)
        {
            switch (type)
            {
                case PathType.SimpleRoad: return new SimpleRoad(source, dest);
                case PathType.OneWayRoad: return new OneWayRoad(source, dest);
                /*case PathType.Rail: return new Rail(source, dest);*/
                default: return new SimpleRoad(source, dest);
            }
        }

        public override RoadNode MakeNode(IntVector pos) => new RoadNode(pos);

        public override bool CanBuildAt(PathType type, IntVector pos)
        {
            if (_map.GetBuilding(pos) != null) return false;
            if (_map.Rails.GetPath(pos) != null || _map.Rails.GetNode(pos) != null) return false;
            return true;
        }

        public override void BuildPath(PathType type, IntVector source, IntVector dest)
        {
            base.BuildPath(type, source, dest);
            if (source == dest) return;

            foreach (var current in source.GetPointsBetween(dest))
            {
                foreach (var building in _map.Buildings)
                {
                    if (building.HasEntrance && building.Entrance != null &&
                        building.Entrance.CanConnect(current, GetCategory(type)))
                    {
                        ConnectBuilding(building);
                    }
                }
            }
        }

        public void ConnectBuilding(Building building)
        {
            var entrance = building.Entrance;
            if (entrance == null)
                throw new ArgumentException("Building does not have an entrance");
            var p = _manager.GetPath(entrance.ConnectionPos);
            var n = _manager.GetNode(entrance.ConnectionPos);
            if ((p != null && p.Category == entrance.Category) || (n != null && n.Category == entrance.Category))
            {
                var node = new BuildingNode(entrance.Pos, building);
                entrance.Connect(node);
                if (entrance.Node == null) throw new Exception("Could not connect entrance");
                _manager.AddNode(entrance.Node);
                BuildPath(PathType.SimpleRoad, entrance.Pos, entrance.ConnectionPos);
            }
        }

        public void DisconnectBuilding(Building building)
        {
            if (building.Entrance == null || building.Entrance.Node == null)
                throw new ArgumentException("Building does not have an entrance node");
            RoadNode n = building.Entrance.Node;
            RoadNode? pathCon = _manager.GetNode(n.Pos + new IntVector(0, 1));
            _manager.RemoveNode(n);
            if (pathCon != null && CanMergeNode(pathCon)) MergeNode(pathCon);
            building.Entrance.Disconnect();
        }
    }
}
