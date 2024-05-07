/********************************************************************

•   File: IniFile.cs

•   Description.

    IniFile is a regular expression based INI file content analyzer.
    This implementation  only supports  reading keys, since data is 
    usually entered into  ini  files manually using  any text editor.

********************************************************************/

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace System.IO
{
    internal class IniFile
    {
        private const string COMMENT_PATTERN = @"(?:(?<=;|#)(?:[ \t]*))(?<comment>.+)(?<=\S)";
        private const string SECTION_PATTERN = @"(?:[ \t]*)(?<=\[)(?:[ \t]*)(?<section>\w+)(?:[ \t]*?)(?=\])";
        private const string ENTRY_PATTERN = @"(?<entry>(?=\S)(?<key>\w+)(?:[ \t]*)(?==)=(?<==)(?:[ \t]*)(?<value>.*)(?<=\S))";
        private const StringComparison CMP = StringComparison.InvariantCultureIgnoreCase;
        private static readonly Regex _regex = new Regex($"{COMMENT_PATTERN}|{SECTION_PATTERN}|{ENTRY_PATTERN}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private readonly MatchCollection _matches;

        public IniFile(TextReader reader)
        {
            _matches = _regex.Matches(reader.ReadToEnd());
        }

        public IniFile(Stream stream, Encoding encoding)
        {
            using (StreamReader reader = new StreamReader(stream, encoding))
            {
                _matches = _regex.Matches(reader.ReadToEnd());
            }
        }

        public IniFile(string fileName, Encoding encoding = null)
        {
            string content = File.ReadAllText(fileName, encoding ?? Encoding.UTF8);
            _matches = _regex.Matches(content);
        }

        private bool CheckSection(Match match, string section, ref string currentSection)
        {
            Group sectionGroup = match.Groups["section"];
            if (sectionGroup.Success)
            {
                currentSection = sectionGroup.Value;
            }

            return currentSection.Equals(section, CMP);
        }

        // Returns a single entry specified by section and key,
        // or a default value if no entry is found.
        public string GetEntry(string section, string key, string defaultValue = null)
        {
            if (key == null) key = string.Empty;
            if (section == null) section = string.Empty;
            string currentSection = string.Empty;
            for (var i = 0; i < _matches.Count; i++)
            {
                var match = _matches[i];
                if (!CheckSection(match, section, ref currentSection))
                {
                    continue;
                }

                Group keyGroup = match.Groups["key"];
                Group valueGroup = match.Groups["value"];
                if (keyGroup.Success && keyGroup.Value.Equals(key, CMP))
                {
                    return valueGroup.Value;
                }
            }

            return defaultValue ?? string.Empty;
        }

        // Returns all entries contained in the section.
        // If no entry is found, an empty enumerator will be returned.
        public IEnumerable<string> GetEntries(string section)
        {
            IList<string> entries = new List<string>();
            string currentSection = string.Empty;
            for (var i = 0; i < _matches.Count; i++)
            {
                var match = _matches[i];
                if (!CheckSection(match, section, ref currentSection))
                {
                    continue;
                }

                Group keyGroup = match.Groups["key"];
                Group valueGroup = match.Groups["value"];
                if (keyGroup.Success)
                {
                    entries.Add(valueGroup.Value);
                }
            }

            return entries;
        }

        // Returns all entries matching the specified key contained in the section.
        // If no entry is found, an empty enumerator will be returned.
        public IEnumerable<string> GetEntries(string section, string key)
        {
            IList<string> entries = new List<string>();
            string currentSection = string.Empty;
            for (var i = 0; i < _matches.Count; i++)
            {
                var match = _matches[i];
                if (!CheckSection(match, section, ref currentSection))
                {
                    continue;
                }

                Group keyGroup = match.Groups["key"];
                Group valueGroup = match.Groups["value"];
                if (keyGroup.Success && keyGroup.Value.Equals(key, CMP))
                {
                    entries.Add(valueGroup.Value);
                }
            }

            return entries;
        }

        public static string GetEntry(string fileName, string section, string key, string defaultValue = null)
        {
            return new IniFile(fileName).GetEntry(section, key, defaultValue ?? string.Empty);
        }

        public static string GetEntry(string fileName, Encoding encoding, string section, string key, string defaultValue = null)
        {
            return new IniFile(fileName, encoding).GetEntry(section, key, defaultValue ?? string.Empty);
        }

        public static IEnumerable<string> GetEntries(string fileName, string section, string key = null)
        {
            IniFile iniFile = new IniFile(fileName);
            return key.IsNullOrEmpty()
                ? iniFile.GetEntries(section)
                : iniFile.GetEntries(section, key);
        }

        public static IEnumerable<string> GetEntries(string fileName, Encoding encoding, string section, string key = null)
        {
            IniFile iniFile = new IniFile(fileName, encoding);
            return key.IsNullOrEmpty()
                ? iniFile.GetEntries(section)
                : iniFile.GetEntries(section, key);
        }
    }
}
