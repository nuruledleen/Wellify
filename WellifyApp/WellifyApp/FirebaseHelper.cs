using System.Globalization;
using System.Net.Http.Json;

namespace WellifyApp;

public class FirebaseHelper
{
    private readonly HttpClient _httpClient = new HttpClient();
    private const string BaseUrl = "https://wellifyapp-ae600-default-rtdb.asia-southeast1.firebasedatabase.app/";

    public async Task SaveRecord(WellnessRecord record)
    {
        // Using the DateKey ensures we update the same record if saved multiple times a day
        var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}Wellness/{record.DateKey}.json", record);

        if (!response.IsSuccessStatusCode)
            throw new Exception("Failed to sync with Firebase.");
    }

    // Inside FirebaseHelper class
    // Fetch a single record for the DailyPage
    public async Task<WellnessRecord?> GetRecord(string dateKey)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}Wellness/{dateKey}.json");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // Firebase returns the string "null" (not an empty object) if the key doesn't exist
                if (string.IsNullOrEmpty(content) || content.Trim() == "null")
                {
                    return null;
                }

                return System.Text.Json.JsonSerializer.Deserialize<WellnessRecord>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Firebase Error: {ex.Message}");
            return null;
        }
    }

    // Fetch all records for the RecordPage history list
    public async Task<List<WellnessRecord>> GetAllRecords()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<Dictionary<string, WellnessRecord>>($"{BaseUrl}Wellness.json");

            if (result == null) return new List<WellnessRecord>();

            // Map the dictionary key (the date) back into the object's DateKey property
            var list = result.Select(item =>
            {
                item.Value.DateKey = item.Key; // Ensure DateKey is populated
                return item.Value;
            }).ToList();

            return list;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            return new List<WellnessRecord>();
        }
    }

    public async Task<List<WellnessRecord>> GetWeeklyRecords(DateTime startOfWeek)
    {
        var weeklyData = new List<WellnessRecord>();
        for (int i = 0; i < 7; i++)
        {
            string dateKey = startOfWeek.AddDays(i).ToString("dd-MM-yyyy");
            var record = await GetRecord(dateKey);
            if (record != null) weeklyData.Add(record);
        }
        return weeklyData;
    }

    public async Task CleanupOldData()
    {
        try
        {
            var allRecords = await GetAllRecords();
            var oneYearAgo = DateTime.Now.AddYears(-1);

            foreach (var record in allRecords)
            {
                if (DateTime.TryParseExact(record.DateKey, "dd-MM-yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime recordDate))
                {
                    if (recordDate < oneYearAgo)
                    {
                        await _httpClient.DeleteAsync($"{BaseUrl}Wellness/{record.DateKey}.json");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cleanup Error: {ex.Message}");
        }
    }
}