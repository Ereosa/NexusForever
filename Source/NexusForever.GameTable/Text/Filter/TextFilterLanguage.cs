﻿using System.Collections.Immutable;
using System.Text.RegularExpressions;
using NexusForever.Game.Static;
using NexusForever.GameTable.Model;
using NexusForever.GameTable.Text.Static;

namespace NexusForever.GameTable.Text.Filter
{
    public class TextFilterLanguage : ITextFilterLanguage
    {
        /// <summary>
        /// <see cref="WordFilterEntry.WordFilterTypeEnum"/> of entries that only match a whole word.
        /// </summary>
        /// <remarks>
        /// Nearly every entry is type 0 and many are short words that are also parts of normal names ("nig" in "Nightsong", "anus" in "Janus"),
        /// the few entries that must be caught anywhere in the text ("fuck", "nigger") are types 3 and 4.
        /// </remarks>
        private const uint WholeWordFilterType = 0u;

        public Language Language { get; }

        private readonly ImmutableDictionary<TextFilterClass, Regex> filters;

        /// <summary>
        /// Create a new <see cref="ITextFilterLanguage"/> class with the specified <see cref="Static.Language"/> and collection of <see cref="WordFilterEntry"/>'s.
        /// </summary>
        public TextFilterLanguage(Language language, List<WordFilterEntry> entries)
        {
            Language = language;

            filters = entries
                .Select(e => e.UserTextFilterClassEnum)
                .Distinct()
                .ToImmutableDictionary(k => (TextFilterClass)k, k => BuildFilter(entries
                    .Where(e => e.UserTextFilterClassEnum >= k && !string.IsNullOrEmpty(e.Filter))));
        }

        private static Regex BuildFilter(IEnumerable<WordFilterEntry> entries)
        {
            IEnumerable<string> patterns = entries
                .Select(e => e.WordFilterTypeEnum == WholeWordFilterType
                    ? $@"(?<![\p{{L}}\p{{N}}]){Regex.Escape(e.Filter)}(?![\p{{L}}\p{{N}}])"
                    : Regex.Escape(e.Filter));

            return new Regex(string.Join("|", patterns), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        }

        /// <summary>
        /// Returns if supplied string meets <see cref="TextFilterClass"/> filtering.
        /// </summary>
        public bool IsTextValid(string text, TextFilterClass filterClass)
        {
            if (!filters.TryGetValue(filterClass, out Regex filter))
                return false;

            return !filter.IsMatch(text);
        }
    }
}
