using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Components.DirectX;
using System.IO;
using System.Xml.Serialization;

namespace RX_FFT
{
    public class FrequencyMarkerList
    {
        public LinkedList<FrequencyMarker> Markers = new LinkedList<FrequencyMarker>();
        public event EventHandler MarkersChanged;

        public FrequencyMarkerList()
        {
            try
            {
                Load("markers.xml");
            }
            catch (Exception)
            {
            }
        }

        public void Load(string file)
        {
            TextReader reader = new StreamReader(file);
            XmlSerializer serializer = new XmlSerializer(typeof(FrequencyMarker[]));
            FrequencyMarker[] markers = (FrequencyMarker[])serializer.Deserialize(reader);

            if (markers != null)
            {
                Markers.Clear();
                foreach (FrequencyMarker marker in markers)
                {
                    Markers.AddLast(marker);
                }

                if (MarkersChanged != null)
                {
                    MarkersChanged(null, null);
                }
            }
        }

        public void Save(string file)
        {
            TextWriter writer = new StreamWriter(file);
            FrequencyMarker[] markers = Markers.ToArray<FrequencyMarker>();
            XmlSerializer serializer = new XmlSerializer(typeof(FrequencyMarker[]));

            serializer.Serialize(writer, markers);
            writer.Close();
        }

        public void Clear()
        {
            Markers.Clear();
            Notify();
        }

        public void Add(FrequencyMarker marker)
        {
            Markers.AddLast(marker);
            Notify();
        }

        public void Remove(FrequencyMarker marker)
        {
            Markers.Remove(marker);
            Notify();
        }

        private void Notify()
        {
            Save("markers.xml");
            if (MarkersChanged != null)
            {
                MarkersChanged(null, null);
            }
        }
    }
}
