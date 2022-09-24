using Songhay.Extensions;
using Songhay.Models;

namespace Songhay.Social.Extensions;

public static class ProgramArgsExtensions
{
    public static string GetJson(this ProgramArgs args, string argJson, string argJsonFile)
    { //TODO: move to Core
        string json = null;

        if (args.HasArg(argJson, requiresValue: false))
        {
            json = args.GetArgValue(argJson);
        }
        else if (args.HasArg(argJsonFile, requiresValue: true))
        {
            var path = args.GetArgValue(argJsonFile);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"The expected JSON file, `{path ?? "[null]"}`, is not here.");
            }

            json = File.ReadAllText(path);
        }

        return json;
    }
}