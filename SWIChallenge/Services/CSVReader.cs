using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SWIChallenge.Extensions;

namespace SWIChallenge.Services
{
    public class CSVReader
    {
        public IList<(double, double)> Points { get; private set; }

        public async Task<bool> ReadAsync(string file)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException(nameof(file));

            if (!File.Exists(file))
                return false;

            Points = new List<(double, double)>();

            try
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync();
                        if (line.IndexOf(',') != -1) {
                            var (x, y) = line.Split(',');

                            if (double.TryParse(x, out var dx) && double.TryParse(y, out var dy))
                            {
                                Points.Add((dx, dy));
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); //TODO: Log on file?
                return false;
            }

            return true;
        }
    }
}
