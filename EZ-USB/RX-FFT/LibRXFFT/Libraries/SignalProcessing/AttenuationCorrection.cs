﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Collections;

namespace LibRXFFT.Libraries.SignalProcessing
{
    public class CorrectionProfilePoint
    {
        public long Frequency;
        public double CorrectionOffset;

        public CorrectionProfilePoint()
        {
        }

        public CorrectionProfilePoint(long frequency, double correctionOffset)
        {
            Frequency = frequency;
            CorrectionOffset = correctionOffset;
        }
    }

    public class CorrectionProfile
    {
        public LinkedList<CorrectionProfilePoint> ProfilePoints;

        public CorrectionProfile()
        {
            ProfilePoints = new LinkedList<CorrectionProfilePoint>();
            /*
            ProfilePoints.AddLast(new CorrectionProfilePoint(0, -50.0f));
            ProfilePoints.AddLast(new CorrectionProfilePoint(10000000, 50.0f));
            */
        }

        public CorrectionProfile(string file)
        {
            Load(file);
        }

        public CorrectionProfile(LinkedList<CorrectionProfilePoint> profilePoints)
        {
            /* first sort the list by frequency */
            List<CorrectionProfilePoint> list = new List<CorrectionProfilePoint>(profilePoints);

            list.Sort(delegate(CorrectionProfilePoint p1, CorrectionProfilePoint p2)
            {
                return (int)(p1.Frequency - p2.Frequency);
            });

            ProfilePoints = new LinkedList<CorrectionProfilePoint>(list);
        }

        public CorrectionProfile(CorrectionProfile profile1, CorrectionProfile profile2)
        {
            ArrayList newList = new ArrayList();

            /* first sort the list by frequency */
            List<CorrectionProfilePoint> list1 = new List<CorrectionProfilePoint>(profile1.ProfilePoints);
            List<CorrectionProfilePoint> list2 = new List<CorrectionProfilePoint>(profile2.ProfilePoints);

            list1.Sort(delegate(CorrectionProfilePoint p1, CorrectionProfilePoint p2)
            {
                return (int)(p1.Frequency - p2.Frequency);
            });
            list2.Sort(delegate(CorrectionProfilePoint p1, CorrectionProfilePoint p2)
            {
                return (int)(p1.Frequency - p2.Frequency);
            });

            int pos1 = 0;
            int pos2 = 0;

            while ((pos1 < list1.Count) || (pos2 < list2.Count))
            {
                if (pos1 >= list1.Count)
                {
                    newList.Add(new CorrectionProfilePoint(list2[pos2].Frequency, list2[pos2].CorrectionOffset));
                    pos2++;
                }
                else if (pos2 >= list2.Count)
                {
                    newList.Add(new CorrectionProfilePoint(list1[pos1].Frequency, list1[pos1].CorrectionOffset));
                    pos1++;
                }
                else if (list1[pos1].Frequency == list2[pos2].Frequency)
                {
                    newList.Add(new CorrectionProfilePoint(list1[pos1].Frequency, (list1[pos1].CorrectionOffset + list2[pos2].CorrectionOffset) / 2));
                    pos1++;
                    pos2++;
                }
                else if (list1[pos1].Frequency < list2[pos2].Frequency)
                {
                    newList.Add(new CorrectionProfilePoint(list1[pos1].Frequency, list1[pos1].CorrectionOffset));
                    pos1++;
                }
                else if (list1[pos1].Frequency > list2[pos2].Frequency)
                {
                    newList.Add(new CorrectionProfilePoint(list2[pos2].Frequency, list2[pos2].CorrectionOffset));
                    pos2++;
                }
            }

            LinkedList<CorrectionProfilePoint> profilePoints = new LinkedList<CorrectionProfilePoint>((CorrectionProfilePoint[])newList.ToArray(typeof(CorrectionProfilePoint)));

            ProfilePoints = new LinkedList<CorrectionProfilePoint>(profilePoints);
        }

        public bool Empty
        {
            get
            {
                return ProfilePoints.Count == 0;
            }
        }

        public void Add(CorrectionProfilePoint point)
        {
            CorrectionProfilePoint p = GetPredecessor(point.Frequency);

            if (p != null)
            {
                ProfilePoints.AddAfter(ProfilePoints.Find(p), point);
            }
            else
            {
                ProfilePoints.AddLast(point);
            }
        }

