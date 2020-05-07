﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EDLtoYT.Pages
{
    public class MarkerColour
    {
        public string Label { get; set; }
        public bool Checked { get; set; }
    }
    public partial class Index
    {
        //001  001      V     C        00:01:00:14 00:01:00:15 00:01:00:14 00:01:00:15  
        Regex TimePattern = new Regex(@"(?:[0-9]):[0-5][0-9]:[0-5][0-9].[0-9][0-9]", RegexOptions.Compiled);

        //|C:ResolveColorBlue |M:Whats Blockboard? |D:1
        Regex MarkerPattern = new Regex(@"\|C:([a-zA-Z]*) \|M:([a-zA-Z ?&]*) \|D:1", RegexOptions.Compiled);

        public string Input { get; set; }
        public string Output { get; set; }

        public List<Marker> Markers = new List<Marker>();

        public List<MarkerColour> Colours = new List<MarkerColour>();
        public void ConvertClick()
        {
            Markers.Clear();

            //Do we need to output "video start"?
            Markers.Add(new Marker() { Time = new TimeSpan(0), MarkerText = "Video Start" });

            using (var reader = new StringReader(Input))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Marker m = new Marker();
                    var timeMatch = TimePattern.Match(line);
                    if (!timeMatch.Success)
                        continue;

                    //Why not TimeSpan.Parse? It kept refusing to interpret it as hh:mm:ss:ms, moving everything up one.
                    var t = timeMatch.Captures[0].ToString().Split(':');
                    var ts = new TimeSpan(0, int.Parse(t[0]), int.Parse(t[1]), int.Parse(t[2]), int.Parse(t[3]));

                    m.Time = ts;

                    line = reader.ReadLine();
                    var markerMatch = MarkerPattern.Match(line);

                    if (!markerMatch.Success)
                        continue;

                    m.Colour = markerMatch.Groups[1].ToString();
                    m.MarkerText = markerMatch.Groups[2].ToString();

                    Markers.Add(m);
                }
            }

            Colours = Markers.Select(m => m.Colour).Distinct().Select(o =>
                        new MarkerColour
                        {
                            Checked = true,
                            Label = o
                        }).ToList();

            OutputText();
        }
        public void CheckboxClicked(MarkerColour c, object checkedValue)
        {
            c.Checked = !c.Checked;
            OutputText();

        }
        private void OutputText()
        {
            StringBuilder output = new StringBuilder();
            var x = Colours.Where(c => c.Checked).Select(c => c.Label).ToList();
            RenderMarkers(output, Markers.Where(m => x.Contains(m.Colour)));
            Output = output.ToString();
        }

        private StringBuilder RenderMarkers(StringBuilder sb, IEnumerable<Marker> _markers)
        {
            foreach (var m in _markers)
            {
                //Format is suitable for <1hr videos
                sb.AppendLine(string.Format("{0} - {1}", m.Time.ToString(@"mm\:ss"), m.MarkerText));
            }
            return sb;
        }
    }

}
