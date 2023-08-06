using System;

using Microsoft.Xna.Framework;

namespace Catan;

class Board : ICloneable
{
    public Board()
    {
        RobberPos = 0;
    }

    public object Clone()
    {
        Board clone = new()
        {
            RobberPos = RobberPos
        };

        for (int i = 0; i < 19; i++)
            clone.Tiles[i] = new Tile(Tiles[i], clone);
        
        for (int i = 0; i < 54; i++)
            clone.Nodes[i] = new Node(Nodes[i], clone);

        for (int i = 0; i < 72; i++)
            clone.Edges[i] = new Edge(Edges[i], clone);

        Array.Copy(Ports, clone.Ports, 9);

        return clone;
    }

    public void Init()
    {
        InitTiles();
        InitNodes();

        MapNodes();
        MapEdges();
        MapPorts();

        ResourceBank = new Resources(19, 19, 19, 19, 19);
    }

    private void InitTiles()
    {
        for (int i = 0; i < 19; i++)
            Tiles[i] = new Tile(i, this);

        Vector2 hexDistTL = new(-ShapeBatcher.SIN_60, 1.5f);
        Vector2 hexDistTL2 = hexDistTL * 2;

        Vector2 hexDistTR = new(-hexDistTL.X, hexDistTL.Y);
        Vector2 hexDistTR2 = hexDistTR * 2;

        Tiles[0].Position = hexDistTL2;
        Tiles[1].Position = hexDistTL + hexDistTR;
        Tiles[2].Position = hexDistTR2;
        Tiles[3].Position = hexDistTL2 - hexDistTR;
        Tiles[4].Position = hexDistTL;
        Tiles[5].Position = hexDistTR;
        Tiles[6].Position = hexDistTR2 - hexDistTL;
        Tiles[7].Position = hexDistTL2 - hexDistTR2;
        Tiles[8].Position = hexDistTL - hexDistTR;
        Tiles[9].Position = Vector2.Zero;
        Tiles[10].Position = hexDistTR - hexDistTL;
        Tiles[11].Position = hexDistTR2 - hexDistTL2;
        Tiles[12].Position = hexDistTL - hexDistTR2;
        Tiles[13].Position = -hexDistTR;
        Tiles[14].Position = -hexDistTL;
        Tiles[15].Position = hexDistTR - hexDistTL2;
        Tiles[16].Position = -hexDistTR2;
        Tiles[17].Position = -hexDistTL - hexDistTR;
        Tiles[18].Position = -hexDistTL2;
    }

    private void InitNodes()
    {
        for (int i = 0; i < 54; i++)
            Nodes[i] = new Node(i, this);

        Vector2 up = new(0, 1);
        Vector2 pointDistTL = new(-ShapeBatcher.SIN_60, .5f);
        Vector2 pointDistTR = new(-pointDistTL.X, pointDistTL.Y);

        for (int i = 0; i < 3; i++)
        {
            Nodes[i * 2].Position = Tiles[i].Position + pointDistTL;
            Nodes[(i * 2) + 1].Position = Tiles[i].Position + up;
        }
        Nodes[6].Position = Tiles[2].Position + pointDistTR;

        for (int i = 3; i < 7; i++)
        {
            Nodes[(i * 2) + 1].Position = Tiles[i].Position + pointDistTL;
            Nodes[(i + 1) * 2].Position = Tiles[i].Position + up;
        }
        Nodes[15].Position = Tiles[6].Position + pointDistTR;

        for (int i = 7; i < 12; i++)
        {
            Nodes[(i + 1) * 2].Position = Tiles[i].Position + pointDistTL;
            Nodes[(i * 2) + 3].Position = Tiles[i].Position + up;
        }
        Nodes[26].Position = Tiles[11].Position + pointDistTR;

        Nodes[27].Position = Tiles[7].Position - pointDistTR;
        for (int i = 12; i < 16; i++)
        {
            Nodes[(i + 2) * 2].Position = Tiles[i].Position + pointDistTL;
            Nodes[(i * 2) + 5].Position = Tiles[i].Position + up;
        }
        Nodes[36].Position = Tiles[15].Position + pointDistTR;
        Nodes[37].Position = Nodes[36].Position + pointDistTR;

        Nodes[38].Position = Nodes[28].Position - up;
        for (int i = 16; i < 19; i++)
        {
            Nodes[(i * 2) + 7].Position = Tiles[i].Position + pointDistTL;
            Nodes[(i + 4) * 2].Position = Tiles[i].Position + up;
        }
        Nodes[45].Position = Nodes[44].Position - pointDistTL;
        Nodes[46].Position = Nodes[45].Position + pointDistTR;

        for (int i = 16; i < 19; i++)
        {
            Nodes[(i * 2) + 15].Position = Tiles[i].Position - pointDistTR;
            Nodes[(i + 8) * 2].Position = Tiles[i].Position - up;
        }
        Nodes[53].Position = Nodes[52].Position + pointDistTR;
    }

    private void MapNodes()
    {
        // Row 1
        for (int i = 0; i < 3; i++)
        {
            MapAboveTile(i, i * 2);
            MapBelowTile(i, (i * 2) + 8);
        }

        // Row 2
        for (int i = 3; i < 7; i++)
        {
            MapAboveTile(i, (i * 2) + 1);
            MapBelowTile(i, (i * 2) + 11);
        }

        // Row 3
        for (int i = 7; i < 12; i++)
        {
            MapAboveTile(i, (i * 2) + 2);
            MapBelowTile(i, (i * 2) + 13);
        }

        // Row 4
        for (int i = 12; i < 16; i++)
        {
            MapAboveTile(i, (i * 2) + 4);
            MapBelowTile(i, (i * 2) + 14);
        }

        // Row 5
        for (int i = 16; i < 19; i++)
        {
            MapAboveTile(i, (i * 2) + 7);
            MapBelowTile(i, (i * 2) + 15);
        }
    }

