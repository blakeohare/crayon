using System;
using System.Collections.Generic;

namespace Common
{
    public static class PerformanceTimer
    {
        internal static Stack<PerfRecord> Cursor = new Stack<PerfRecord>();
        private static PerfRecord RootRecord = null;

        public static string GetSummary()
        {
            if (Cursor.Count != 0) throw new Exception(); // invalid state? more or less perf markers pushed than popped.

            System.Text.StringBuilder buffer = new System.Text.StringBuilder();
            RootRecord.GetSummary("", RootRecord.Milliseconds, buffer);
            return buffer.ToString().Trim();
        }

        private static string FormatTime(double seconds)
        {
            return ((seconds * 100000) / 100.0) + "ms";
        }

        internal class PerfRecord
        {
            private double startMilliseconds;

            public string Name { get; set; }
            public double Milliseconds { get; set; }
            public List<PerfRecord> SubRecords { get; private set; }
            public int Aggregations { get; set; }

            public PerfRecord(string name)
            {
                this.Aggregations = 0;
                if (PerformanceTimer.RootRecord == null)
                {
                    PerformanceTimer.RootRecord = this;
                }
                else
                {
                    PerformanceTimer.Cursor.Peek().SubRecords.Add(this);
                }
                PerformanceTimer.Cursor.Push(this);

                this.Name = name;
                this.SubRecords = new List<PerfRecord>();
                this.startMilliseconds = CommonUtil.DateTime.Time.UnixTimeNowMillis;
            }

            public void Close(bool isAggregated)
            {
                PerfRecord shouldBeThis = PerformanceTimer.Cursor.Pop();

                if (this != shouldBeThis) throw new Exception(); // invalid state

                this.Milliseconds = CommonUtil.DateTime.Time.UnixTimeNowMillis - this.startMilliseconds;

                // not interested in sub sections. Just want to see what all the direct children of identical names add up to.
                if (isAggregated)
                {
                    Dictionary<string, PerfRecord> nodes = new Dictionary<string, PerfRecord>();
                    PerfRecord primeEntry;
                    for (int i = 0; i < this.SubRecords.Count; ++i)
                    {
                        PerfRecord subrecord = this.SubRecords[i];
                        if (nodes.ContainsKey(subrecord.Name))
                        {
                            primeEntry = nodes[subrecord.Name];
                            primeEntry.Milliseconds += this.SubRecords[i].Milliseconds;
                            primeEntry.Aggregations++;

                            this.SubRecords.RemoveAt(i--);
                        }
                        else
                        {
                            nodes[subrecord.Name] = subrecord;
                            subrecord.Aggregations = 1;
                            subrecord.SubRecords.Clear();
                        }
                    }
                }
            }

            public void GetSummary(string tabs, double total, System.Text.StringBuilder buffer)
            {
                buffer.Append(tabs);
                buffer.Append(this.Name);
                double millis = ((int)this.Milliseconds * 100) / 100.0;
                double percentage = ((int)(10000 * this.Milliseconds / total)) / 100.0;
                buffer.Append(" ");
                buffer.Append(millis);
                buffer.Append(" ms (");
                buffer.Append(percentage);
                buffer.Append("%)");
                if (this.Aggregations > 0)
                {
                    buffer.Append(" Aggregations: ");
                    buffer.Append(this.Aggregations);
                }
                buffer.Append('\n');
                foreach (PerfRecord pr in this.SubRecords)
                {
                    pr.GetSummary(tabs + "  ", total, buffer);
                }
            }
        }
    }

    public class PerformanceSection : IDisposable
    {
        public bool aggregated = false;

        public PerformanceSection(string name, bool aggregated)
        {
            PerformanceTimer.Cursor.Push(new PerformanceTimer.PerfRecord(name));
            this.aggregated = aggregated;
        }

        public PerformanceSection(string name) : this(name, false)
        { }

        public void Dispose()
        {
            PerformanceTimer.Cursor.Pop().Close(this.aggregated);
        }
    }
}
