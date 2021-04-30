using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;

namespace ClassLibrary1
{
    [Serializable]
    public class V5DataOnGrid : V5Data, IEnumerable, ISerializable
    {
        public Grid2D Grid { get; set; }
        public Vector2[,] ValuesGrid { get; set; } = { };

        public V5DataOnGrid(string serviceInfo, DateTime measurementTime, Grid2D grid) :
            base(serviceInfo, measurementTime)
        {
            Grid = grid;
        }

        public V5DataOnGrid(string filename)
            : base("", new DateTime(1970, 1, 1))
        {
            /* 
             * The data in file looks like this:
             *     serviceInfo (string)
             *     measurementTime (DateTime)
             *     grid: nodesNumberX (int) grid: nodesNumberY (int)
             *     grid: StepSizeX (float) grid: StepSizeY (float)
             *     ValuesGrid[0, 0] ValuesGrid[0, 1] ... ValuesGrid[0, nodesNumberY]
             *     ...
             *     ValuesGrid[nodesNumberX, 0] ... ValuesGrid[nodesNumberX, nodesNumberY]
             *     (ValuesGrid[i, j] looks like float, float)
             */

            try
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(filename))
                {
                    ServiceInfo = sr.ReadLine();
                    MeasurementTime = Convert.ToDateTime(sr.ReadLine());

                    string[] first_str = sr.ReadLine().Split(' ');
                    string[] second_str = sr.ReadLine().Split(' ');

                    CultureInfo ru_culture_info = new CultureInfo("ru-RU");

                    Grid = new Grid2D(int.Parse(first_str[0]), int.Parse(first_str[1]),
                        float.Parse(second_str[0], ru_culture_info),
                        float.Parse(second_str[1], ru_culture_info));

                    ValuesGrid = new Vector2[Grid.NodesNumberX, Grid.NodesNumberY];
                    for (int i = 0; i < Grid.NodesNumberX; ++i)
                    {
                        string[] read_str = sr.ReadLine().Split(' ');

                        for (int j = 0; j < Grid.NodesNumberY; ++j)
                        {
                            ValuesGrid[i, j] = new Vector2(
                                float.Parse(read_str[2 * j], ru_culture_info),
                                float.Parse(read_str[2 * j + 1], ru_culture_info));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (exception is System.IO.FileNotFoundException)
                {
                    Console.WriteLine("File not found");
                }
                else if (exception is FormatException ||
                    exception is NullReferenceException ||
                    exception is IndexOutOfRangeException)
                {
                    Console.WriteLine("Incorrect file format");
                }
                else
                {
                    Console.WriteLine("Unexpected error");
                }

                throw exception;
            }
        }

        public static implicit operator V5DataCollection(V5DataOnGrid v5DataOnGridInstance)
        {
            V5DataCollection v5DataCollectionInstance =
                new V5DataCollection(v5DataOnGridInstance.ServiceInfo,
                                     v5DataOnGridInstance.MeasurementTime);

            for (int i = 0; i < v5DataOnGridInstance.Grid.NodesNumberX; ++i)
            {
                for (int j = 0; j < v5DataOnGridInstance.Grid.NodesNumberY; ++j)
                {
                    float x = v5DataOnGridInstance.Grid.StepSizeX * i;
                    float y = v5DataOnGridInstance.Grid.StepSizeY * j;
                    v5DataCollectionInstance.ValuesDct[new Vector2(x, y)] =
                        v5DataOnGridInstance.ValuesGrid[i, j];
                }
            }

            return v5DataCollectionInstance;
        }

        public void InitRandom(float minValue, float maxValue)
        {
            ValuesGrid = new Vector2[Grid.NodesNumberX, Grid.NodesNumberY];
            Random rand = new Random();
            for (int i = 0; i < Grid.NodesNumberX; ++i)
            {
                for (int j = 0; j < Grid.NodesNumberY; ++j)
                {
                    ValuesGrid[i, j] =
                        new Vector2(minValue + (maxValue - minValue) * (float)rand.NextDouble(),
                                    minValue + (maxValue - minValue) * (float)rand.NextDouble());
                }
            }
        }

        public override Vector2[] NearEqual(float eps)
        {
            List<Vector2> NearValues = new List<Vector2>();
            foreach (Vector2 value in ValuesGrid)
            {
                if (Math.Abs(value.X - value.Y) < eps)
                {
                    NearValues.Add(value);
                }
            }
            return NearValues.ToArray();
        }

        public override string ToString()
        {
            return "Type: V5DataOnGrid\n" + base.ToString() + "\n" + Grid.ToString();
        }

        public override string ToLongString()
        {
            StringBuilder strBulder = new StringBuilder(ToString() + "\n");

            for (int i = 0; i < Grid.NodesNumberX; ++i)
            {
                for (int j = 0; j < Grid.NodesNumberY; ++j)
                {
                    strBulder.Append($"x = {i * Grid.StepSizeX}  y = {j * Grid.StepSizeY}: ");
                    strBulder.Append($"{ValuesGrid[i, j]}\n");
                }
            }
            return strBulder.ToString();
        }

        public override string ToLongString(string format)
        {
            StringBuilder strBulder = new StringBuilder(ToString() + "\n");

            for (int i = 0; i < Grid.NodesNumberX; ++i)
            {
                for (int j = 0; j < Grid.NodesNumberY; ++j)
                {
                    strBulder.Append($"x = {(i * Grid.StepSizeX).ToString(format)} " +
                        $"y = {(j * Grid.StepSizeY).ToString(format)}: ");
                    strBulder.Append($"{ValuesGrid[i, j].ToString(format)}\n");
                }
            }
            return strBulder.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override IEnumerator<DataItem> GetEnumerator()
        {
            for (int i = 0; i < Grid.NodesNumberX; ++i)
            {
                for (int j = 0; j < Grid.NodesNumberY; ++j)
                {
                    yield return new DataItem(new Vector2(i * Grid.StepSizeX, j * Grid.StepSizeY),
                        ValuesGrid[i, j]);
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ServiceInfo", ServiceInfo, ServiceInfo.GetType());
            info.AddValue("Grid", Grid, Grid.GetType());
            for (int i = 0; i < Grid.NodesNumberX; ++i)
            {
                for (int j = 0; j < Grid.NodesNumberY; ++j)
                {
                    info.AddValue($"ValuesGrid_{i}{j}_X", ValuesGrid[i, j].X);
                    info.AddValue($"ValuesGrid_{i}{j}_Y", ValuesGrid[i, j].Y);
                }
            }
        }

        public V5DataOnGrid(SerializationInfo info, StreamingContext context) :
           base("Default service info", new DateTime(1970, 1, 1))
        {
            ServiceInfo = (string)info.GetValue("ServiceInfo", ServiceInfo.GetType());
            Grid = (Grid2D)info.GetValue("Grid", Grid.GetType());
            ValuesGrid = new Vector2[Grid.NodesNumberX, Grid.NodesNumberY];

            for (int i = 0; i < Grid.NodesNumberX; ++i)
            {
                for (int j = 0; j < Grid.NodesNumberY; ++j)
                {
                    float x = info.GetSingle($"ValuesGrid_{i}{j}_X");
                    float y = info.GetSingle($"ValuesGrid_{i}{j}_Y");
                    ValuesGrid[i, j] = new Vector2(x, y);
                }
            }
        }

    }
}
