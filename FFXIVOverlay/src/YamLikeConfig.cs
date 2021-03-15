using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using FFXIVOverlay;

namespace YamLikeConfig
{
    public class Command
    {
        public Command()
        {
            this.Parent = null;

            this.Params = null;
            this.UnnamedParams = null;

            this.SubCommand = null;
            this.NormalDepth = 0;
        }

        public Command Parent { get; set; }
        public string Name { get; set; }
        public int Depth { get; set; }
        public int NormalDepth { get; set; }
        public Dictionary<string, string> Params { get; set; }
        public List<string> UnnamedParams { get; set; }

        public List<Command> SubCommand { get; set; }

        public bool IsRoot { get { return Parent == null; } }

        public void AddSubcommand(Command cmd)
        {
            if (this.SubCommand == null)
            {
                this.SubCommand = new List<Command>();
            }

            this.SubCommand.Add(cmd);
        }

        public string this[int i]
        {
            get => value(i, string.Empty);
        }

        public string this[string key]
        {
            get => value(key, string.Empty);
        }

        public string value(int i, string defaultValue = "")
        {
            if (UnnamedParams == null)
                return defaultValue;

            if (UnnamedParams.Count > i && i >= 0)
            {
                return UnnamedParams[i];
            }

            return defaultValue;
        }

        public string value(string key, string defaultValue = "")
        {
            if (Params == null)
                return defaultValue;

            if (Params.ContainsKey(key))
            {
                return Params[key];
            }

            return defaultValue;
        }
        public bool has(string key)
        {
            if (Params == null)
                return false;

            return Params.ContainsKey(key);
        }

        public bool has(int index)
        {
            if (UnnamedParams == null)
                return false;

            return index >= 0 && index < UnnamedParams.Count;
        }

        public bool tryGet(string key, out float value)
        {
            value = 0;

            if (!has(key))
                return false;

            float tmp = 0;
            if (float.TryParse(this[key], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
            {
                value = tmp;
                return true;
            }

            return false;
        }

        public bool tryGet(string key, out System.Drawing.Color value)
        {
            value = System.Drawing.Color.White;

            if (!has(key))
                return false;

            System.Drawing.Color tmpColor = System.Drawing.Color.FromArgb(0, 50, 137, 37);

            System.Drawing.Color parseResult = this[key].ParseColor(tmpColor);
            if (parseResult == tmpColor)
            {
                return false;
            }

            value = parseResult;
            return true;
        }

        public bool tryGet(string key, out uint value)
        {
            value = 0;

            if (!has(key))
                return false;

            uint tmp = 0;
            if (uint.TryParse(this[key], out tmp))
            {
                value = tmp;
                return true;
            }

            return false;
        }
    }

    public class Document
    {
        public Document()
        {
            this.TabSize = 2;
            Commands = new List<Command>();
            RawCommands = new List<Command>();

            Params = null;
            UnnamedParams = null;
        }

        public string Name { get; set; }
        public int TabSize { get; set; }

        public List<Command> Commands { get; }

        public List<Command> RawCommands { get; }
        public Dictionary<string, string> Params { get; set; }
        public List<string> UnnamedParams { get; set; }

        public string this[int i]
        {
            get => value(i, string.Empty);
        }

        public string this[string key]
        {
            get => value(key, string.Empty);
        }

        public string value(int i, string defaultValue = "")
        {
            if (UnnamedParams == null)
                return defaultValue;

            if (UnnamedParams.Count > i && i >= 0)
            {
                return UnnamedParams[i];
            }

            return defaultValue;
        }

        public string value(string key, string defaultValue = "")
        {
            if (Params == null)
                return defaultValue;

            if (Params.ContainsKey(key))
            {
                return Params[key];
            }

            return defaultValue;
        }

        public bool has(string key)
        {
            if (Params == null)
                return false;

            return Params.ContainsKey(key);
        }

        public bool has(int index)
        {
            if (UnnamedParams == null)
                return false;

            return index >= 0 && index < UnnamedParams.Count;
        }
    }

    public class ConfigParser
    {
        public ConfigParser()
        {
            this.Result = new List<Document>();
        }
        public List<Document> Result { get; }

        public bool parse(string fileName)
        {
            Document currentDoc = null;
            Command prevCmd = null;

            using (StreamReader sr = File.OpenText(fileName))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    int prefixSpaceCount = 0;
                    int prefixTabCount = 0;
                    string commandName;
                    Dictionary<string, string> commandParams = null;
                    List<string> unnamedParams = null;

                    parseLine(s, out prefixSpaceCount, out prefixTabCount, out commandName, out commandParams, out unnamedParams);
                    if (string.IsNullOrEmpty(commandName))
                        continue;


                    if (commandName == "---")
                    {
                        // StartNewDocument command.
                        currentDoc = new Document();
                        currentDoc.UnnamedParams = unnamedParams;
                        currentDoc.Params = commandParams;

                        prevCmd = null;
                        Result.Add(currentDoc);

                        if (unnamedParams != null && unnamedParams.Count >= 1)
                        {
                            currentDoc.Name = unnamedParams[0];
                        }

                        if (commandParams != null)
                        {
                            if (commandParams.ContainsKey("name"))
                            {
                                currentDoc.Name = commandParams["name"];
                            }

                            if (commandParams.ContainsKey("tab"))
                            {
                                int tabSize = 2;
                                if (int.TryParse(commandParams["tab"], out tabSize))
                                {
                                    currentDoc.TabSize = tabSize;
                                }
                            }
                        }

                        continue;
                    }

                    if (currentDoc == null)
                    {
                        currentDoc = new Document();
                        Result.Add(currentDoc);
                    }

                    int currentDepth = prefixSpaceCount + prefixTabCount * currentDoc.TabSize;
                    Command cmd = new Command
                    {
                        Depth = currentDepth,
                        Name = commandName,
                        Params = commandParams,
                        UnnamedParams = unnamedParams
                    };

                    currentDoc.RawCommands.Add(cmd);

                    if (prevCmd == null)
                    {
                        currentDoc.Commands.Add(cmd);
                        prevCmd = cmd;
                        continue;
                    }

                    if (currentDepth > prevCmd.Depth)
                    {
                        cmd.Parent = prevCmd;
                        cmd.NormalDepth = prevCmd.NormalDepth + 1;
                        prevCmd.AddSubcommand(cmd);
                        prevCmd = cmd;
                        continue;
                    }

                    Command realParent = prevCmd;
                    while (realParent != null && realParent.Depth >= currentDepth)
                    {
                        realParent = realParent.Parent;
                    }

                    if (realParent == null)
                    {
                        currentDoc.Commands.Add(cmd);
                        prevCmd = cmd;
                        continue;
                    }

                    cmd.Parent = realParent;
                    cmd.NormalDepth = realParent.NormalDepth + 1;
                    realParent.AddSubcommand(cmd);
                    prevCmd = cmd;
                }
            }
            return true;
        }

