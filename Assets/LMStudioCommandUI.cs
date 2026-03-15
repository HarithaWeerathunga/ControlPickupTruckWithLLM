using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class LMStudioCommandUI : MonoBehaviour
{
    public TMP_InputField commandInput;
    public TMP_Text responseText;
    public TMP_Text chatLogText;
    public PickupAIController pickupController;

    public string endpoint = "http://127.0.0.1:1234/v1/chat/completions";
    public string model = "google/gemma-3-4b";

    private readonly List<string> chatHistory = new List<string>();

    public void SendCommand()
    {
        string prompt = commandInput.text.Trim();
        if (string.IsNullOrEmpty(prompt)) return;

        AddToChat("You", prompt);
        commandInput.text = "";

        StartCoroutine(SendCommandCoroutine(prompt));
    }

    private IEnumerator SendCommandCoroutine(string userInput)
    {
        responseText.text = "Sending...";

        ChatRequest payload = BuildRequest(userInput);
        string json = JsonUtility.ToJson(payload);

        using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 300;
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                responseText.text = "Request failed: " + request.error + "\nURL: " + endpoint;
                AddToChat("Assistant", "I couldn't reach the model server.");
                yield break;
            }

            string raw = request.downloadHandler.text;
            string assistantText = ExtractStreamedContent(raw).Trim();

            string cleaned = CleanupModelOutput(assistantText);

            ChatCommandSequence parsed = null;

            try
            {
                parsed = JsonUtility.FromJson<ChatCommandSequence>(cleaned);
            }
            catch
            {
                responseText.text = "Invalid JSON";
                AddToChat("Assistant", "I returned invalid JSON. Please try again.");
                yield break;
            }

            if (parsed == null)
            {
                responseText.text = "Invalid response";
                AddToChat("Assistant", "I returned an empty response. Please try again.");
                yield break;
            }

            string reply = string.IsNullOrWhiteSpace(parsed.reply)
                ? "Done. Where should I go next?"
                : parsed.reply;

            responseText.text = reply;
            AddToChat("Assistant", reply);

            if (parsed.commands != null && parsed.commands.Length > 0)
            {
                pickupController.ExecuteCommands(parsed.commands);
            }
        }
    }

    private void AddToChat(string speaker, string message)
    {
        chatHistory.Add($"{speaker}: {message}");

        if (chatHistory.Count > 4)
            chatHistory.RemoveAt(0);

        if (chatLogText != null)
            chatLogText.text = string.Join("\n\n", chatHistory);
    }

    private ChatRequest BuildRequest(string userInput)
    {
        string systemPrompt =
            "You control a pickup truck in a Unity game. " +
            "Always respond with JSON only. No markdown. No explanation outside JSON. " +
            "Use this exact format: " +
            "{\"reply\":\"text\",\"commands\":[{\"action\":\"move\",\"direction\":\"forward\",\"distance\":number},{\"action\":\"turn\",\"direction\":\"left\",\"degrees\":number}]} " +
            "or with backward/right variations. " +
            "Allowed actions are move, turn, stop. " +
            "For 'go forward and turn left', return two commands: first move forward, then turn left. " +
            "For 'go backward and take a right turn', return two commands: first move backward, then turn right. " +
            "If no distance is given, use 2. " +
            "If no degrees are given for a turn, use 90. " +
            "The reply should be conversational and ask where to go next.";

        return new ChatRequest
        {
            model = model,
            stream = true,
            messages = new List<ChatRequestMessage>
            {
                new ChatRequestMessage { role = "system", content = systemPrompt },
                new ChatRequestMessage { role = "user", content = userInput }
            }
        };
    }

    private string ExtractStreamedContent(string rawResponse)
    {
        StringBuilder builder = new StringBuilder();
        string[] lines = rawResponse.Split('\n');

        foreach (string line in lines)
        {
            string trimmed = line.Trim();

            if (!trimmed.StartsWith("data: "))
                continue;

            string dataString = trimmed.Substring(6);

            if (dataString == "[DONE]")
                break;

            try
            {
                StreamChunk chunk = JsonUtility.FromJson<StreamChunk>(dataString);

                if (chunk != null &&
                    chunk.choices != null &&
                    chunk.choices.Length > 0 &&
                    chunk.choices[0].delta != null &&
                    !string.IsNullOrEmpty(chunk.choices[0].delta.content))
                {
                    builder.Append(chunk.choices[0].delta.content);
                }
            }
            catch
            {
            }
        }

        return builder.ToString();
    }

    private string CleanupModelOutput(string text)
    {
        string cleaned = text.Trim();
        cleaned = cleaned.Replace("```json", "");
        cleaned = cleaned.Replace("```", "");

        int firstBrace = cleaned.IndexOf('{');
        int lastBrace = cleaned.LastIndexOf('}');

        if (firstBrace >= 0 && lastBrace > firstBrace)
            cleaned = cleaned.Substring(firstBrace, lastBrace - firstBrace + 1);

        return cleaned.Trim();
    }
}

[Serializable]
public class ChatRequest
{
    public string model;
    public List<ChatRequestMessage> messages;
    public bool stream;
}

[Serializable]
public class ChatRequestMessage
{
    public string role;
    public string content;
}

[Serializable]
public class StreamChunk
{
    public Choice[] choices;
}

[Serializable]
public class Choice
{
    public Delta delta;
    public string finish_reason;
}

[Serializable]
public class Delta
{
    public string role;
    public string content;
}