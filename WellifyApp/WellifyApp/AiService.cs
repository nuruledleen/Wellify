using System.Text;
using System.Text.Json;

public class AiService
{
    private const string ApiKey = "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";
    private const string ApiUrl = "https://api.openai.com/v1/responses";

    private static readonly HttpClient client = new HttpClient();

    public async Task<string> GetWellnessInsight(
        double score, string sleep, string water,
        string exercise, string meal, string mood)
    {
        // ✅ Check empty input
        if (score == 0 &&
            string.IsNullOrWhiteSpace(sleep) &&
            string.IsNullOrWhiteSpace(water) &&
            string.IsNullOrWhiteSpace(exercise) &&
            string.IsNullOrWhiteSpace(meal) &&
            string.IsNullOrWhiteSpace(mood))
        {
            return "Please fill in all the data.";
        }

        string prompt =
            $"You are a friendly wellness coach. " +
            $"Health score: {score}%. Sleep: {sleep}h, Water: {water}L, Exercise: {exercise}m, Meals: {meal}, Mood: {mood}/5. " +
            $"Give 2 encouraging sentences.";

        var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);

        request.Headers.Add("Authorization", $"Bearer {ApiKey}");

        var body = new
        {
            model = "gpt-4.1-mini", // fast + cheaper
            input = prompt
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await client.SendAsync(request);
            var raw = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"STATUS: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"RAW RESPONSE: {raw}");

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(raw);

                return doc.RootElement
                    .GetProperty("output")[0]
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString()
                    ?? GenerateFallbackInsight(score);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OpenAI Error: {ex.Message}");
        }

        return GenerateFallbackInsight(score);
    }

    // 🔥 fallback (same as yours)
    private string GenerateFallbackInsight(double score)
    {
        var random = new Random();

        string[] high =
        {
            "Excellent work! Your healthy habits are really paying off.",
            "Amazing progress! Keep maintaining these strong routines.",
            "You're doing fantastic—your consistency is showing results."
        };

        string[] mid =
        {
            "You're doing well. Keep improving your daily habits!",
            "Nice effort so far—small improvements will take you further.",
            "You're on the right track, just stay consistent!"
        };

        string[] low =
        {
            "Try to improve your routine—small steps can make a big difference.",
            "Start small and stay consistent. Your health will improve over time.",
            "Focus on building better habits gradually—you can do it!"
        };

        if (score >= 80)
            return high[random.Next(high.Length)];

        if (score >= 50)
            return mid[random.Next(mid.Length)];

        return low[random.Next(low.Length)];
    }
}