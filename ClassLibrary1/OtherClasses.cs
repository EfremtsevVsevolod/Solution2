using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.Serialization;

namespace ClassLibrary1
{
    [Serializable]
    public struct Grid2D
    {
        public int NodesNumberX { get; set; }
        public int NodesNumberY { get; set; }
        public float StepSizeX { get; set; }
        public float StepSizeY { get; set; }
        public Grid2D(int nodesNumberX, int nodesNumberY, float stepSizeX, float stepSizeY)
        {
            NodesNumberX = nodesNumberX;
            NodesNumberY = nodesNumberY;
            StepSizeX = stepSizeX;
            StepSizeY = stepSizeY;
        }
        public override string ToString()
        {
            return $"NodesNumberX:{NodesNumberX} NodesNumberY: {NodesNumberY}\n" +
                $"StepSizeX: {StepSizeX} StepSizeY: {StepSizeY}";
        }
        public string ToString(string format)
        {
            return $"NodesNumberX: {NodesNumberX} NodesNumberY: {NodesNumberY}\n" +
                $"StepSizeX: {StepSizeX.ToString(format)}" +
                $"StepSizeY: {StepSizeY.ToString(format)}";
        }
    }

    [Serializable]
    public struct DataItem : ISerializable
    {
        public Vector2 PointCoord { get; set; }
        public Vector2 FieldValue { get; set; }

        public DataItem(Vector2 pointCoord, Vector2 fieldValue)
        {
            PointCoord = pointCoord;
            FieldValue = fieldValue;
        }
        public override string ToString()
        {
            return $"PointCoord={PointCoord}  FieldValue={FieldValue}";
        }
        public string ToString(string format)
        {
            return $"PointCoord={PointCoord.ToString(format)}  " +
                $"FieldValue={FieldValue.ToString(format)}";
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("PointCoord_X", PointCoord.X);
            info.AddValue("PointCoord_Y", PointCoord.Y);
            info.AddValue("FieldValue_X", FieldValue.X);
            info.AddValue("FieldValue_Y", FieldValue.Y);
        }

        public DataItem(SerializationInfo info, StreamingContext context)
        {
            float x = info.GetSingle("PointCoord_X");
            float y = info.GetSingle("PointCoord_Y");
            PointCoord = new Vector2(x, y);
            x = info.GetSingle("FieldValue_X");
            y = info.GetSingle("FieldValue_Y");
            FieldValue = new Vector2(x, y);
        }
    }

    [Serializable]
    public abstract class V5Data : IEnumerable<DataItem>, INotifyPropertyChanged
    {
        private string service_info;
        private DateTime measurement_time;

        public string ServiceInfo
        {
            get
            {
                return service_info;
            }
            set
            {
                service_info = value;
                OnPropertyChanged("ServiceInfo");
            }
        }
        public DateTime MeasurementTime
        {
            get
            {
                return measurement_time;
            }
            set
            {
                measurement_time = value;
                OnPropertyChanged("MeasurementTime");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public V5Data(string serviceInfo, DateTime measurementTime)
        {
            ServiceInfo = serviceInfo;
            MeasurementTime = measurementTime;
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public abstract Vector2[] NearEqual(float eps);

        public abstract string ToLongString();
        public abstract string ToLongString(string format);

        public override string ToString()
        {
            return $"ServiceInfo: {ServiceInfo}\nMeasurementTime: {MeasurementTime}";
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public abstract IEnumerator<DataItem> GetEnumerator();
    }

    public enum ChangeInfo
    {
        ItemChanged,
        Add,
        Remove,
        Replace
    }

    public class DataChangedEventArgs
    {
        public ChangeInfo ChangeInfoInstance { get; set; }
        public string InfoStr { get; set; }

        public DataChangedEventArgs(ChangeInfo changeInfoInstance, string infoSrt)
        {
            ChangeInfoInstance = changeInfoInstance;
            InfoStr = infoSrt;
        }

        public override string ToString()
        {
            return ChangeInfoInstance.ToString() + " " + InfoStr;
        }
    }
}
