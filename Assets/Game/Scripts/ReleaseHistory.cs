using System;
using System.Collections.Generic;
using System.Text;

// A deliberately small, bounded Markdown subset for compact release summaries.
// Full Markdown (including tables, images and code) remains available on GitHub.
public static class ReleaseHistory
{
    public struct Block
    {
        public string Text;
        public bool Heading;
        public Block(string text, bool heading) { Text = text; Heading = heading; }
    }
    public const int MaxReleases = 100;
    public const int MaxBlocks = 120;
    const int MaxCharacters = 32768;

    public static bool IsVersionHeading(string heading, string version)
    {
        if (string.IsNullOrEmpty(version)) return false;
        heading = (heading ?? "").TrimStart('v');
        version = version.TrimStart('v');
        return heading == version || heading.StartsWith(version + " (", StringComparison.Ordinal);
    }

    public static Block[] Parse(string markdown)
    {
        if (string.IsNullOrEmpty(markdown)) return new Block[0];
        markdown = markdown.Substring(0, Math.Min(markdown.Length, MaxCharacters));
        var blocks = new List<Block>();
        bool downloads = false, fenced = false;
        foreach (var raw in markdown.Replace("\r", "").Split('\n')) {
            string line = raw.Trim();
            int level = 0;
            while (level < line.Length && line[level] == '#') level++;
            bool heading = level > 0 && level <= 6 && level < line.Length && line[level] == ' ';
            if (heading) {
                string title = Inline(line.Substring(level).Trim().TrimEnd('#').TrimEnd());
                if (title == "Download LegendOfCat") { downloads = true; continue; }
                // semantic-release starts the following changelog with a level-one version heading.
                downloads = false;
                line = title;
            }
            if (downloads) continue;
            if (line.StartsWith("```", StringComparison.Ordinal) || line.StartsWith("~~~", StringComparison.Ordinal)) {
                fenced = !fenced; continue;
            }
            if (fenced || line.Length == 0 || line.StartsWith("<!--", StringComparison.Ordinal) ||
                line == "---" || line == "***" || line.StartsWith("|", StringComparison.Ordinal)) continue;
            // Reference definitions are metadata; inline references keep only their label.
            if (line[0] == '[' && line.IndexOf("]:", StringComparison.Ordinal) > 0) continue;
            while (line.StartsWith(">", StringComparison.Ordinal)) line = line.Substring(1).TrimStart();
            bool list = line.StartsWith("- ", StringComparison.Ordinal) || line.StartsWith("* ", StringComparison.Ordinal) || line.StartsWith("+ ", StringComparison.Ordinal);
            if (list) line = line.Substring(2);
            else {
                int n = 0;
                while (n < line.Length && char.IsDigit(line[n])) n++;
                if (n > 0 && n + 1 < line.Length && (line[n] == '.' || line[n] == ')') && line[n + 1] == ' ') {
                    list = true; line = line.Substring(n + 2);
                }
            }
            if (line.StartsWith("[x] ", StringComparison.OrdinalIgnoreCase) || line.StartsWith("[ ] ", StringComparison.Ordinal)) line = line.Substring(4);
            line = Inline(line).Trim();
            if (line.Length == 0) continue;
            if (blocks.Count == MaxBlocks) { blocks.Add(new Block("More details on GitHub.", false)); break; }
            blocks.Add(new Block((list ? "• " : "") + line, heading));
        }
        return blocks.ToArray();
    }

    static bool IsHash(string value)
    {
        if (value.Length < 7 || value.Length > 40) return false;
        foreach (char c in value) if (!Uri.IsHexDigit(c)) return false;
        return true;
    }

    static bool IsIssueReference(string value)
    {
        if (value.Length < 2 || value[0] != '#') return false;
        for (int i = 1; i < value.Length; i++) if (!char.IsDigit(value[i])) return false;
        return true;
    }

    public static string Inline(string text)
    {
        var output = new StringBuilder();
        for (int i = 0; i < text.Length; i++) {
            char c = text[i];
            if (c == '\\' && i + 1 < text.Length) { output.Append(text[++i]); continue; }
            if (c == '!' && i + 1 < text.Length && text[i + 1] == '[') continue;
            if (c == '[') {
                int close = text.IndexOf(']', i + 1);
                if (close >= 0) {
                    string label = text.Substring(i + 1, close - i - 1);
                    if (!IsHash(label) && !IsIssueReference(label)) output.Append(label.Replace("*", "").Replace("`", "").Replace("_", ""));
                    i = close;
                    if (i + 1 < text.Length && text[i + 1] == '(') {
                        int depth = 1; i += 2;
                        while (i < text.Length && depth > 0) {
                            if (text[i] == '(') depth++;
                            if (text[i] == ')') depth--;
                            if (depth > 0) i++;
                        }
                    } else if (i + 1 < text.Length && text[i + 1] == '[') {
                        int end = text.IndexOf(']', i + 2);
                        if (end >= 0) i = end;
                    }
                    continue;
                }
            }
            if (c == '<') {
                int close = text.IndexOf('>', i + 1);
                if (close >= 0) { i = close; continue; }
            }
            if ((i + 8 <= text.Length && string.Compare(text, i, "https://", 0, 8, StringComparison.OrdinalIgnoreCase) == 0) ||
                (i + 7 <= text.Length && string.Compare(text, i, "http://", 0, 7, StringComparison.OrdinalIgnoreCase) == 0)) {
                while (i + 1 < text.Length && !char.IsWhiteSpace(text[i + 1])) i++;
                continue;
            }
            if (c == '(') {
                int close = text.IndexOf(')', i + 1);
                if (close >= 0 && IsHash(text.Substring(i + 1, close - i - 1))) { i = close; continue; }
            }
            if (Uri.IsHexDigit(c) && (i == 0 || !char.IsLetterOrDigit(text[i - 1]))) {
                int end = i;
                while (end < text.Length && Uri.IsHexDigit(text[end])) end++;
                if ((end == text.Length || !char.IsLetterOrDigit(text[end])) && IsHash(text.Substring(i, end - i))) {
                    i = end - 1; continue;
                }
            }
            if (c == '*' || c == '`' || c == '~') continue;
            if (c == '_' && (i == 0 || !char.IsLetterOrDigit(text[i - 1]) || i + 1 == text.Length || !char.IsLetterOrDigit(text[i + 1]))) continue;
            output.Append(c);
        }
        return output.ToString().Replace("()", "").Trim().Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&#39;", "'");
    }
}
