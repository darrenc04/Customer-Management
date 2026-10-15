using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiApp1_Assignment1_.Services
{
    public class ApiErrorReader
    {
        public const string NetworkMessage = "Cannot reach the server. Check that the API is running and that the address in ApiSetting.cs is correct.";

        public const string TimeoutMessage = "The server took too long to respond. Please try again.";

        public static async Task<string> ReadAsync(HttpResponseMessage response)
        {
            try
            {
                string body = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(body))
                {
                    using JsonDocument document = JsonDocument.Parse(body);
                    JsonElement root = document.RootElement;

                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        //Validation errors look like: {"errors": {"Username": ["message", ...]}}
                        if (root.TryGetProperty("errors", out JsonElement errors) && errors.ValueKind == JsonValueKind.Object)
                        {
                            var messages = new List<string>();
                            foreach (JsonProperty field in errors.EnumerateObject())
                            {
                                if (field.Value.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (JsonElement message in field.Value.EnumerateArray())
                                    {
                                        string? text = message.GetString();
                                        if (!string.IsNullOrWhiteSpace(text))
                                            messages.Add(text);
                                    }
                                }

                                if (messages.Count > 0)
                                    return string.Join(Environment.NewLine, messages);
                            }


                            // Other errors look like: { "title": "...", "detail": "..." }
                            if (root.TryGetProperty("detail", out JsonElement detail) && detail.ValueKind == JsonValueKind.String)
                                return detail.GetString()!;

                            if (root.TryGetProperty("title", out JsonElement title) && title.ValueKind == JsonValueKind.String)
                                return title.GetString()!;
                        }
                    }
                }
            }
            catch (JsonException)
            {
                /* Ignore JSON parsing errors */
            }
            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Your session has expired. Please login again.",
                HttpStatusCode.NotFound => "That item was not found.",
                HttpStatusCode.Conflict => "That username is already taken.",
                _ => $"The request failed ({(int)response.StatusCode})."
            };
            }
        }
    }

