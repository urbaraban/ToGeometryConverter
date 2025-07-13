using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ToGeometryConverter.Object;
using ToGeometryConverter.Object.Elements;

namespace ToGeometryConverter.Format
{
    public class GCODE : GCFormat
    {
        public GCODE() : base("GCODE beta", new string[5] { ".gcode", ".g", ".gco", ".nc", ".ncc" })
        {
            this.ReadFile = GetAsync;
        }


        public async Task<object> GetAsync(string Filepath)
        {
            if (File.Exists(Filepath) == true) {
                GCCollection gccollection = new GCCollection(Name);
                string[] lines = File.ReadAllLines(Filepath);

                GcodeCoordinate[] coordinates = GetGcodeCoordinates(lines);

                GCTools.SetProgress?.Invoke(0, coordinates.Length, $"Parse {this.Name}");

                for (int i = 0; i < coordinates.Length; i += 1)
                {
                    if (coordinates[0].Type == "G00")
                    {
                        PathGeometry pathPath = new PathGeometry();
                        PathFigure contourPath = new PathFigure();

                        pathPath.Figures.Add(contourPath);

                        contourPath.StartPoint = coordinates[i].GetPoint;

                        i += 1;

                        for (; i < coordinates.Length && coordinates[i].Type == "G01"; i += 1)
                        {
                            contourPath.Segments.Add(
                                new LineSegment(coordinates[i].GetPoint, true));
                        }

                        if (contourPath.Segments.Count > 0)
                            gccollection.Add(new GeometryElement(pathPath, string.Empty));

                        if (i < coordinates.Length && coordinates[i].Type == "G00") i -= 1;
                    }
                }

                return gccollection;
            }

            GCTools.SetProgress?.Invoke(0, 99, string.Empty);
            return null;
        }

        private GcodeCoordinate[] GetGcodeCoordinates(string[] lines)
        {
            List<GcodeCoordinate> coordinates = new List<GcodeCoordinate>();
            for (int i = 0; i < lines.Length; i += 1)
            {
                string[] splitLine = lines[i].Split(' ');
                if (splitLine[0] == "G00" || splitLine[0] == "G01")
                {
                    GcodeCoordinate coordinate = new GcodeCoordinate(splitLine[0], 0, 0, 0);
                    for (int j = 1; j < splitLine.Length; j += 1)
                    {
                        if (splitLine[j][0] == 'X') coordinate.X = GetDigit(splitLine[j]);
                        if (splitLine[j][0] == 'Y') coordinate.Y = GetDigit(splitLine[j]);
                        if (splitLine[j][0] == 'Z') coordinate.Z = GetDigit(splitLine[j]);
                    }
                    coordinates.Add(coordinate);
                }
            }
            return coordinates.ToArray();
        }

        private double GetDigit(string str)
        {
            return double.Parse(str.Remove(0, 1).Replace('.', ','));
        }
    }

    internal struct GcodeCoordinate
    {
        public string Type;
        public double X;
        public double Y;
        public double Z;

        public GcodeCoordinate(string type, double x, double y, double z)
        {
            this.Type = type;
            this.X = x; 
            this.Y = y; 
            this.Z = z;
        }

        public Point GetPoint => new Point(X, Y);
    }
}