        public CorrectionProfilePoint GetPredecessor(long freq)
        {
            CorrectionProfilePoint pred = null;

            foreach (CorrectionProfilePoint point in ProfilePoints)
            {
                /* this is the first entry */
                if (point.Frequency >= freq)
                {
                    /* we are already beyond the point that was looked for */
                    return pred;
                }

                pred = point;
            }

            return pred;
        }

        public CorrectionProfilePoint GetSuccessor(long freq)
        {
            CorrectionProfilePoint last = null;

            foreach (CorrectionProfilePoint point in ProfilePoints)
            {
                last = point;

                if (last.Frequency >= freq)
                {
                    /* we are now beyond the point that was looked for */
                    return last;
                }
            }

            /* else return the last one found */
            return last;
        }

        public double GetCorrectionValue(long freq)
        {
            CorrectionProfilePoint p1 = GetPredecessor(freq);
            CorrectionProfilePoint p2 = GetSuccessor(freq);

            /* no correction values found */
            if (p1 == null && p2 == null)
            {
                return 0.0f;
            }

            /* freq is lower than lowest found correction point */
            if (p1 != null && p2 == null)
            {
                return p1.CorrectionOffset;
            }

            /* freq is beyond last correction point */
            if (p1 == null && p2 != null)
            {
                return p2.CorrectionOffset;
            }

            /* freq must be between p1 and p2 freq */
            double diff1 = p2.Frequency - p1.Frequency;
            double diff2 = p2.Frequency - freq;

            /* reached end of list */
            if (diff1 < 1)
            {
                return p1.CorrectionOffset;
            }

            double corr1 = diff2 / diff1;
            double corr2 = 1 - diff2 / diff1;

            return p1.CorrectionOffset * corr1 + p2.CorrectionOffset * corr2;
        }


        public void Load(string file)
        {
            TextReader reader = new StreamReader(file);
            XmlSerializer serializer = new XmlSerializer(typeof(CorrectionProfilePoint[]));
            CorrectionProfilePoint[] points = (CorrectionProfilePoint[])serializer.Deserialize(reader);

            if (points != null)
            {
                ProfilePoints = new LinkedList<CorrectionProfilePoint>();
                foreach (CorrectionProfilePoint point in points)
                {
                    ProfilePoints.AddLast(point);
                }
            }
        }

        public void Save(string file)
        {
            TextWriter writer = new StreamWriter(file);
            CorrectionProfilePoint[] points = ProfilePoints.ToArray<CorrectionProfilePoint>();
            XmlSerializer serializer = new XmlSerializer(typeof(CorrectionProfilePoint[]));

            serializer.Serialize(writer, points);
            writer.Close();
        }
    }

    public class AttenuationCorrection
    {
        private CorrectionProfile Profile;
        private double[] CorrectionTable;

        public AttenuationCorrection()
        {
            Profile = new CorrectionProfile();
        }

        public AttenuationCorrection(string fileName)
        {
            Profile = new CorrectionProfile(fileName);
        }

        public AttenuationCorrection(CorrectionProfile profile)
        {
            Profile = profile;
        }

        public CorrectionProfile GetProfile()
        {
            return Profile;
        }

        public bool Empty
        {
            get
            {
                return (Profile == null) || Profile.Empty;
            }
        }


        public void BuildCorrectionTable(long startFreq, long endFreq, long steps)
        {
            double freqPerStep = (double)(endFreq - startFreq) / steps;
            CorrectionTable = new double[steps];

            if (Profile == null)
            {
                return;
            }

            for (long step = 0; step < steps; step++)
            {
                long freq = (long)Math.Min(startFreq + (freqPerStep * step), endFreq);

                CorrectionTable[step] = Profile.GetCorrectionValue(freq);
            }
        }

        public void ApplyCorrectionTable(double[] strengths)
        {
            if (CorrectionTable == null || strengths.Length != CorrectionTable.Length)
            {
                return;
            }

            for (int pos = 0; pos < CorrectionTable.Length; pos++)
            {
                strengths[pos] += CorrectionTable[pos];
            }
        }
    }
}
