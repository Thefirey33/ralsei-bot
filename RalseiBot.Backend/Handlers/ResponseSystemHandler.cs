using System.Text.Json;

namespace ralsei_bot_discord.Handlers;

public class ResponseSystemHandler
{
    /// <summary>
    ///     These specify the responses that Ralsei can give.
    ///     Some of them are for moderation reasons, others are responses to other people.
    /// </summary>
    public enum ResponseTypes
    {
        /// <summary>
        ///     When the channel is locked.
        /// </summary>
        LockChannel,

        /// <summary>
        ///     When the channel is unlocked.
        /// </summary>
        UnlockChannel,

        /// <summary>
        ///     When Ralsei is petted.
        /// </summary>
        PetPet,

        /// <summary>
        ///     When Ralsei is booped.
        /// </summary>
        Boop,

        /// <summary>
        ///     When Ralsei is called cute.
        /// </summary>
        CalledCute,

        /// <summary>
        ///     When a user is kicked.
        /// </summary>
        Kicked,

        /// <summary>
        ///     When a user is banned.
        /// </summary>
        Banned,

        /// <summary>
        ///     When messages are purged.
        /// </summary>
        RuleViolation
    }

    /// <summary>
    ///     All the responses that the system can give.
    /// </summary>
    private readonly LinkedList<Dictionary<ResponseTypes, List<string>>> _responses = [];

    public ResponseSystemHandler(ILogger<ResponseSystemHandler> logger)
    {
        // Import the responses that are needed.
        foreach (var directory in Directory.GetFiles("./ResponseResources"))
        {
            var importedFile = File.ReadAllText(directory);
            var deserializedValue = JsonSerializer.Deserialize<Dictionary<ResponseTypes, List<string>>>(importedFile);

            if (deserializedValue != null) _responses.AddFirst(deserializedValue);
        }

        logger.LogInformation("Finished registering all the responses.");
    }

    /// <summary>
    ///     Attempt to get a random response with a specified response type.
    /// </summary>
    /// <param name="responseTypes"></param>
    /// <returns></returns>
    public string GetRandomResponse(ResponseTypes responseTypes)
    {
        // This is the most over-engineered pile of code in this repository.
        // 1. It retrieves all the possible response trees from the files, after retrieving the responses,
        // 2. It selects a random file containing all eligible response types. After selecting a file with a list of responses,
        // 3. It selects a random response from the file that matches the current response type since it was filtered.
        // It's a very flexible system, but GOD is it complicated!

        // Thanks LinQ for existing.

        var allResponses =
            _responses.Select(dictionary =>
                    dictionary.Where(pair => pair.Key == responseTypes)
                        .Select(pair => pair.Value)
                        .ToList()
                )
                .ElementAt(Random.Shared.Next(0, _responses.Count));

        var randomPickedResponseTree = allResponses[Random.Shared.Next(0, allResponses.Count)];
        return randomPickedResponseTree[Random.Shared.Next(0, randomPickedResponseTree.Count)];
    }
}