        void internalAddToList(ref List<string> l, string s)
        {
            if (l == null)
            {
                l = new List<string>();
            }

            l.Add(s);
        }

        void internalAddToDictionary(ref Dictionary<string, string> d, string k, string v)
        {
            if (d == null)
            {
                d = new Dictionary<string, string>();
            }

            d[k] = v;
        }

        void parseLine(string line, out int prefixSpaceCount, out int prefixTabCount, out string commandName, out Dictionary<string, string> commandParams, out List<string> unnamedParams)
        {
            prefixSpaceCount = 0;
            prefixTabCount = 0;
            commandName = string.Empty;
            commandParams = null;
            unnamedParams = null;

            if (String.IsNullOrWhiteSpace(line)) // skip empty strings;
                return;

            int index = 0;
            char c = line[index];

            bool endOfLine = false;

            // parse indentation
            while (c == ' ' || c == '\t')
            {
                if (c == ' ') prefixSpaceCount++;
                if (c == '\t') prefixTabCount++;
                index++;

                if (index >= line.Length)
                {
                    endOfLine = true;
                    break;
                }


                c = line[index];
            }

            if (endOfLine)
                return;

            bool commentStarted = false;
            int cmdStart = index;
            while (!Char.IsWhiteSpace(c))
            {
                if (c == '#')
                {
                    commentStarted = true;
                    break;
                }

                index++;
                if (index >= line.Length)
                {
                    endOfLine = true;
                    break;
                }

                c = line[index];
            }

            commandName = line.Substring(cmdStart, index - cmdStart);
            if (endOfLine || commentStarted)
                return;

            while (index < line.Length)
            {
                c = line[index];
                if (Char.IsWhiteSpace(c))
                {
                    index++;
                    continue;
                }

                if (c == '#')
                    break;

                if (c == '\'' || c == '"' || c == '`')
                {
                    // quote string - it can't be KeyValue param, so it's unnamed param, skip till same quote or endOfLine
                    char startQuote = c;
                    if ((index + 1) >= line.Length)
                        break;

                    index++;
                    c = line[index];

                    int stringStart = index;
                    while (c != startQuote)
                    {
                        index++;
                        if (index >= line.Length)
                        {
                            endOfLine = true;
                            break;
                        }

                        c = line[index];
                    }

                    if (endOfLine)
                    {
                        break;
                    }

                    string unnameArg = line.Substring(stringStart, index - stringStart);
                    internalAddToList(ref unnamedParams, unnameArg);

                    index++; // move from end quote to next char
                    continue;
                }

                // get Key=Value pair.

                int startIndex = index;
                bool isPair = false;
                index++;

                while (index < line.Length)
                {
                    c = line[index];
                    if (c == '=')
                    {
                        isPair = true;
                        break;
                    }

                    if (c == '#')
                    {
                        break;
                    }

                    if (Char.IsWhiteSpace(c))
                    {
                        break;
                    }

                    index++;
                }

                string token = line.Substring(startIndex, index - startIndex);
                if (index >= line.Length || !isPair)
                {
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        internalAddToList(ref unnamedParams, token);
                    }

                    continue;
                }

                // token=...
                //      ^-index
                index++;
                if (index >= line.Length)
                {
                    internalAddToDictionary(ref commandParams, token, string.Empty);
                    continue;
                }

                c = line[index];
                if (c == '#')
                {
                    internalAddToDictionary(ref commandParams, token, string.Empty);
                    continue;
                }

                if (c == '\'' || c == '"' || c == '`')
                {
                    // pair value is in quote. ex: token='tokenValue'
                    char startQuote = c;
                    if ((index + 1) >= line.Length)
                    {
                        // bad value, end of string, exit from function;
                        break;
                    }

                    index++;
                    c = line[index];

                    startIndex = index;
                    while (c != startQuote)
                    {
                        index++;
                        if (index >= line.Length)
                        {
                            endOfLine = true;
                            break;
                        }

                        c = line[index];
                    }

                    if (endOfLine)
                    {
                        // bad value, unfinished quoted string ignore parametr
                        break;
                    }

                    string tokenValue = line.Substring(startIndex, index - startIndex);
                    internalAddToDictionary(ref commandParams, token, tokenValue);

                    index++; // move from end quote to next char
                    continue;
                }
                else
                {
                    // pair value is unquoted. ex: token=tokenValue qweqwe qwe qwe
                    startIndex = index;

                    while (!Char.IsWhiteSpace(c))
                    {
                        if (c == '#')
                        {
                            commentStarted = true;
                            break;
                        }

                        index++;
                        if (index >= line.Length)
                        {
                            endOfLine = true;
                            break;
                        }

                        c = line[index];
                    }

                    string tokenValue = line.Substring(startIndex, index - startIndex);
                    internalAddToDictionary(ref commandParams, token, tokenValue);

                    index++; // move from end quote to next char
                    continue;
                }
            }
        }
    }

    public class Helper
    {
        public static string Dump(Document doc, int intendantSize = 2)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("--- `{0}` tab={1}\n\n", doc.Name, doc.TabSize);

            //Console.WriteLine("--- `{0}` tab={1}", doc.Name, doc.TabSize);
            if (doc.Commands == null || doc.Commands.Count == 0)
                return sb.ToString();


            foreach (Command cmd in doc.RawCommands)
            {
                string prefix = cmd.NormalDepth == 0 ? string.Empty : new string(' ', cmd.NormalDepth * intendantSize);
                //StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0}{1} ", prefix, cmd.Name);
                if (cmd.UnnamedParams != null)
                {
                    foreach (var p in cmd.UnnamedParams)
                    {
                        if (string.IsNullOrWhiteSpace(p) || p.Length == 0)
                        {
                            sb.AppendFormat("'' ");
                            continue;
                        }

                        char x = p[0];
                        bool hasWhiteSpace = p.Any(x1 => Char.IsWhiteSpace(x1));
                        bool firstQoute = x == '\'' || x == '"' || x == '`';
                        if (!hasWhiteSpace && !firstQoute)
                        {
                            sb.AppendFormat("{0} ", p);
                            continue;
                        }

                        // ` ' "
                        char selectedQoute = '-';

                        if (!p.Any(x1 => x1 == '`'))
                        {
                            selectedQoute = '`';
                        }
                        else if (!p.Any(x1 => x1 == '\''))
                        {
                            selectedQoute = '\'';
                        }
                        else if (!p.Any(x1 => x1 == '"'))
                        {
                            selectedQoute = '"';
                        }

                        if (selectedQoute == '-')
                        {
                            // fail
                            sb.AppendFormat("<FailArg[{0}]/> ", p);
                            continue;
                        }

                        sb.AppendFormat("{0}{1}{0} ", selectedQoute, p);
                    }
                }

                if (cmd.Params != null)
                {
                    foreach (var kv in cmd.Params)
                    {
                        sb.AppendFormat("{0}=", kv.Key);

                        if (string.IsNullOrWhiteSpace(kv.Value) || kv.Value.Length == 0)
                        {
                            sb.AppendFormat(" ");
                            continue;
                        }

                        string p = kv.Value;

                        char x = p[0];
                        bool hasWhiteSpace = p.Any(x1 => Char.IsWhiteSpace(x1));
                        bool firstQoute = x == '\'' || x == '"' || x == '`';
                        if (!hasWhiteSpace && !firstQoute)
                        {
                            sb.AppendFormat("{0} ", p);
                            continue;
                        }

                        // ` ' "
                        char selectedQoute = '-';

                        if (!p.Any(x1 => x1 == '`'))
                        {
                            selectedQoute = '`';
                        }
                        else if (!p.Any(x1 => x1 == '\''))
                        {
                            selectedQoute = '\'';
                        }
                        else if (!p.Any(x1 => x1 == '"'))
                        {
                            selectedQoute = '"';
                        }

                        if (selectedQoute == '-')
                        {
                            // fail
                            sb.AppendFormat("<FailArg[{0}]/> ", p);
                            continue;
                        }

                        sb.AppendFormat("{0}{1}{0} ", selectedQoute, p);
                    }
                }

                sb.AppendLine();
                //Console.WriteLine(sb.ToString());
            }

            //Console.WriteLine();
            sb.AppendLine();

            return sb.ToString();
        }
    }
}