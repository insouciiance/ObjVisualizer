﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Engine3D.OctTree
{
    internal class OctTree
    {
        private const int MinSize = 1;

        public BoundingBox Region { get; init; }
        public OctTree[] Children { get; init; }
        public List<Face> Faces { get; init; }

        public OctTree(BoundingBox maxRegion, IEnumerable<Face> faces)
        {
            Region = maxRegion;
            Faces = faces.ToList();
        }

        private void BuildTree()
        {
            if (Faces.Count <= 1) return;

            Vector3 dimensions = Region.Max - Region.Min;

            if (dimensions.X <= MinSize && dimensions.Y <= MinSize && dimensions.Z <= MinSize) return;

            Vector3 half = dimensions / 2;
            Vector3 center = Region.Min + half;

            BoundingBox[] childRegions = new BoundingBox[8];
            childRegions[0] = new BoundingBox(Region.Min, center);
            childRegions[1] = new BoundingBox(new Vector3(center.X, Region.Min.Y, Region.Min.Z), new Vector3(Region.Max.X, center.Y, center.Z));
            childRegions[2] = new BoundingBox(new Vector3(center.X, Region.Min.Y, center.Z), new Vector3(Region.Max.X, center.Y, Region.Max.Z));
            childRegions[3] = new BoundingBox(new Vector3(Region.Min.X, Region.Min.Y, center.Z), new Vector3(center.X, center.Y, Region.Max.Z));
            childRegions[4] = new BoundingBox(new Vector3(Region.Min.X, center.Y, Region.Min.Z), new Vector3(center.X, Region.Max.Y, center.Z));
            childRegions[5] = new BoundingBox(new Vector3(center.X, center.Y, Region.Min.Z), new Vector3(Region.Max.X, Region.Max.Y, center.Z));
            childRegions[6] = new BoundingBox(center, Region.Max);
            childRegions[7] = new BoundingBox(new Vector3(Region.Min.X, center.Y, center.Z), new Vector3(center.X, Region.Max.Y, Region.Max.Z));

            List<Face>[] regionsElements = new List<Face>[8];
            for (int i = 0; i < 8; i++)
            {
                regionsElements[i] = new List<Face>();
            }

            List<Face> nextElements = new();

            foreach (Face face in Faces)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (IsFaceInsideBox(face, childRegions[i]))
                    {
                        regionsElements[i].Add(face);
                        nextElements.Add(face);
                        break;
                    }
                }
            }

            foreach (Face nextFace in nextElements)
            {
                Faces.Remove(nextFace);
            }

            for (int i = 0; i < 8; i++)
            {
                if (regionsElements[i].Count != 0)
                {
                    Children[i] = CreateNode(childRegions[i], regionsElements[i]);
                    Children[i].BuildTree();
                }
            }
        }

        private OctTree CreateNode(BoundingBox region, IEnumerable<Face> faces)
        {
            OctTree tree = new(region, faces);
            return tree;
        }

        private bool IsFaceInsideBox(Face f, BoundingBox box)
        {
            foreach (Vector3 point in f.Points)
            {
                if (Vector3.Min(box.Min, point) == box.Min && Vector3.Max(box.Max, point) == box.Max)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