    private void MapAboveTile(int tileIndex, int nodeIndex)
    {
        Nodes[nodeIndex].SetTileID(1, tileIndex);
        Tiles[tileIndex].SetNodeID(0, nodeIndex++);

        Nodes[nodeIndex].SetTileID(2, tileIndex);
        Tiles[tileIndex].SetNodeID(1, nodeIndex++);

        Nodes[nodeIndex].SetTileID(2, tileIndex);
        Tiles[tileIndex].SetNodeID(2, nodeIndex++);
    }

    private void MapBelowTile(int tileIndex, int nodeIndex)
    {
        Nodes[nodeIndex].SetTileID(1, tileIndex);
        Tiles[tileIndex].SetNodeID(3, nodeIndex++);

        Nodes[nodeIndex].SetTileID(0, tileIndex);
        Tiles[tileIndex].SetNodeID(4, nodeIndex++);

        Nodes[nodeIndex].SetTileID(0, tileIndex);
        Tiles[tileIndex].SetNodeID(5, nodeIndex++);
    }

    private void MapEdges()
    {
        for (int i = 0; i < 72; i++)
            Edges[i] = new Edge(i, this);

        // Row 1
        for (int i = 0; i < 6; i++)
        {
            Edges[i].SetNodeID(0, i);
            Edges[i].SetNodeID(1, i + 1);
        }

        // Row 2
        for (int i = 6; i < 10; i++)
        {
            Edges[i].SetNodeID(0, (i - 6) * 2);
            Edges[i].SetNodeID(1, (i - 2) * 2);
        }

        // Row 3
        for (int i = 10; i < 18; i++)
        {
            Edges[i].SetNodeID(0, i - 3);
            Edges[i].SetNodeID(1, i - 2);
        }

        // Row 4
        for (int i = 18; i < 23; i++)
        {
            Edges[i].SetNodeID(0, ((i - 15) * 2) + 1);
            Edges[i].SetNodeID(1, ((i - 10) * 2) + 1);
        }

        // Row 5
        for (int i = 23; i < 33; i++)
        {
            Edges[i].SetNodeID(0, i - 7);
            Edges[i].SetNodeID(1, i - 6);
        }

        // Row 6
        for (int i = 33; i < 39; i++)
        {
            Edges[i].SetNodeID(0, (i - 25) * 2);
            Edges[i].SetNodeID(1, ((i - 20) * 2) + 1);
        }

        // Row 7
        for (int i = 39; i < 49; i++)
        {
            Edges[i].SetNodeID(0, i - 12);
            Edges[i].SetNodeID(1, i - 11);
        }

        // Row 8
        for (int i = 49; i < 54; i++)
        {
            Edges[i].SetNodeID(0, (i - 35) * 2);
            Edges[i].SetNodeID(1, (i - 30) * 2);
        }

        // Row 9
        for (int i = 54; i < 62; i++)
        {
            Edges[i].SetNodeID(0, i - 16);
            Edges[i].SetNodeID(1, i - 15);
        }

        // Row 10
        for (int i = 62; i < 66; i++)
        {
            Edges[i].SetNodeID(0, ((i - 43) * 2) + 1);
            Edges[i].SetNodeID(1, ((i - 39) * 2) + 1);
        }

        // Row 11
        for (int i = 66; i < 72; i++)
        {
            Edges[i].SetNodeID(0, i - 19);
            Edges[i].SetNodeID(1, i - 18);
        }

        for (int i = 0; i < 72; i++)
        {
            Edge edge = Edges[i];

            for (int j = 0; j < 2; j++)
            {
                int n = -1;
                while (edge.GetNode(j).GetEdge(++n) != null);

                edge.GetNode(j).SetEdgeID(n, i);
            }
        }
        
        for (int i = 0; i < 72; i++)
            Edges[i].CalculatePosition();
    }

    private void MapPorts()
    {
        Ports[0] = new Port(Nodes[1], Nodes[0], Port.TradeType.Versatile);
        Ports[1] = new Port(Nodes[4], Nodes[3], Port.TradeType.Grain);
        Ports[2] = new Port(Nodes[15], Nodes[14], Port.TradeType.Ore);
        Ports[3] = new Port(Nodes[7], Nodes[17], Port.TradeType.Lumber);
        Ports[4] = new Port(Nodes[37], Nodes[26], Port.TradeType.Versatile);
        Ports[5] = new Port(Nodes[28], Nodes[38], Port.TradeType.Brick);
        Ports[6] = new Port(Nodes[45], Nodes[46], Port.TradeType.Wool);
        Ports[7] = new Port(Nodes[47], Nodes[48], Port.TradeType.Versatile);
        Ports[8] = new Port(Nodes[50], Nodes[51], Port.TradeType.Versatile);
    }

    public Tile[] Tiles = new Tile[19];
    public int RobberPos;

    public Node[] Nodes = new Node[54];

    public Edge[] Edges = new Edge[72];

    public Port[] Ports = new Port[9];

    public Resources ResourceBank;
}