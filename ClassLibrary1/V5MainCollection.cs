using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ClassLibrary1
{
    public class V5MainCollection : IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        List<V5Data> LstData = new List<V5Data>();
        public int Count { get; set; } = 0;

        private bool PrivateAddChangesAfterSave = false;
        public bool AddChangesAfterSave
        {
            get
            {
                return PrivateAddChangesAfterSave;
            }
            set
            {
                PrivateAddChangesAfterSave = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Min"));
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public V5Data this[int index]
        {
            get
            {
                return LstData[index];
            }
            set
            {
                LstData[index] = value;
                LstData[index].PropertyChanged += DataChangedEvent;
                DataChanged(this,
                    new DataChangedEventArgs(ChangeInfo.Replace,
                    LstData[index].GetType().ToString() + " with " +
                    value.GetType().ToString()));
            }
        }

        public void Add(V5Data item)
        {
            LstData.Add(item);
            LstData[Count].PropertyChanged += DataChangedEvent;
            Count++;
            DataChanged(item,
                new DataChangedEventArgs(ChangeInfo.Add, item.GetType().ToString()));
            AddChangesAfterSave = true;
        }

        public bool Remove(string id, DateTime date)
        {
            foreach (var lstItem in LstData.FindAll(elem => elem.ServiceInfo == id &&
                                                    elem.MeasurementTime == date))
            {
                lstItem.PropertyChanged -= DataChangedEvent;
            }

            int removedCount = LstData.RemoveAll(elem => elem.ServiceInfo == id &&
                                              elem.MeasurementTime == date);
            Count -= removedCount;
            if (removedCount > 0)
            {
                AddChangesAfterSave = true;
            }
            return removedCount > 0;
        }
        public bool RemoveElement(V5Data select_elem)
        {
            bool delete_flg = LstData.Remove(select_elem);

            if (delete_flg)
            {
                --Count;
                AddChangesAfterSave = true;
            }

            return delete_flg;
        }


        public void AddOneDefaultGrid(Grid2D grid)
        {
            V5DataOnGrid v5DataOnGridDefaultInstance =
                    new V5DataOnGrid("Default service info", new DateTime(1970, 1, 1), grid);
            v5DataOnGridDefaultInstance.InitRandom(-100.0f, 100.0f);
            LstData.Add(v5DataOnGridDefaultInstance);
            AddChangesAfterSave = true;
        }

        public void AddOneCustomGrid(V5DataOnGrid grid)
        {
            grid.InitRandom(-100.0f, 100.0f);
            LstData.Add(grid);
            AddChangesAfterSave = true;
        }


        public void AddOneFileGrid(string filename)
        {
            try
            {
                V5DataOnGrid v5DataOnGridDefaultInstance = new V5DataOnGrid(filename);
                LstData.Add(v5DataOnGridDefaultInstance);
                AddChangesAfterSave = true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void AddOneDefaultColection(int nItems)
        {
            V5DataCollection v5DataCollectionDefaultInstance =
                new V5DataCollection("Default service info", new DateTime(1970, 1, 1));
            v5DataCollectionDefaultInstance.InitRandom(nItems, 10.0f, 10.0f, -100.0f, 100.0f);
            LstData.Add(v5DataCollectionDefaultInstance);
            AddChangesAfterSave = true;
        }

        public void AddPairDefaultGridAndCollection(Grid2D grid)
        {
            V5DataOnGrid v5DataOnGridDefaultInstance =
                    new V5DataOnGrid("Default info", new DateTime(1970, 1, 1), grid);
            v5DataOnGridDefaultInstance.InitRandom(-100.0f, 100.0f);
            LstData.Add(v5DataOnGridDefaultInstance);
            LstData.Add((V5DataCollection)v5DataOnGridDefaultInstance);
        }

        public void AddDefaults()
        {
            Grid2D smallGrid = new Grid2D(1, 1, 1.0f, 1.0f);
            Grid2D bigGrid = new Grid2D(2, 2, 1.0f, 1.0f);
            Grid2D emptyGrid = new Grid2D(0, 0, 1.0f, 1.0f);

            // Different nonempty
            AddOneDefaultColection(3);
            AddOneDefaultGrid(bigGrid);

            // The same nonempty
            AddPairDefaultGridAndCollection(smallGrid);

            // Empty
            AddOneDefaultColection(0);
            AddOneDefaultGrid(emptyGrid);

            AddChangesAfterSave = true;
        }

        public override string ToString()
        {
            StringBuilder strBulder = new StringBuilder();

            foreach (V5Data dataElem in LstData)
            {
                strBulder.Append(dataElem.ToString() + "\n\n");
            }

            return strBulder.ToString();
        }

        public string ToLongString(string format)
        {
            StringBuilder strBulder = new StringBuilder();

            foreach (V5Data dataElem in LstData)
            {
                strBulder.Append(dataElem.ToLongString(format) + "\n\n");
            }

            return strBulder.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)LstData).GetEnumerator();
        }

        public IEnumerator<V5Data> GetEnumerator()
        {
            return ((IEnumerable<V5Data>)LstData).GetEnumerator();
        }

        public float Min
        {
            get
            {
                if (LstData.Count() != 0)
                {
                    try
                    {
                        return (from lstElem in LstData
                                from dataItem in lstElem
                                select dataItem.FieldValue.Length()).Min();
                    }
                    catch (Exception)
                    {
                        return float.PositiveInfinity;
                    }
                }
                else
                {
                    return float.PositiveInfinity;
                }
            }
        }

        public IEnumerable<DataItem> IterDataItemsFromCollectionWithMin
        {
            get
            {
                float minLengh = Min;
                return from lstElem in LstData
                       from dataItem in lstElem
                       where dataItem.FieldValue.Length() == minLengh
                       select dataItem;
            }
        }

        public IEnumerable<Vector2> IterVector2Target
        {
            get
            {
                var iterV5DataOnGrid = from lstElem in LstData
                                       where lstElem.GetType() == typeof(V5DataOnGrid)
                                       from dataItem in lstElem
                                       select dataItem.PointCoord;

                var iterV5DataCollection = from lstElem in LstData
                                           where lstElem.GetType() == typeof(V5DataCollection)
                                           from dataItem in lstElem
                                           select dataItem.PointCoord;

                return iterV5DataOnGrid.Except(iterV5DataCollection).Distinct();
            }
        }

        public delegate void DataChangedEventHandler(object source, DataChangedEventArgs args);

        public event DataChangedEventHandler DataChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        void DataChangedEvent(object sender, PropertyChangedEventArgs args)
        {
            DataChanged(sender, new DataChangedEventArgs(ChangeInfo.ItemChanged,
                    sender.GetType().ToString()));
        }

        public void Save(string filename)
        {
            FileStream savefilestream = null;
            try
            {
                savefilestream = File.Create(filename);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(savefilestream, LstData);
                AddChangesAfterSave = false;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"File {filename} open error or save error with exception:" +
                    $"\n{exception.Message}");
            }
            finally
            {
                if (savefilestream != null)
                {
                    savefilestream.Close();
                }
            }
        }

        public void Load(string filename)
        {
            FileStream loadfilestream = null;

            try
            {
                loadfilestream = File.OpenRead(filename);
                BinaryFormatter formatter = new BinaryFormatter();
                LstData = formatter.Deserialize(loadfilestream) as List<V5Data>;
            }
            catch (Exception exception)
            {
                throw new Exception($"File {filename} open error or load error with exception", exception);
            }
            finally
            {
                if (loadfilestream != null)
                {
                    loadfilestream.Close();
                }
            }
        }

        public bool IsValidServiceInfo(string service_info)
        {
            foreach (var lstItem in LstData)
            {
                if (service_info == lstItem.ServiceInfo)
                {
                    return false;
                }
            }
            return true;
        }
    }
}