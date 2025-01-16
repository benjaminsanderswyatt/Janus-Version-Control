﻿namespace Janus.Models
{
    public class CommitMetadata
    {
        public string Commit {  get; set; }
        public string Parent {  get; set; }
        public string Branch { get; set; }
        public string Author { get; set; }
        public DateTimeOffset Date {  get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> Files { get; set; }

    }
}
