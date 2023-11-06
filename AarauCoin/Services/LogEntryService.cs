using AarauCoin.Models;

namespace AarauCoin.Services
{
    public class LogEntryService
    {

        #region Log

        /// <summary>
        /// Methode welche die Log Datei durch eine Methode parsed und in eine Liste von LogViewModels umwandelt, und zuletzt zurückgibt
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">Wird geworfen wenn File nicht gefunden wird</exception>
        public async Task<string> ReadLog(string date)
        {
            string fileName = $"../logs/webapi-{date}.log";

            if (!File.Exists(fileName))
                throw new FileNotFoundException("Datei nicht gefunden", fileName);

            string fileContent = string.Empty;
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                StreamReader streamReader = new StreamReader(fileStream);
                fileContent = await streamReader.ReadToEndAsync();
            }
            return fileContent;
        }

        /// <summary>
        /// Methode welche die logs aus der ReadLog Methode in eine Liste von LogViewModels umwandelt und parsed
        /// </summary>
        /// <param name="fileContent"></param>
        /// <returns></returns>
        public List<LogViewModel> ParseLog(string fileContent)
        {
            List<LogViewModel> list = new();

            foreach (var line in fileContent.Split("\n"))
            {
                var data = line.Split("]");

                if (data.Length < 2)
                    continue;

                string datum = data[0].Remove(0, 1).Remove(31, 4);
                string message = data[1];
                string type = data[0].Remove(0, 33);

                if (DateTimeOffset.TryParse(datum, out DateTimeOffset parsedDatum))
                {
                    var log = new LogViewModel() { LogDate = parsedDatum, TypeOfLog = type, LogMessage = message };
                    list.Add(log);
                }
                else
                {
                    continue;
                }
            }

            return list;
        }

        #endregion Log

    }
}
