using EDLtoYT.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EDLtoYT.Pages
{
    public partial class Index
    {
        //|C:ResolveColorBlue |M:Whats Blockboard? |D:1
        private Regex MarkerPattern = new Regex(@"\|C:([a-zA-Z]*) \|M:([a-zA-Z0-9\\.\\,()\\#\\:\-\\! \\/?&]*) \|D:1", RegexOptions.Compiled);

        //001  001      V     C        00:01:00:14 00:01:00:15 00:01:00:14 00:01:00:15
        private Regex TimePattern = new Regex(@"(?:[0-9]):[0-5][0-9]:[0-5][0-9].[0-9][0-9]", RegexOptions.Compiled);

        public List<MarkerColour> Colours = new List<MarkerColour>();
        public List<Marker> Markers = new List<Marker>();
        public string Input { get; set; }
        public string Output { get; set; }
        public bool dummy {get; set;}

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

        public void DummyChecked()
        {
            dummy = !dummy;
        }

        public void CheckboxClicked(MarkerColour c, object checkedValue)
        {
            c.Checked = !c.Checked;
            OutputText();
        }

        public void ConvertClick()
        {
            Markers.Clear();

            //Do we need to output "video start"?
            if (dummy)
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

        public async Task ReadFile()
        {
            foreach (var file in await fileReaderService.CreateReference(inputElement).EnumerateFilesAsync())
            {
                // Read into buffer and act (uses less memory)
                await using (Stream stream = await file.OpenReadAsync())
                {
                    // Do (async) stuff with stream...
                    var sr = new StreamReader(stream);
                    var x = await sr.ReadToEndAsync();
                    Input = x;
                    if (!string.IsNullOrEmpty(Input))
                    {
                        ConvertClick();
                        break;
                    }
                }
            }
        }
    }
}