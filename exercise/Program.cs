using System.Text.Json;
using programming_exercise;

try
{
    const string path = "../../../trainings.txt";
    var jsonContent = File.ReadAllText(path);
    var persons = JsonSerializer.Deserialize<Person[]>(jsonContent);

    #region Part1

    var completionsData = GetNumberOfCompletions(persons);
    WriteJsonFile(completionsData, "../../../numberOfCompletions.json");

    #endregion

    #region Part2

    var completedTraining = GetCompletedTrainings(persons);
    WriteJsonFile(completedTraining, "../../../completedTrainings.json");

    #endregion

    #region Part3

    var expiredOrWillExpire = GetExpiredOrWillExpireTrainings(persons);
    WriteJsonFile(expiredOrWillExpire, "../../../expiredOrWillExpireTrainings.json");

    #endregion
}
catch (Exception e)
{
    Console.WriteLine("Exception: " + e.Message);
}

Dictionary<string, int> GetNumberOfCompletions(Person[] persons)
{
    var data = new Dictionary<string, int>();

    foreach (var person in persons)
    {
        var completionSet = new HashSet<string>();

        foreach (var completion in person.completions)
        {
            var completionName = completion.name;
            if (!completionSet.Contains(completionName))
            {
                completionSet.Add(completionName);
                if (data.ContainsKey(completionName))
                {
                    data[completionName] += 1;
                }
                else
                {
                    data[completionName] = 1;
                }
            }
        }
    }

    return data;
}

Dictionary<string, List<string>> GetCompletedTrainings(Person[] persons)
{
    var data = new Dictionary<string, List<string>>();
    var fiscalYearStart = new DateTime(2023, 7, 1);
    var fiscalYearEnd = new DateTime(2024, 6, 30);

    var specifiedTrainings = new List<string>
    {
        "Electrical Safety for Labs",
        "X-Ray Safety",
        "Laboratory Safety Training"
    };

    foreach (var person in persons)
    {
        foreach (var completion in person.completions)
        {
            DateTime.TryParse(completion.timestamp, out var dateTime);

            if (specifiedTrainings.Contains(completion.name) && dateTime >= fiscalYearStart &&
                dateTime <= fiscalYearEnd)
            {
                if (!data.ContainsKey(completion.name))
                {
                    data[completion.name] = new List<string>();
                }

                data[completion.name].Add(person.name);
            }
        }
    }

    return data;
}

Dictionary<string, List<Dictionary<string, object>>> GetExpiredOrWillExpireTrainings(Person[] persons)
{
    var data = new Dictionary<string, List<Dictionary<string, object>>>();
    var specifiedDate = new DateTime(2023, 10, 1);

    foreach (var person in persons)
    {
        var completedTrainings = new Dictionary<string, Dictionary<string, string>>();

        foreach (var completion in person.completions.OrderByDescending(c =>
                     DateTime.TryParse(c.timestamp, out var date) ? date : DateTime.MinValue))
        {
            DateTime.TryParse(completion.timestamp, out var completionDate);
            DateTime.TryParse(completion.expires, out var expirationDate);

            if (completedTrainings.ContainsKey(completion.name))
            {
                continue;
            }

            if (expirationDate > completionDate && expirationDate < specifiedDate)
            {
                completedTrainings[completion.name] = new Dictionary<string, string>
                    { { "expiredOrWillExpire", "expired" } };
            }
            else if (expirationDate > completionDate && expirationDate >= specifiedDate &&
                     expirationDate <= specifiedDate.AddMonths(1))
            {
                completedTrainings[completion.name] = new Dictionary<string, string>
                    { { "expiredOrWillExpire", "expires soon" } };
            }
        }

        var personTrainings = completedTrainings.Select(trainingEntry => new Dictionary<string, object>
            { { trainingEntry.Key, trainingEntry.Value } }).ToList();

        if (personTrainings.Count > 0)
        {
            data[person.name] = personTrainings;
        }
    }

    return data;
}

void WriteJsonFile<T>(T data, string path)
{
    var jsonString = JsonSerializer.Serialize(data);
    File.WriteAllText(path, jsonString);
    Console.WriteLine($"JSON data has been written to {path}");
}